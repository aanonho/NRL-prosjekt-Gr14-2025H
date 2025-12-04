// Filen beskriver hvordan et hinder lagres i databasen, inkludert geometri og kobling til rapporter.
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebApplication1.Models.Entities
{
    [Table("ObstacleData")]
    public class ObstacleData
    {
        // Primærnøkkel og basisfelter som beskriver hinderets identitet og navn.
        [Key]
        [Column("ObstacleID")]
        public int ObstacleID { get; set; }

        [StringLength(50)]
        public string? ObstacleName { get; set; }

        public double? ObstacleHeight { get; set; }

        public string? ObstacleDescription { get; set; }

        public bool ObstacleHasLight { get; set; }

        // Posisjon og geometri for hinderet slik at det kan tegnes på kartet.
        public double? ObstacleLatitude { get; set; }

        public double? ObstacleLongitude { get; set; }

        [StringLength(50)]
        public string? ObstacleType { get; set; }

        public double? ObstacleRadius { get; set; }

        public string? ObstacleGeoJson { get; set; }

        public string? ObstacleLineCoordinates { get; set; }

        [StringLength(50)]
        public string? ObstacleLineLength { get; set; }

        // Opplastet bilde og tidspunkt for når hinderet ble registrert.
        [StringLength(255)]
        public string? ImagePath { get; set; }

        public DateTime ObstacleRegistrationTime { get; set; } = DateTime.UtcNow;

        // Lokal tid kalkuleres ved behov for visning i UI, men lagres ikke i databasen.
        [NotMapped]
        public DateTime ObstacleRegistrationLocalTime =>
            TimeZoneInfo.ConvertTimeFromUtc(
                ObstacleRegistrationTime,
                TimeZoneInfo.FindSystemTimeZoneById("Central European Standard Time")
            );

        // Statusfelt og kobling til rapporten som eier hinderet.
        public bool IsDraft { get; set; } = true;

        [ForeignKey("ReportItem")]
        public int ReportID { get; set; }

        public virtual ReportItem? ReportItem { get; set; }
    }
}
