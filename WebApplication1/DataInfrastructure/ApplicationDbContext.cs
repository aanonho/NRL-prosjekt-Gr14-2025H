using Microsoft.EntityFrameworkCore;
using WebApplication1.Models;

namespace WebApplication1.DataInfrastructure
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        public DbSet<ObstacleData> ObstacleData { get; set; }
    }
}
