using Microsoft.Extensions.Configuration;
using NSubstitute;
using WebApplication1.Controllers;

namespace Kartverket.UnitTests
{
    public class HomeControllerUnitTests
    {
        [Fact]
        public void IndexHasNullViewName()
        {
            // Arrange
            var controller = GetUnitUnderTest();

            // Act
            var result = controller.Index();
            var viewResult = result as ViewResult;

            // Assert
            Assert.Null(viewResult.ViewName);

        }

        private HomeController GetUnitUnderTest()
        {
            var logger = Substitute.For<Microsoft.Extensions.Logging.ILogger<HomeController>>();
            var config = Substitute.For<IConfiguration>();

            return new HomeController(logger, config);
        }
    }
}
