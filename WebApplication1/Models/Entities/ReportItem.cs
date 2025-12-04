// Filen beskriver en hinder-rapport med status, eierinformasjon og navigasjonskoblinger.
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using WebApplication1.Models.Entities;

namespace WebApplication1.Models
{
    [Table("ReportItem")]
    public partial class ReportItem
    {
        // Identifikator og tidsstempel for rapporten, samt referanse til piloten som leverte den.
        [Key]
        [Column("ReportID")]
        public int ReportID { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        [Required]
        public int PilotID { get; set; }

        // Status og organisasjonstilknytning styrer arbeidsflyt og tilgang.
        [Required]
        [StringLength(32)]
        public string Status { get; set; } = "Pending";

        public int? OrganizationID { get; set; }

        // Metadata om hvem som sendte inn rapporten og hvordan den ble opprettet.
        [StringLength(100)]
        public string? SubmittedByName { get; set; }

        [StringLength(150)]
        public string? SubmittedByEmail { get; set; }

        [StringLength(100)]
        public string? CreatedBy { get; set; }

        public DateTime? ReviewedAt { get; set; }

        public string? ReviewMessage { get; set; }

        // Kobling til selve hinderdataene når rapporten beskriver en hindring.
        public virtual ObstacleData? ReportObstacle { get; set; }


        // Diverse statusfelt og filreferanser for rapporten.
        public bool IsDraft { get; set; } = false;

        public bool HasLights { get; set; }

        [StringLength(255)]
        public string? ImagePath { get; set; }

        [StringLength(100)]
        public string? Organization { get; set; }

        public Organization? OrganizationRef { get; set; }

        // Hjelpeobjekter for visning og validering som ikke lagres i databasen.
        [NotMapped]
        public UserLink? UserInfo { get; set; }
        public Pilot? Pilot { get; set; }

        [NotMapped]
        public ValidatedObstacleData? Obstacle { get; set; }

        // Konstruktører for å kunne opprette rapporter med forhåndsverdier.
        public ReportItem() { }

        public ReportItem(DateTime createdAt, string status, int? organizationID)
        {
            CreatedAt = createdAt;
            Status = status;
            OrganizationID = organizationID;
        }
    }
}
