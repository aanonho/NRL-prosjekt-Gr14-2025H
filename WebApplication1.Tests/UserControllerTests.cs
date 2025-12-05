using System;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.EntityFrameworkCore;
using Xunit;
using WebApplication1.Controllers;
using WebApplication1.DataInfrastructure;
using WebApplication1.Models;
using WebApplication1.Models.Entities;
using System.Collections.Generic;

namespace WebApplication1.Tests
{
    public class UserControllerTests
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
            // TempData is required because the controller writes success messages into it
            return new TempDataDictionary(httpContext, new MemoryTempDataProvider());
        }

        [Fact]
        public async Task UserForm_CreatesNewUserAndRedirectsToLogin()
        {
            using var context = CreateContext();
            var controller = new UserController(context);
            var httpContext = new DefaultHttpContext();

            controller.ControllerContext = new ControllerContext { HttpContext = httpContext };
            controller.TempData = CreateTempData(httpContext);

            var userData = new UserData
            {
                Name = "Test User",
                Email = "test@example.com",
                Phone = "123-456-7890",
                Role = "Pilot",
                Organization = "Avinor",
                Password = "Password123!",
                ConfirmPassword = "Password123!"
            };

            var result = await controller.UserForm(userData);

            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Login", redirect.ActionName);
            Assert.Equal("Account", redirect.ControllerName);

            var savedUser = Assert.Single(context.Users);
            Assert.Equal("Pilot", savedUser.Role);
            Assert.False(string.IsNullOrWhiteSpace(savedUser.PasswordHash));
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
