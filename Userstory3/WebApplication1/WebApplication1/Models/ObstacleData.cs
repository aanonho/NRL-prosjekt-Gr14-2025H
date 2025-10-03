using System.ComponentModel.DataAnnotations;

namespace WebApplication1.Models
{
    public class ObstacleData
    {
        [Key] public int Id { get; set; }

        [Required, StringLength(100)]
        public string ObstacleName { get; set; } = "";

        [Range(0, 10000)]
        public double ObstacleHeight { get; set; }

        [Required, StringLength(1000)]
        public string ObstacleDescription { get; set; } = "";

        // "Posisjonen" på selve hindret (for marker/circle = senter, for line = senter av linjens bounding box)
        [Required, Range(-90, 90)]
        public double ObstacleLatitude { get; set; }

        [Required, Range(-180, 180)]
        public double ObstacleLongitude { get; set; }

        // US3 – automatisk fanget posisjon for rapportør (skjult i skjemaet)
        public double? ReporterLatitude { get; set; }
        public double? ReporterLongitude { get; set; }
        public int? ReporterAccuracy { get; set; }
        public DateTime? CapturedAt { get; set; }

        // --- NYTT: form på hinderet ---
        // "marker" | "circle" | "polyline"
        public string? ObstacleType { get; set; }

        // Kun relevant for circle (meter)
        public double? ObstacleRadius { get; set; }

        // Kun relevant for polyline: JSON-array av [ [lat,lon], [lat,lon], ... ]
        public string? ObstacleLineCoords { get; set; }
    }
}
