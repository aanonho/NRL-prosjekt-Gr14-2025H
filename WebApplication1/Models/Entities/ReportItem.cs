using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using WebApplication1.Models.Entities;

namespace WebApplication1.Models
{
    [Table("ReportItem")]
    public partial class ReportItem
    {
        [Key]
        [Column("ReportID")]
        public int ReportID { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        [Required]
        public int PilotID { get; set; }

        [Required]
        [StringLength(32)]
        public string Status { get; set; } = "Pending";

        public int? OrganizationID { get; set; }

        [StringLength(100)]
        public string? SubmittedByName { get; set; }

        [StringLength(150)]
        public string? SubmittedByEmail { get; set; }

        [StringLength(100)]
        public string? CreatedBy { get; set; }

        public DateTime? ReviewedAt { get; set; }

        public string? ReviewMessage { get; set; }

        //public int? ObstacleID { get; set; } // FK vers ObstacleData
        public virtual ObstacleData? ReportObstacle { get; set; }


        public bool IsDraft { get; set; } = false;

        public bool HasLights { get; set; }

        [StringLength(255)]
        public string? ImagePath { get; set; }

        [StringLength(100)]
        public string? Organization { get; set; }

        public Organization? OrganizationRef { get; set; }

        // Not use in DB for now
        [NotMapped]
        public UserLink? UserInfo { get; set; }
        public Pilot? Pilot { get; set; }

        // Not use in DB, only for form validations
        [NotMapped]
        public ValidatedObstacleData? Obstacle { get; set; }

        public ReportItem() { }

        public ReportItem(DateTime createdAt, string status, int? organizationID)
        {
            CreatedAt = createdAt;
            Status = status;
            OrganizationID = organizationID;
        }
    }
}
