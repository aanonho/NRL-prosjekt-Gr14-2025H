// This file describes an obstacle report with status, ownership info, and navigation links.
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using WebApplication1.Models.Entities;

namespace WebApplication1.Models
{
    [Table("ReportItem")]
    public partial class ReportItem
    {
        // Identifier and timestamp for the report, along with reference to the pilot who submitted it.
        [Key]
        [Column("ReportID")]
        public int ReportID { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        [Required]
        public int PilotID { get; set; }

        // Status and organization affiliation control workflow and access.
        [Required]
        [StringLength(32)]
        public string Status { get; set; } = "Pending";

        public int? OrganizationID { get; set; }

        // Metadata about who submitted the report and how it was created.
        [StringLength(100)]
        public string? SubmittedByName { get; set; }

        [StringLength(150)]
        public string? SubmittedByEmail { get; set; }

        [StringLength(100)]
        public string? CreatedBy { get; set; }

        public DateTime? ReviewedAt { get; set; }

        public string? ReviewMessage { get; set; }

        // Link to the actual obstacle data when the report describes an obstacle.
        public virtual ObstacleData? ReportObstacle { get; set; }


        // Various status fields and file references for the report.
        public bool IsDraft { get; set; } = false;

        public bool HasLights { get; set; }

        [StringLength(255)]
        public string? ImagePath { get; set; }

        [StringLength(100)]
        public string? Organization { get; set; }

        public Organization? OrganizationRef { get; set; }

        // Helper objects for display and validation that are not stored in the database.
        [NotMapped]
        public UserLink? UserInfo { get; set; }
        public Pilot? Pilot { get; set; }

        [NotMapped]
        public ValidatedObstacleData? Obstacle { get; set; }

        // Constructor for creating a report with preset values.
        public ReportItem() { }

        public ReportItem(DateTime createdAt, string status, int? organizationID)
        {
            CreatedAt = createdAt;
            Status = status;
            OrganizationID = organizationID;
        }
    }
}
