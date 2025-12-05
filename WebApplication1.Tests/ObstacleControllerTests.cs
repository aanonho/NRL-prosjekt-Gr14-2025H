using System;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.EntityFrameworkCore;
using WebApplication1.Controllers;
using WebApplication1.DataInfrastructure;
using WebApplication1.Models;
using WebApplication1.Models.Entities;
using Xunit;
using System.Collections.Generic;

namespace WebApplication1.Tests
{
    public class ObstacleControllerTests
    {
        private static ApplicationDbContext CreateContext()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            return new ApplicationDbContext(options);
        }

        private static ObstacleController BuildController(ApplicationDbContext context, string email)
        {
            var controller = new ObstacleController(context);
            var httpContext = new DefaultHttpContext
            {
                // Simulate an authenticated pilot so the controller passes role checks
                User = new ClaimsPrincipal(new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.Name, email),
                    new Claim(ClaimTypes.Role, "Pilot")
                }, "TestAuth"))
            };

            controller.ControllerContext = new ControllerContext { HttpContext = httpContext };
            controller.TempData = new TempDataDictionary(httpContext, new MemoryTempDataProvider());
            controller.TempData["CurrentUserEmail"] = email;

            return controller;
        }

        [Fact]
        public async Task DataForm_SubmitsNewObstacleAndReport()
        {
            using var context = CreateContext();
            const string email = "pilot@example.com";

            var organization = new Organization { Name = "Test Org" };
            context.Organizations.Add(organization);

            var user = new UserEntity { Email = email, Role = "Pilot", Name = "Pilot", Organization = organization };
            context.Users.Add(user);
            await context.SaveChangesAsync();

            var controller = BuildController(context, email);

            // Provide the minimum valid obstacle payload so the action will accept it
            var data = new ValidatedObstacleData
            {
                ObstacleName = "Tower",
                ObstacleHeight = 120,
                ObstacleDescription = "Test obstacle",
                ObstacleLatitude = 10,
                ObstacleLongitude = 20,
                ObstacleType = "circle",
                ObstacleRadius = 5,
                ObstacleHasLight = true
            };

            var result = await controller.DataForm(data, imageFiles: null, submitType: "Submit");

            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirect.ActionName);
            Assert.Equal("Reports", redirect.ControllerName);

            var report = Assert.Single(context.ReportItems);
            var obstacle = Assert.Single(context.Obstacles);
            Assert.False(report.IsDraft);
            Assert.Equal(report.ReportID, obstacle.ReportID);
            Assert.Equal("Tower", obstacle.ObstacleName);
        }

        [Fact]
        public async Task DataForm_SavesDraftReportWhenRequested()
        {
            using var context = CreateContext();
            const string email = "drafter@example.com";

            var user = new UserEntity { Email = email, Role = "Pilot", Name = "Draft Pilot" };
            context.Users.Add(user);
            await context.SaveChangesAsync();

            var controller = BuildController(context, email);

            // Ask the action to keep the report as a draft instead of submitting it
            var data = new ValidatedObstacleData
            {
                ObstacleName = "Draft Mast",
                ObstacleHeight = 80,
                ObstacleDescription = "Draft description",
                ObstacleLatitude = 30,
                ObstacleLongitude = 40,
                ObstacleType = "circle",
                ObstacleRadius = 2,
                ObstacleHasLight = false
            };

            var result = await controller.DataForm(data, imageFiles: null, submitType: "SaveDraft");

            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirect.ActionName);
            Assert.Equal("Reports", redirect.ControllerName);

            var report = Assert.Single(context.ReportItems);
            var obstacle = Assert.Single(context.Obstacles);
            Assert.True(report.IsDraft);
            Assert.True(obstacle.IsDraft);
            Assert.Equal("Draft", report.Status);
        }

        [Fact]
        public async Task DataForm_UpdatesExistingObstacle()
        {
            using var context = CreateContext();
            const string email = "editor@example.com";

            var user = new UserEntity { Email = email, Role = "Pilot", Name = "Editor" };
            context.Users.Add(user);
            await context.SaveChangesAsync();

            var report = new ReportItem
            {
                PilotID = user.UserID,
                SubmittedByEmail = email,
                SubmittedByName = "Editor",
                Status = "Draft",
                IsDraft = true
            };
            context.ReportItems.Add(report);
            await context.SaveChangesAsync();

            var obstacle = new ObstacleData
            {
                ReportID = report.ReportID,
                ObstacleName = "Old Name",
                ObstacleHeight = 50,
                ObstacleDescription = "Old description",
                IsDraft = true
            };
            context.Obstacles.Add(obstacle);
            report.ReportObstacle = obstacle;
            await context.SaveChangesAsync();

            var controller = BuildController(context, email);

            // Supply new obstacle details to verify the edit branch rewrites the stored entity
            var updatedData = new ValidatedObstacleData
            {
                ReportID = report.ReportID,
                ObstacleName = "New Name",
                ObstacleHeight = 75,
                ObstacleDescription = "Updated description",
                ObstacleLatitude = 1,
                ObstacleLongitude = 2,
                ObstacleType = "line",
                ObstacleRadius = 3,
                ObstacleHasLight = true,
                ObstacleLineCoordinates = "58.146,7.995;58.150,8.001"
            };

            var result = await controller.DataForm(updatedData, imageFiles: null, submitType: "Submit");

            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirect.ActionName);
            Assert.Equal("Reports", redirect.ControllerName);

            var refreshedReport = Assert.Single(context.ReportItems.Include(r => r.ReportObstacle));
            Assert.False(refreshedReport.IsDraft);
            Assert.Equal("Pending", refreshedReport.Status);
            Assert.Equal("New Name", refreshedReport.ReportObstacle!.ObstacleName);
            Assert.Equal(75, refreshedReport.ReportObstacle.ObstacleHeight);
        }

        private sealed class MemoryTempDataProvider : ITempDataProvider
        {
            private Dictionary<string, object?> _store = new();

            public IDictionary<string, object?> LoadTempData(HttpContext context)
            {
                return new Dictionary<string, object?>(_store);
            }

            public void SaveTempData(HttpContext context, IDictionary<string, object?> values)
            {
                _store = new Dictionary<string, object?>(values);
            }
        }
    }
}
