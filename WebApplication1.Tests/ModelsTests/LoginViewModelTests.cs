using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using WebApplication1.Models;
using Xunit;

namespace WebApplication1.Tests.Models
{
    public class LoginViewModelTests
    {
        // Helper: run data annotation validation on a model instance
        private static IList<ValidationResult> ValidateModel(object model)
        {
            var results = new List<ValidationResult>();
            var context = new ValidationContext(model, serviceProvider: null, items: null);
            Validator.TryValidateObject(model, context, results, validateAllProperties: true);
            return results;
        }

        [Fact]
        public void ValidModel_HasNoValidationErrors()
        {
            // Arrange
            var vm = new LoginViewModel
            {
                Email = "user@example.com",
                Password = "MySecurePassword123!",
                RememberMe = true,
                ReturnUrl = "/home/index"
            };

            // Act
            var results = ValidateModel(vm);

            // Assert
            Assert.Empty(results);
        }

        [Fact]
        public void MissingEmail_IsInvalid()
        {
            // Arrange
            var vm = new LoginViewModel
            {
                Email = null,
                Password = "Password123!"
            };

            // Act
            var results = ValidateModel(vm);

            // Assert
            Assert.Contains(results, r => r.MemberNames.Contains(nameof(LoginViewModel.Email)));
        }

        [Fact]
        public void InvalidEmailFormat_IsInvalid()
        {
            // Arrange
            var vm = new LoginViewModel
            {
                Email = "not-an-email",
                Password = "Password123!"
            };

            // Act
            var results = ValidateModel(vm);

            // Assert
            Assert.Contains(results, r => r.MemberNames.Contains(nameof(LoginViewModel.Email)));
        }

        [Fact]
        public void MissingPassword_IsInvalid()
        {
            // Arrange
            var vm = new LoginViewModel
            {
                Email = "user@example.com",
                Password = null
            };

            // Act
            var results = ValidateModel(vm);

            // Assert
            Assert.Contains(results, r => r.MemberNames.Contains(nameof(LoginViewModel.Password)));
        }

        [Fact]
        public void DefaultValues_AreReasonable()
        {
            // Arrange
            var vm = new LoginViewModel();

            // Act & Assert
            Assert.False(vm.RememberMe);        // default bool = false
            Assert.Null(vm.Email);
            Assert.Null(vm.Password);
            Assert.Null(vm.ReturnUrl);
        }
    }
}
