using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApplication1.Controllers;
using WebApplication1.DataInfrastructure;
using WebApplication1.Models;
using Xunit;

namespace WebApplication1.Tests.Controllers
{
    public class ReportsControllerTests
    {
        // Local helper: create InMemory ApplicationDbContext for this test class
        private static ApplicationDbContext CreateInMemoryDbContext(string databaseName)
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName)
                .Options;

            return new ApplicationDbContext(options);
        }

        private static ClaimsPrincipal CreateUser(string email, bool isPilot = false, bool isRegistrar = false)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, email),
                new Claim(ClaimTypes.Email, email)
            };

            if (isPilot) claims.Add(new Claim(ClaimTypes.Role, "Pilot"));
            if (isRegistrar) claims.Add(new Claim(ClaimTypes.Role, "Registrar"));

            var identity = new ClaimsIdentity(claims, "TestAuth");
            return new ClaimsPrincipal(identity);
        }

        [Fact]
        public async Task Index_Pilot_OnlySeesOwnReports()
        {
            // Arrange
            var db = CreateInMemoryDbContext(nameof(Index_Pilot_OnlySeesOwnReports));

            db.ReportItems.AddRange(
                new ReportItem { ReportID = 1, PilotID = 1, SubmittedByEmail = "pilot@test.com" },
                new ReportItem { ReportID = 2, PilotID = 2, SubmittedByEmail = "other@test.com" }
            );
            db.SaveChanges();

            var controller = new ReportsController(db)
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = new DefaultHttpContext
                    {
                        User = CreateUser("pilot@test.com", isPilot: true)
                    }
                }
            };

            // Act
            var result = await controller.Index();

            // Assert
            var view = Assert.IsType<ViewResult>(result);
            var vm = Assert.IsType<ReportsIndexViewModel>(view.Model);

            Assert.Single(vm.Reports);
            Assert.Equal("pilot@test.com", vm.Reports.First().SubmittedByEmail);
        }

        [Fact]
        public async Task Index_SortAscending_Works()
        {
            // Arrange
            var db = CreateInMemoryDbContext(nameof(Index_SortAscending_Works));

            db.ReportItems.AddRange(
                new ReportItem { ReportID = 1, PilotID = 1, SubmittedByEmail = "a@test.com", CreatedAt = DateTime.Now.AddDays(-1) },
                new ReportItem { ReportID = 2, PilotID = 1, SubmittedByEmail = "a@test.com", CreatedAt = DateTime.Now }
            );
            db.SaveChanges();

            var controller = new ReportsController(db)
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = new DefaultHttpContext
                    {
                        User = CreateUser("a@test.com", isPilot: true)
                    }
                }
            };

            // Act
            var result = await controller.Index(sort: "date_asc");

            // Assert
            var view = Assert.IsType<ViewResult>(result);
            var vm = Assert.IsType<ReportsIndexViewModel>(view.Model);

            Assert.Equal(1, vm.Reports.First().ReportID);
        }

        [Fact]
        public async Task Index_FilterByStatus_Draft_Works()
        {
            // Arrange
            var db = CreateInMemoryDbContext(nameof(Index_FilterByStatus_Draft_Works));

            db.ReportItems.AddRange(
                new ReportItem { ReportID = 1, PilotID = 1, SubmittedByEmail = "pilot@test.com", IsDraft = true, Status = "Draft" },
                new ReportItem { ReportID = 2, PilotID = 1, SubmittedByEmail = "pilot@test.com", IsDraft = false, Status = "Pending" }
            );
            db.SaveChanges();

            var controller = new ReportsController(db)
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = new DefaultHttpContext
                    {
                        User = CreateUser("pilot@test.com", isPilot: true)
                    }
                }
            };

            // Act
            var result = await controller.Index(status: "draft");

            // Assert
            var view = Assert.IsType<ViewResult>(result);
            var vm = Assert.IsType<ReportsIndexViewModel>(view.Model);

            Assert.Single(vm.Reports);
            Assert.True(vm.Reports.First().IsDraft);
        }

        [Fact]
        public void ReportStatus_ReturnsFilteredList()
        {
            // Arrange
            var db = CreateInMemoryDbContext(nameof(ReportStatus_ReturnsFilteredList));

            db.ReportItems.AddRange(
                new ReportItem { ReportID = 1, Status = "Pending", PilotID = 1 },
                new ReportItem { ReportID = 2, Status = "Approved", PilotID = 1 }
            );
            db.SaveChanges();

            var controller = new ReportsController(db);

            // Act
            var result = controller.ReportStatus("approved");

            // Assert
            var view = Assert.IsType<ViewResult>(result);
            var list = Assert.IsType<List<ReportItem>>(view.Model);

            Assert.Single(list);
            Assert.Equal("Approved", list.First().Status);
        }

        [Fact]
        public void UpdateStatus_UpdatesReport()
        {
            // Arrange
            var db = CreateInMemoryDbContext(nameof(UpdateStatus_UpdatesReport));

            db.ReportItems.Add(new ReportItem { ReportID = 1, PilotID = 1, Status = "Pending" });
            db.SaveChanges();

            var controller = new ReportsController(db);

            // Act
            var result = controller.UpdateStatus(1, "Approved", "Looks good");

            // Assert
            var updated = db.ReportItems.First();
            Assert.Equal("Approved", updated.Status);
            Assert.Equal("Looks good", updated.ReviewMessage);
            Assert.IsType<RedirectToActionResult>(result);
        }

        [Fact]
        public void Details_ReportNotFound_ReturnsNotFound()
        {
            // Arrange
            var db = CreateInMemoryDbContext(nameof(Details_ReportNotFound_ReturnsNotFound));
            var controller = new ReportsController(db);

            // Act
            var result = controller.Details(999);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }
    }
}
