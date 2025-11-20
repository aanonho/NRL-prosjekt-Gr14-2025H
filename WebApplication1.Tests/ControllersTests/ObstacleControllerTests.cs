using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.EntityFrameworkCore;
using WebApplication1.Controllers;
using WebApplication1.DataInfrastructure;
using WebApplication1.Models;
using WebApplication1.Models.Entities;
using WebApplication1.Tests.TestHelpers;
using Xunit;

namespace WebApplication1.Tests.Controllers
{
    public class ObstacleControllerTests
    {
        // Simple in-memory TempData provider for testing
        private class TestTempDataProvider : ITempDataProvider
        {
            private readonly Dictionary<string, object?> _data = new();

            public IDictionary<string, object?> LoadTempData(HttpContext context)
                => _data;

            public void SaveTempData(HttpContext context, IDictionary<string, object?> values)
            {
                _data.Clear();
                foreach (var kvp in values)
                {
                    _data[kvp.Key] = kvp.Value;
                }
            }
        }

        private static ObstacleController CreateController(
            ApplicationDbContext context,
            ClaimsPrincipal user = null,
            ITempDataDictionary tempData = null)
        {
            var httpContext = new DefaultHttpContext();

            if (user != null)
            {
                httpContext.User = user;
            }

            var controller = new ObstacleController(context)
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = httpContext
                }
            };

            if (tempData != null)
            {
                controller.TempData = tempData;
            }
            else
            {
                // Use our in-memory temp data provider
                controller.TempData = new TempDataDictionary(httpContext, new TestTempDataProvider());
            }

            return controller;
        }

        private static ClaimsPrincipal CreateUser(string email, params string[] roles)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, email),
                new Claim(ClaimTypes.Email, email)
            };

            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            var identity = new ClaimsIdentity(claims, "TestAuth");
            return new ClaimsPrincipal(identity);
        }

        [Fact]
        public void ReadAll_ReturnsOkWithAllObstacles()
        {
            // Arrange
            var db = DbContextHelper.CreateInMemoryDbContext(nameof(ReadAll_ReturnsOkWithAllObstacles));

            db.Obstacles.AddRange(
                new ObstacleData { ObstacleName = "Obstacle 1" },
                new ObstacleData { ObstacleName = "Obstacle 2" }
            );
            db.SaveChanges();

            var controller = CreateController(db);

            // Act
            var result = controller.ReadAll();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var obstacles = Assert.IsAssignableFrom<List<ObstacleData>>(okResult.Value);
            Assert.Equal(2, obstacles.Count);
        }

        [Fact]
        public void DataForm_UserIsRegistrar_SetsFlagAndReturnsView()
        {
            // Arrange
            var db = DbContextHelper.CreateInMemoryDbContext(nameof(DataForm_UserIsRegistrar_SetsFlagAndReturnsView));
            var user = CreateUser("registrar@test.local", "Registrar");
            var controller = CreateController(db, user);

            // Act
            var result = controller.DataForm();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.True(controller.ViewBag.IsRegistrarViewer);
        }

        [Fact]
        public void DataForm_UserIsNotRegistrar_DoesNotSetFlagAndReturnsView()
        {
            // Arrange
            var db = DbContextHelper.CreateInMemoryDbContext(nameof(DataForm_UserIsNotRegistrar_DoesNotSetFlagAndReturnsView));
            var user = CreateUser("pilot@test.local", "Pilot");
            var controller = CreateController(db, user);

            // Act
            var result = controller.DataForm();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            // ViewBag.IsRegistrarViewer should either be null or false when not registrar
            Assert.False((bool?)controller.ViewBag.IsRegistrarViewer ?? false);
        }

        [Fact]
        public async Task DataForm_Post_UserNotPilot_RedirectsToReportsIndexAndSetsError()
        {
            // Arrange
            var db = DbContextHelper.CreateInMemoryDbContext(nameof(DataForm_Post_UserNotPilot_RedirectsToReportsIndexAndSetsError));
            var user = CreateUser("user@test.local", "Registrar"); // Not a Pilot

            var httpContext = new DefaultHttpContext { User = user };
            var tempData = new TempDataDictionary(httpContext, new TestTempDataProvider());

            var controller = new ObstacleController(db)
            {
                ControllerContext = new ControllerContext { HttpContext = httpContext },
                TempData = tempData
            };

            var validatedData = new ValidatedObstacleData
            {
                ObstacleName = "Test",
                ObstacleHeight = 10,
                ObstacleDescription = "Desc"
            };

            // Act
            var result = await controller.DataForm(validatedData, imageFile: null, submitType: "Submit");

            // Assert
            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirect.ActionName);
            Assert.Equal("Reports", redirect.ControllerName);

            Assert.True(controller.TempData.ContainsKey("ErrorMessage"));
        }

        [Fact]
        public async Task Details_ReportNotFound_ReturnsNotFound()
        {
            // Arrange
            var db = DbContextHelper.CreateInMemoryDbContext(nameof(Details_ReportNotFound_ReturnsNotFound));
            var controller = CreateController(db);

            // Act
            var result = await controller.Details(999);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task Details_ReportFoundButNoCurrentUserEmail_RedirectsToUserForm()
        {
            // Arrange
            var db = DbContextHelper.CreateInMemoryDbContext(nameof(Details_ReportFoundButNoCurrentUserEmail_RedirectsToUserForm));

            var report = new ReportItem
            {
                ReportID = 1,
                ReportObstacle = new ObstacleData
                {
                    ObstacleName = "Test Obs"
                }
            };
            db.ReportItems.Add(report);
            db.SaveChanges();

            var controller = CreateController(db);
            // TempData["CurrentUserEmail"] is left null

            // Act
            var result = await controller.Details(1);

            // Assert
            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("UserForm", redirect.ActionName);
            Assert.Equal("User", redirect.ControllerName);
        }
    }
}
