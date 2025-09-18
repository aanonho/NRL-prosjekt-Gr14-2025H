using Microsoft.EntityFrameworkCore;
using WebApplication1.Models;

namespace WebApplication1.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<ObstacleData> Obstacles => Set<ObstacleData>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ObstacleData>(e =>
            {
                e.HasKey(x => x.Id);
                e.Property(x => x.ObstacleName).IsRequired().HasMaxLength(100);
                e.Property(x => x.ObstacleDescription).IsRequired().HasMaxLength(1000);
                e.Property(x => x.ObstacleLatitude).IsRequired();
                e.Property(x => x.ObstacleLongitude).IsRequired();
            });
        }
    }
}
