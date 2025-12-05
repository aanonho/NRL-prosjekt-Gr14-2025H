// DBContext that links EF Core to the database tables we use in the app.
using Microsoft.EntityFrameworkCore;
using WebApplication1.Models;
using WebApplication1.Models.Entities;

namespace WebApplication1.DataInfrastructure
{
    public class ApplicationDbContext : DbContext
    {
        // Standard setup so that DI can provide us with the correct DbContextOptions.
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {

        }

        // These DBSets reflect the tables that exist in the database.
        public DbSet<ObstacleData> Obstacles { get; set; }
        public DbSet<ReportItem> ReportItems { get; set; }
        public DbSet<Organization> Organizations { get; set; }
        public DbSet<UserEntity> Users { get; set; }
        public DbSet<Pilot> Pilots { get; set; }
        public DbSet<Registrar> Registrars { get; set; }
        public DbSet<PasswordResetToken> PasswordResetTokens { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Skips helper classes/fields that should not be stored directly in the database.
            modelBuilder.Ignore<ValidatedObstacleData>();
            modelBuilder.Entity<ReportItem>().Ignore(r => r.Obstacle);

            modelBuilder.Entity<ReportItem>()
                .HasOne(r => r.ReportObstacle)
                .WithOne(o => o.ReportItem)
                .HasForeignKey<ObstacleData>(o => o.ReportID)
                .OnDelete(DeleteBehavior.Cascade);

            // Organizations are managed in a separate table with OrganizationID as primary key.
            modelBuilder.Entity<Organization>(e =>
            {
                e.ToTable("Organization");
                e.HasKey(o => o.OrganizationID);
            });

            // Users are stored in UserData and linked to organizations.
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

            // Pilots inherits UserID as key and follows one-to-one user relationship.
            modelBuilder.Entity<Pilot>(e =>
            {
                e.ToTable("Pilot");
                e.HasKey(p => p.UserID);
                e.HasOne(p => p.User)
                    .WithOne(u => u.Pilot)
                    .HasForeignKey<Pilot>(p => p.UserID)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Registrars inherits UserID as key and follows one-to-one user relationship.
            modelBuilder.Entity<Registrar>(e =>
            {
                e.ToTable("Registrar");
                e.HasKey(r => r.UserID);
                e.HasOne(r => r.User)
                    .WithOne(u => u.Registrar)
                    .HasForeignKey<Registrar>(r => r.UserID)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // The token table handles password resets and ensures unique token hashes.
            modelBuilder.Entity<PasswordResetToken>(e =>
            {
                e.ToTable("PasswordResetTokens");
                e.HasKey(p => p.Id);
                e.HasIndex(p => p.TokenHash).IsUnique();
                e.HasOne(p => p.User)
                    .WithMany()
                    .HasForeignKey(p => p.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Reports link to pilots and optionally organizations with restrictive deletion.
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
