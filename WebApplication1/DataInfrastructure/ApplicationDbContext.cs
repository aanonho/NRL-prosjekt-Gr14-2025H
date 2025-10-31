using Microsoft.EntityFrameworkCore;
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

        // Database migrations
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Prevent EF to create tables already existing in the DB at each migrations

            modelBuilder.Ignore<ValidatedObstacleData>();
            modelBuilder.Entity<ReportItem>().Ignore(r => r.Obstacle);

            modelBuilder.Entity<ReportItem>()
                .HasOne(r => r.ReportObstacle)
                .WithOne(o => o.ReportItem)
                .HasForeignKey<ObstacleData>(o => o.ReportID)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
