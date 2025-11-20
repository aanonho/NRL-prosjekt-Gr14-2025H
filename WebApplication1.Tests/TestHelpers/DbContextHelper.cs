using Microsoft.EntityFrameworkCore;
using WebApplication1.DataInfrastructure;

namespace WebApplication1.Tests.TestHelpers
{
    public static class DbContextHelper
    {
        /// <summary>
        /// Creates a new ApplicationDbContext using EF Core InMemory
        /// with the given database name (use a unique name per test).
        /// </summary>
        public static ApplicationDbContext CreateInMemoryDbContext(string databaseName)
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName)
                .Options;

            return new ApplicationDbContext(options);
        }
    }
}