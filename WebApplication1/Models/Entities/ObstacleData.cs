// This file describes how an obstacle is stored in the database, including geometry and linkage to reports.
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebApplication1.Models.Entities
{
    [Table("ObstacleData")]
    public class ObstacleData
    {
        // Primaty key and basic fields describing the obstacle's identity and name.
        [Key]
        [Column("ObstacleID")]
        public int ObstacleID { get; set; }

        [StringLength(50)]
        public string? ObstacleName { get; set; }

        public double? ObstacleHeight { get; set; }

        public string? ObstacleDescription { get; set; }

        public bool ObstacleHasLight { get; set; }

        // The postion and geometry of the obstacle, so it can be drawn on the map.
        public double? ObstacleLatitude { get; set; }

        public double? ObstacleLongitude { get; set; }

        [StringLength(50)]
        public string? ObstacleType { get; set; }

        public double? ObstacleRadius { get; set; }

        public string? ObstacleGeoJson { get; set; }

        public string? ObstacleLineCoordinates { get; set; }

        [StringLength(50)]
        public string? ObstacleLineLength { get; set; }

        // Uploaded image and the time when the obstacle was registered.
        [StringLength(255)]
        public string? ImagePath { get; set; }

        public DateTime ObstacleRegistrationTime { get; set; } = DateTime.UtcNow;

        // Local time is calculated as needed for UI display, but not stored in the database.
        [NotMapped]
        public DateTime ObstacleRegistrationLocalTime =>
            TimeZoneInfo.ConvertTimeFromUtc(
                ObstacleRegistrationTime,
                TimeZoneInfo.FindSystemTimeZoneById("Central European Standard Time")
            );

        // Status field and link to the report that owns the obstacle.
        public bool IsDraft { get; set; } = true;

        [ForeignKey("ReportItem")]
        public int ReportID { get; set; }

        public virtual ReportItem? ReportItem { get; set; }
    }
}
