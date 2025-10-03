using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebApplication1.Models;
using Xunit;

namespace WebApplication1.Tests.Models
{
    public class ObstacleDataTests
    {
        private IList<ValidationResult> ValidateModel(ObstacleData model)
        {
            var context = new ValidationContext(model);
            var results = new List<ValidationResult>();
            Validator.TryValidateObject(model, context, results, true);
            return results;
        }

        [Fact]
        public void ValidObstacleData_ShouldPassValidation()
        {
            var model = new ObstacleData
            {
                ObstacleName = "Kran",
                ObstacleHeight = 45.5,
                ObstacleDescription = "Stor byggekran nær landing",
                ObstacleLatitude = 58.146,
                ObstacleLongitude = 8.006,
                ObstacleType = "marker",
                ObstacleRadius = null,
                ObstacleLineCoords = null
            };

            var results = ValidateModel(model);

            Assert.Empty(results); // Ingen valideringsfeil
        }

        [Fact]
        public void MissingRequiredFields_ShouldFailValidation()
        {
            var model = new ObstacleData(); // Alt mangler

            var results = ValidateModel(model);

            Assert.Contains(results, r => r.MemberNames.Contains("ObstacleName"));
            Assert.Contains(results, r => r.MemberNames.Contains("ObstacleHeight"));
            Assert.Contains(results, r => r.MemberNames.Contains("ObstacleDescription"));
        }

        [Fact]
        public void ObstacleHeight_AboveMax_ShouldFailValidation()
        {
            var model = new ObstacleData
            {
                ObstacleName = "Test",
                ObstacleHeight = 250, // Over 200
                ObstacleDescription = "Test"
            };

            var results = ValidateModel(model);

            Assert.Contains(results, r => r.MemberNames.Contains("ObstacleHeight"));
        }

        [Fact]
        public void ObstacleName_TooLong_ShouldFailValidation()
        {
            var model = new ObstacleData
            {
                ObstacleName = new string('X', 101), // 101 tegn
                ObstacleHeight = 10,
                ObstacleDescription = "Test"
            };

            var results = ValidateModel(model);

            Assert.Contains(results, r => r.MemberNames.Contains("ObstacleName"));
        }
    }
}
