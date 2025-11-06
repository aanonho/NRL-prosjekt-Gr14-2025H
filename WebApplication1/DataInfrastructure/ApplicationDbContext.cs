using Microsoft.EntityFrameworkCore;
using MySqlConnector;
using WebApplication1.Models;

namespace WebApplication1.DataInfrastructure
{
    public class ApplicationDbContext : DbContext
    {
        // Constructor to initialize the DbContext with options
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {

        }

        public DbSet<ObstacleData> Obstacles { get; set; }
        public DbSet<ReportItem> ReportStore { get; set; }
        
    }
}
