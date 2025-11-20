using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq; 
using WebApplication1.Models;
using Xunit;

namespace WebApplication1.Tests.Models
{
    public class ObstacleDataTests
    {
        // Helper to run data annotation validation on a model instance
        private static IList<ValidationResult> ValidateModel(object model)
        {
            var results = new List<ValidationResult>();
            var context = new ValidationContext(model, serviceProvider: null, items: null);
            Validator.TryValidateObject(model, context, results, validateAllProperties: true);
            return results;
        }

        [Fact]
        public void ValidatedObstacleData_WithValidValues_IsValid()
        {
            // Arrange
            var model = new ValidatedObstacleData
            {
                ObstacleName = "Test Obstacle",
                ObstacleHeight = 50.0,
                ObstacleDescription = "A valid description for the obstacle."
            };

            // Act
            var results = ValidateModel(model);

            // Assert
            Assert.Empty(results);
        }

        [Fact]
        public void ValidatedObstacleData_MissingName_IsInvalid()
        {
            // Arrange
            var model = new ValidatedObstacleData
            {
                ObstacleName = null,
                ObstacleHeight = 50.0,
                ObstacleDescription = "Some description"
            };

            // Act
            var results = ValidateModel(model);

            // Assert
            Assert.Contains(results, r => r.MemberNames.Contains(nameof(ValidatedObstacleData.ObstacleName)));
        }

        [Fact]
        public void ValidatedObstacleData_NameTooLong_IsInvalid()
        {
            // Arrange
            var longName = new string('A', 101); // 101 chars, max is 100
            var model = new ValidatedObstacleData
            {
                ObstacleName = longName,
                ObstacleHeight = 50.0,
                ObstacleDescription = "Some description"
            };

            // Act
            var results = ValidateModel(model);

            // Assert
            Assert.Contains(results, r => r.MemberNames.Contains(nameof(ValidatedObstacleData.ObstacleName)));
        }

        [Fact]
        public void ValidatedObstacleData_MissingHeight_IsInvalid()
        {
            // Arrange
            var model = new ValidatedObstacleData
            {
                ObstacleName = "Test Obstacle",
                ObstacleHeight = null,
                ObstacleDescription = "Some description"
            };

            // Act
            var results = ValidateModel(model);

            // Assert
            Assert.Contains(results, r => r.MemberNames.Contains(nameof(ValidatedObstacleData.ObstacleHeight)));
        }

        [Theory]
        [InlineData(-1.0)]
        [InlineData(201.0)]
        public void ValidatedObstacleData_HeightOutOfRange_IsInvalid(double invalidHeight)
        {
            // Arrange
            var model = new ValidatedObstacleData
            {
                ObstacleName = "Test Obstacle",
                ObstacleHeight = invalidHeight,
                ObstacleDescription = "Some description"
            };

            // Act
            var results = ValidateModel(model);

            // Assert
            Assert.Contains(results, r => r.MemberNames.Contains(nameof(ValidatedObstacleData.ObstacleHeight)));
        }

        [Fact]
        public void ValidatedObstacleData_MissingDescription_IsInvalid()
        {
            // Arrange
            var model = new ValidatedObstacleData
            {
                ObstacleName = "Test Obstacle",
                ObstacleHeight = 50.0,
                ObstacleDescription = null
            };

            // Act
            var results = ValidateModel(model);

            // Assert
            Assert.Contains(results, r => r.MemberNames.Contains(nameof(ValidatedObstacleData.ObstacleDescription)));
        }

        [Fact]
        public void ValidatedObstacleData_DescriptionTooLong_IsInvalid()
        {
            // Arrange
            var longDescription = new string('D', 1001); // 1001 chars, max is 1000
            var model = new ValidatedObstacleData
            {
                ObstacleName = "Test Obstacle",
                ObstacleHeight = 50.0,
                ObstacleDescription = longDescription
            };

            // Act
            var results = ValidateModel(model);

            // Assert
            Assert.Contains(results, r => r.MemberNames.Contains(nameof(ValidatedObstacleData.ObstacleDescription)));
        }
    }
}
