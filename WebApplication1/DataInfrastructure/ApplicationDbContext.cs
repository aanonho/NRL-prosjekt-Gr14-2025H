using Microsoft.EntityFrameworkCore;
using WebApplication1.Models;
using WebApplication1.Models.Entities;

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
        public DbSet<Organization> Organizations { get; set; }
        public DbSet<UserEntity> Users { get; set; }
        public DbSet<Pilot> Pilots { get; set; }
        public DbSet<Registrar> Registrars { get; set; }


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
            // --- Organization ---
            modelBuilder.Entity<Organization>(e =>
            {
                e.ToTable("Organization");
                e.HasKey(o => o.OrganizationID);
            });

            // --- UserData (as UserEntity class) ---
            modelBuilder.Entity<UserEntity>(e =>
            {
                e.ToTable("UserData");
                e.HasKey(u => u.UserID);
                e.HasIndex(u => u.OrganizationID).HasDatabaseName("UserData_Organization_idx");
                e.HasOne(u => u.Organization)
                    .WithMany(o => o.Users)
                    .HasForeignKey(u => u.OrganizationID)
                    .OnDelete(DeleteBehavior.SetNull);
            });

            // --- Pilot (1–1 with UserData) ---
            modelBuilder.Entity<Pilot>(e =>
            {
                e.ToTable("Pilot");
                e.HasKey(p => p.UserID);
                e.HasOne(p => p.User)
                    .WithOne(u => u.Pilot)
                    .HasForeignKey<Pilot>(p => p.UserID)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // --- Registrar (1–1 with UserData) ---
            modelBuilder.Entity<Registrar>(e =>
            {
                e.ToTable("Registrar");
                e.HasKey(r => r.UserID);
                e.HasOne(r => r.User)
                    .WithOne(u => u.Registrar)
                    .HasForeignKey<Registrar>(r => r.UserID)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // --- ReportItem → Pilot (FK = ReportItem.PilotID → Pilot.UserID) ---
            modelBuilder.Entity<ReportItem>(e =>
            {
                e.HasOne(r => r.Pilot)
                 .WithMany(p => p.Reports)
                 .HasForeignKey(r => r.PilotID)
                 .HasPrincipalKey(p => p.UserID)
                 .OnDelete(DeleteBehavior.Restrict);

                e.HasOne(r => r.OrganizationRef)
                 .WithMany(o => o.Reports)
                 .HasForeignKey(r => r.OrganizationID)
                 .OnDelete(DeleteBehavior.SetNull);
               
            });

        }
    }
}
