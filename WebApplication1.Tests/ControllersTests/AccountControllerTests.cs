using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using WebApplication1.Controllers;
using WebApplication1.DataInfrastructure;
using WebApplication1.Models;
using WebApplication1.Models.Entities;
using Xunit;

namespace WebApplication1.Tests.Controllers
{
    public class AccountControllerTests
    {
        // Simple in-memory TempData provider for tests
        private class TestTempDataProvider : ITempDataProvider
        {
            private readonly Dictionary<string, object?> _data = new();

            public IDictionary<string, object?> LoadTempData(HttpContext context) => _data;

            public void SaveTempData(HttpContext context, IDictionary<string, object?> values)
            {
                _data.Clear();
                foreach (var kvp in values)
                {
                    _data[kvp.Key] = kvp.Value;
                }
            }
        }

        // Fake URL helper so RedirectToAction / Url.Action does not crash
        private class FakeUrlHelper : IUrlHelper
        {
            public ActionContext ActionContext { get; }

            public FakeUrlHelper()
            {
                ActionContext = new ActionContext();
            }

            public string Action(UrlActionContext actionContext) => "/fake";
            public string Action(string action, string controller, object values, string protocol) => "/fake";
            public string Content(string contentPath) => contentPath;
            public bool IsLocalUrl(string url) => true;
            public string Link(string routeName, object values) => "/fake";
            public string RouteUrl(UrlRouteContext routeContext) => "/fake";
            public string RouteUrl(string routeName, object values, string protocol, string host, string fragment) => "/fake";
        }

        // Fake authentication service to avoid real cookies
        private class FakeAuthenticationService : IAuthenticationService
        {
            public string? LastScheme { get; private set; }
            public ClaimsPrincipal? LastPrincipal { get; private set; }
            public AuthenticationProperties? LastProperties { get; private set; }
            public bool SignOutCalled { get; private set; }

            public Task<AuthenticateResult> AuthenticateAsync(HttpContext context, string scheme)
                => Task.FromResult(AuthenticateResult.NoResult());

            public Task ChallengeAsync(HttpContext context, string scheme, AuthenticationProperties? properties)
                => Task.CompletedTask;

            public Task ForbidAsync(HttpContext context, string scheme, AuthenticationProperties? properties)
                => Task.CompletedTask;

            public Task SignInAsync(HttpContext context, string scheme, ClaimsPrincipal principal, AuthenticationProperties? properties)
            {
                LastScheme = scheme;
                LastPrincipal = principal;
                LastProperties = properties;
                return Task.CompletedTask;
            }

            public Task SignOutAsync(HttpContext context, string scheme, AuthenticationProperties? properties)
            {
                SignOutCalled = true;
                return Task.CompletedTask;
            }
        }

        private static ApplicationDbContext CreateInMemoryDbContext(string dbName)
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(dbName)
                .Options;

            return new ApplicationDbContext(options);
        }

        private static AccountController CreateController(
            ApplicationDbContext db,
            ClaimsPrincipal? user = null,
            bool withTempData = true,
            FakeAuthenticationService? authService = null)
        {
            var httpContext = new DefaultHttpContext();

            if (user != null)
            {
                httpContext.User = user;
            }

            var services = new ServiceCollection();
            if (authService != null)
            {
                services.AddSingleton<IAuthenticationService>(authService);
            }
            httpContext.RequestServices = services.BuildServiceProvider();

            var controller = new AccountController(db)
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = httpContext
                },
                TempData = withTempData
                    ? new TempDataDictionary(httpContext, new TestTempDataProvider())
                    : null
            };

            // Make sure Url is not null
            controller.Url = new FakeUrlHelper();

            return controller;
        }

        [Fact]
        public void Login_Get_AuthenticatedUser_RedirectsToProfile()
        {
            var db = CreateInMemoryDbContext(nameof(Login_Get_AuthenticatedUser_RedirectsToProfile));

            var claims = new[]
            {
                new Claim(ClaimTypes.Name, "user@test.local"),
                new Claim(ClaimTypes.Email, "user@test.local")
            };
            var identity = new ClaimsIdentity(claims, "TestAuth");
            var principal = new ClaimsPrincipal(identity);

            var controller = CreateController(db, principal);

            string? returnUrl = null;
            var result = controller.Login(returnUrl);

            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("UserProfile", redirect.ActionName);
            Assert.Equal("User", redirect.ControllerName);
            Assert.Equal("user@test.local", redirect.RouteValues?["email"]);
        }

        [Fact]
        public void Login_Get_AnonymousUser_ReturnsViewWithModel()
        {
            var db = CreateInMemoryDbContext(nameof(Login_Get_AnonymousUser_ReturnsViewWithModel));
            var controller = CreateController(db);

            var result = controller.Login("/Reports/Index");

            var view = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<LoginViewModel>(view.Model);
            Assert.Equal("/Reports/Index", model.ReturnUrl);
        }

        [Fact]
        public async Task Login_Post_InvalidModel_ReturnsViewWithSameModel()
        {
            var db = CreateInMemoryDbContext(nameof(Login_Post_InvalidModel_ReturnsViewWithSameModel));
            var controller = CreateController(db);
            controller.ModelState.AddModelError("Email", "Required");

            var model = new LoginViewModel
            {
                Email = null,
                Password = "whatever"
            };

            var result = await controller.Login(model);

            var view = Assert.IsType<ViewResult>(result);
            Assert.Same(model, view.Model);
        }

        [Fact]
        public async Task Login_Post_UserNotFound_AddsModelErrorAndReturnsView()
        {
            var db = CreateInMemoryDbContext(nameof(Login_Post_UserNotFound_AddsModelErrorAndReturnsView));
            var controller = CreateController(db);

            var model = new LoginViewModel
            {
                Email = "unknown@test.local",
                Password = "Password123!"
            };

            var result = await controller.Login(model);

            var view = Assert.IsType<ViewResult>(result);
            Assert.Same(model, view.Model);
            Assert.True(controller.ModelState.ContainsKey(string.Empty));
        }

        [Fact]
        public async Task Login_Post_InvalidPassword_AddsModelErrorAndReturnsView()
        {
            var db = CreateInMemoryDbContext(nameof(Login_Post_InvalidPassword_AddsModelErrorAndReturnsView));
            var passwordHasher = new PasswordHasher<UserEntity>();

            var user = new UserEntity
            {
                UserID = 1,
                Email = "user@test.local",
                Role = "Pilot"
            };
            user.PasswordHash = passwordHasher.HashPassword(user, "CorrectPassword!");

            db.Users.Add(user);
            db.SaveChanges();

            var controller = CreateController(db);

            var model = new LoginViewModel
            {
                Email = "user@test.local",
                Password = "WrongPassword!"
            };

            var result = await controller.Login(model);

            var view = Assert.IsType<ViewResult>(result);
            Assert.Same(model, view.Model);
            Assert.True(controller.ModelState.ContainsKey(string.Empty));
        }

        [Fact]
        public async Task Login_Post_ValidCredentials_SignsInAndRedirectsToProfile()
        {
            var db = CreateInMemoryDbContext(nameof(Login_Post_ValidCredentials_SignsInAndRedirectsToProfile));
            var passwordHasher = new PasswordHasher<UserEntity>();

            var user = new UserEntity
            {
                UserID = 1,
                Email = "user@test.local",
                Role = "Pilot"
            };
            user.PasswordHash = passwordHasher.HashPassword(user, "CorrectPassword!");

            db.Users.Add(user);
            db.SaveChanges();

            var authService = new FakeAuthenticationService();
            var controller = CreateController(db, user: null, authService: authService);

            var model = new LoginViewModel
            {
                Email = "user@test.local",
                Password = "CorrectPassword!",
                RememberMe = true,
                ReturnUrl = null
            };

            var result = await controller.Login(model);

            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("UserProfile", redirect.ActionName);
            Assert.Equal("User", redirect.ControllerName);
            Assert.Equal("user@test.local", redirect.RouteValues?["email"]);

            Assert.Equal(CookieAuthenticationDefaults.AuthenticationScheme, authService.LastScheme);
            Assert.NotNull(authService.LastPrincipal);
            Assert.True(authService.LastProperties?.IsPersistent ?? false);
        }

        [Fact]
        public async Task Logout_SignsOutAndRedirectsToHomeIndex()
        {
            var db = CreateInMemoryDbContext(nameof(Logout_SignsOutAndRedirectsToHomeIndex));
            var authService = new FakeAuthenticationService();

            var claims = new[]
            {
                new Claim(ClaimTypes.Name, "user@test.local"),
            };
            var identity = new ClaimsIdentity(claims, "TestAuth");
            var principal = new ClaimsPrincipal(identity);

            // ⬅️ IMPORTANT: pass authService into CreateController so it gets registered
            var controller = CreateController(db, user: principal, authService: authService);
            controller.TempData["CurrentUserEmail"] = "user@test.local";

            var result = await controller.Logout();

            Assert.True(authService.SignOutCalled);
            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirect.ActionName);
            Assert.Equal("Home", redirect.ControllerName);
        }
    }
}
