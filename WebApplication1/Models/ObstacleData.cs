using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebApplication1.Models
{
    [Table("ObstacleData")]
    public class ObstacleData
    {
        [Key]
        [Column("ObstacleID")]
        public int ObstacleID { get; set; }

        [StringLength(50)]
        public string? ObstacleName { get; set; }

        public double? ObstacleHeight { get; set; }

        public string? ObstacleDescription { get; set; }

        public double? ObstacleLatitude { get; set; }

        public double? ObstacleLongitude { get; set; }

        [StringLength(50)]
        public string? ObstacleType { get; set; }

        public double? ObstacleRadius { get; set; }

        public string? ObstacleGeometry { get; set; }

        public string? ObstacleLineCoordinates { get; set; }

        [StringLength(50)]
        public string? ObstacleLineLength { get; set; }

        [StringLength(255)]
        public string? ImagePath { get; set; }

        public DateTime ObstacleRegistrationTime { get; set; } = DateTime.UtcNow;

        [NotMapped]
        public DateTime ObstacleRegistrationLocalTime =>
            TimeZoneInfo.ConvertTimeFromUtc(
                ObstacleRegistrationTime,
                TimeZoneInfo.FindSystemTimeZoneById("Central European Standard Time")
            );

        public bool IsDraft { get; set; } = true;

        [ForeignKey("ReportItem")]
        public int ReportID { get; set; }

        public virtual ReportItem? ReportItem { get; set; }
    }
}
