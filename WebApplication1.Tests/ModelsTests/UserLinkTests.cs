using WebApplication1.Models;
using Xunit;

namespace WebApplication1.Tests.Models
{
    public class UserLinkTests
    {
        [Fact]
        public void GetDisplayName_ReturnsFormattedString()
        {
            var link = new UserLink
            {
                SubmittedByName = "Test User",
                OrganizationName = "NRL"
            };

            var result = link.GetDisplayName();

            Assert.Equal("Test User (NRL)", result);
        }

        [Fact]
        public void DefaultValues_AreEmpty()
        {
            var link = new UserLink();

            Assert.Equal(string.Empty, link.SubmittedByEmail);
            Assert.Equal(string.Empty, link.SubmittedByName);
            Assert.Equal(string.Empty, link.OrganizationName);
            Assert.Null(link.ReviewMessage);
        }
    }
}