using System;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using WebApplication1.Controllers;
using WebApplication1.DataInfrastructure;
using WebApplication1.Models;
using WebApplication1.Models.Entities;
using WebApplication1.Services;
using Xunit;
using System.Collections.Generic;

namespace WebApplication1.Tests
{
    public class AccountControllerTests
    {
        private static ApplicationDbContext CreateContext()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            return new ApplicationDbContext(options);
        }

        private static TempDataDictionary CreateTempData(HttpContext httpContext)
        {
            return new TempDataDictionary(httpContext, new MemoryTempDataProvider());
        }

        [Fact]
        public async Task Login_WithValidCredentialsSignsInPilot()
        {
            using var context = CreateContext();
            var hasher = new PasswordHasher<UserEntity>();
            var user = new UserEntity
            {
                Email = "pilot@example.com",
                Role = "Pilot",
                Name = "Pilot User"
            };
            user.PasswordHash = hasher.HashPassword(user, "Secret123!");
            context.Users.Add(user);
            await context.SaveChangesAsync();

            var httpContext = new DefaultHttpContext();
            var services = new ServiceCollection();
            var authService = new FakeAuthenticationService();
            services.AddSingleton<IAuthenticationService>(authService);
            // Provide the URL helper factory so RedirectToAction can resolve Url
            services.AddSingleton<IUrlHelperFactory, UrlHelperFactory>();
            httpContext.RequestServices = services.BuildServiceProvider();
            httpContext.User = new ClaimsPrincipal(new ClaimsIdentity());

            var controller = new AccountController(context, new FakeEmailSender(), NullLogger<AccountController>.Instance)
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = httpContext,
                    RouteData = new Microsoft.AspNetCore.Routing.RouteData(),
                    ActionDescriptor = new Microsoft.AspNetCore.Mvc.Controllers.ControllerActionDescriptor()
                },
                TempData = CreateTempData(httpContext)
            };

            var model = new LoginViewModel
            {
                Email = user.Email,
                Password = "Secret123!",
                RememberMe = true
            };

            var result = await controller.Login(model);

            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Dataform", redirect.ActionName);
            Assert.Equal("Obstacle", redirect.ControllerName);
            Assert.NotNull(authService.SignedInPrincipal);
            Assert.Contains(authService.SignedInPrincipal!.Claims, c => c.Type == ClaimTypes.NameIdentifier);
        }

        private class FakeAuthenticationService : IAuthenticationService
        {
            public ClaimsPrincipal? SignedInPrincipal { get; private set; }

            public Task<AuthenticateResult> AuthenticateAsync(HttpContext context, string? scheme)
            {
                return Task.FromResult(AuthenticateResult.NoResult());
            }

            public Task ChallengeAsync(HttpContext context, string? scheme, AuthenticationProperties? properties)
            {
                return Task.CompletedTask;
            }

            public Task ForbidAsync(HttpContext context, string? scheme, AuthenticationProperties? properties)
            {
                return Task.CompletedTask;
            }

            public Task SignInAsync(HttpContext context, string? scheme, ClaimsPrincipal principal, AuthenticationProperties? properties)
            {
                // Record the principal so the test can assert the sign-in happened
                SignedInPrincipal = principal;
                return Task.CompletedTask;
            }

            public Task SignOutAsync(HttpContext context, string? scheme, AuthenticationProperties? properties)
            {
                SignedInPrincipal = null;
                return Task.CompletedTask;
            }
        }

        private sealed class MemoryTempDataProvider : ITempDataProvider
        {
            private Dictionary<string, object?> _store = new();

            public IDictionary<string, object?> LoadTempData(HttpContext context)
            {
                // Return a copy so the TempDataDictionary owns its own set
                return new Dictionary<string, object?>(_store);
            }

            public void SaveTempData(HttpContext context, IDictionary<string, object?> values)
            {
                _store = new Dictionary<string, object?>(values);
            }
        }

        private class FakeEmailSender : IEmailSender
        {
            public Task SendPasswordResetAsync(string recipientEmail, string recipientName, string resetLink)
            {
                return Task.CompletedTask;
            }
        }
    }
}
