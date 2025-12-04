// DbContext som binder EF Core til database-tabellene vi bruker i appen
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Reflection.Emit;
using WebApplication1.Models;
using WebApplication1.Models.Entities;

namespace WebApplication1.DataInfrastructure
{
    public class ApplicationDbContext : DbContext
    {
        // Standard oppsett slik at DI kan gi oss riktige DbContextOptions
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {

        }

        // Disse DbSet-ene speiler tabellene som finnes i databasen
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

            // Hopper over hjelpeklasser/felter som ikke skal lagres direkte i databasen
            modelBuilder.Ignore<ValidatedObstacleData>();
            modelBuilder.Entity<ReportItem>().Ignore(r => r.Obstacle);

            modelBuilder.Entity<ReportItem>()
                .HasOne(r => r.ReportObstacle)
                .WithOne(o => o.ReportItem)
                .HasForeignKey<ObstacleData>(o => o.ReportID)
                .OnDelete(DeleteBehavior.Cascade);

            // Organisasjoner styres i egen tabell med primærnøkkel OrganizationID
            modelBuilder.Entity<Organization>(e =>
            {
                e.ToTable("Organization");
                e.HasKey(o => o.OrganizationID);
            });

            // Brukere lagres i UserData og får lenker til organisasjoner
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

            // Piloter arver UserID som nøkkel og følger brukeren én-til-én
            modelBuilder.Entity<Pilot>(e =>
            {
                e.ToTable("Pilot");
                e.HasKey(p => p.UserID);
                e.HasOne(p => p.User)
                    .WithOne(u => u.Pilot)
                    .HasForeignKey<Pilot>(p => p.UserID)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Registratorer har samme mønster som piloter: én-til-én med bruker
            modelBuilder.Entity<Registrar>(e =>
            {
                e.ToTable("Registrar");
                e.HasKey(r => r.UserID);
                e.HasOne(r => r.User)
                    .WithOne(u => u.Registrar)
                    .HasForeignKey<Registrar>(r => r.UserID)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Token-tabellen håndterer passordresett og sørger for unike token-hash
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

            // Rapporter peker både til pilot og organisasjon med restriktiv sletting
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