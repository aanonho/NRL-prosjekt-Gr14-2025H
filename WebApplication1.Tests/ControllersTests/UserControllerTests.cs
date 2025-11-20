using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.EntityFrameworkCore;
using WebApplication1.Controllers;
using WebApplication1.DataInfrastructure;
using WebApplication1.Models;
using WebApplication1.Models.Entities;
using Xunit;

namespace WebApplication1.Tests.Controllers
{
    public class UserControllerTests
    {
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

        // Simple fake UrlHelper so Url.Action doesn't crash
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

        private static ApplicationDbContext CreateInMemoryDbContext(string dbName)
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(dbName)
                .Options;

            return new ApplicationDbContext(options);
        }

        private static UserController CreateController(
            ApplicationDbContext db,
            ClaimsPrincipal? user = null,
            bool withTempData = false)
        {
            var httpContext = new DefaultHttpContext();

            if (user != null)
            {
                httpContext.User = user;
            }

            var controller = new UserController(db)
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = httpContext
                }
            };

            if (withTempData)
            {
                controller.TempData = new TempDataDictionary(httpContext, new TestTempDataProvider());
            }

            controller.Url = new FakeUrlHelper();

            return controller;
        }

        [Fact]
        public void UserForm_Get_ReturnsViewWithNewUserData()
        {
            var db = CreateInMemoryDbContext(nameof(UserForm_Get_ReturnsViewWithNewUserData));
            var controller = CreateController(db);

            var result = controller.UserForm();

            var view = Assert.IsType<ViewResult>(result);
            Assert.IsType<UserData>(view.Model);
        }

        [Fact]
        public async Task UserForm_Post_InvalidModel_ReturnsViewWithSameModel()
        {
            var db = CreateInMemoryDbContext(nameof(UserForm_Post_InvalidModel_ReturnsViewWithSameModel));
            var controller = CreateController(db);

            var userData = new UserData
            {
                Email = null,
                Role = "Pilot"
            };

            controller.ModelState.AddModelError("Email", "Required");

            var result = await controller.UserForm(userData);

            var view = Assert.IsType<ViewResult>(result);
            Assert.Same(userData, view.Model);
        }

        [Fact]
        public async Task UserForm_Post_NewPilotUser_SavesAndRedirectsToLogin()
        {
            var db = CreateInMemoryDbContext(nameof(UserForm_Post_NewPilotUser_SavesAndRedirectsToLogin));
            var controller = CreateController(db, withTempData: true);

            var userData = new UserData
            {
                Name = "Test Pilot",
                Email = "pilot@test.local",
                Phone = "12345678",
                Role = "Pilot",
                Password = "Secret123!",
                Organization = "NRL Org"
            };

            var result = await controller.UserForm(userData);

            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Login", redirect.ActionName);
            Assert.Equal("Account", redirect.ControllerName);

            var savedUser = db.Users
                .Include(u => u.Organization)
                .Include(u => u.Pilot)
                .FirstOrDefault(u => u.Email == "pilot@test.local");

            Assert.NotNull(savedUser);
            Assert.Equal("Pilot", savedUser!.Role);
            Assert.NotNull(savedUser.Pilot);
            Assert.Equal("NRL Org", savedUser.Organization?.Name);
        }

        [Fact]
        public async Task UserProfile_NoEmailAnywhere_RedirectsToAccountLogin()
        {
            var db = CreateInMemoryDbContext(nameof(UserProfile_NoEmailAnywhere_RedirectsToAccountLogin));
            var controller = CreateController(db, withTempData: true);

            var result = await controller.UserProfile(null);

            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Login", redirect.ActionName);
            Assert.Equal("Account", redirect.ControllerName);
        }

        [Fact]
        public async Task RegistrarDashboard_ExcludesDraftsAndSortsDesc()
        {
            var db = CreateInMemoryDbContext(nameof(RegistrarDashboard_ExcludesDraftsAndSortsDesc));

            db.ReportItems.AddRange(
                new ReportItem { ReportID = 1, IsDraft = true, Status = "Draft", CreatedAt = DateTime.Now.AddDays(-2), PilotID = 1 },
                new ReportItem { ReportID = 2, IsDraft = false, Status = "Pending", CreatedAt = DateTime.Now.AddDays(-1), PilotID = 1 },
                new ReportItem { ReportID = 3, IsDraft = false, Status = "Approved", CreatedAt = DateTime.Now, PilotID = 1 }
            );
            db.SaveChanges();

            var controller = CreateController(db);

            var result = await controller.RegistrarDashboard(status: "all", sort: "date_desc");

            var view = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<List<ReportItem>>(view.Model);

            Assert.Equal(2, model.Count);
            Assert.All(model, r => Assert.False(r.IsDraft));
            Assert.Equal(3, model[0].ReportID);
            Assert.Equal(2, model[1].ReportID);
        }

        [Fact]
        public async Task UpdateReportStatus_ReportIsDraft_SetsErrorAndRedirects()
        {
            var db = CreateInMemoryDbContext(nameof(UpdateReportStatus_ReportIsDraft_SetsErrorAndRedirects));

            db.ReportItems.Add(new ReportItem
            {
                ReportID = 1,
                IsDraft = true,
                Status = "Draft",
                PilotID = 1
            });
            db.SaveChanges();

            var controller = CreateController(db, withTempData: true);

            var result = await controller.UpdateReportStatus(1, "Approved", "ok");

            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("RegistrarDashboard", redirect.ActionName);
            Assert.True(controller.TempData.ContainsKey("ErrorMessage"));
        }

        [Fact]
        public async Task UpdateReportStatus_ValidSubmittedReport_UpdatesAndRedirects()
        {
            var db = CreateInMemoryDbContext(nameof(UpdateReportStatus_ValidSubmittedReport_UpdatesAndRedirects));

            db.ReportItems.Add(new ReportItem
            {
                ReportID = 1,
                IsDraft = false,
                Status = "Pending",
                PilotID = 1
            });
            db.SaveChanges();

            var controller = CreateController(db, withTempData: true);

            var result = await controller.UpdateReportStatus(1, "Approved", "Looks good");

            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("RegistrarDashboard", redirect.ActionName);

            var updated = db.ReportItems.First(r => r.ReportID == 1);
            Assert.Equal("Approved", updated.Status);
            Assert.False(updated.IsDraft);
            Assert.Equal("Looks good", updated.ReviewMessage);
            Assert.NotNull(updated.ReviewedAt);
            Assert.True(controller.TempData.ContainsKey("SuccessMessage"));
        }
    }
}
