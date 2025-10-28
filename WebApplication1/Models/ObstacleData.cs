using System.ComponentModel.DataAnnotations.Schema;

namespace WebApplication1.Models
{
    public class ObstacleData
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public virtual string? ObstacleName { get; set; }

        public virtual double? ObstacleHeight { get; set; }

        public virtual string? ObstacleDescription { get; set; }

        public double? ObstacleLatitude { get; set; }
        public double? ObstacleLongitude { get; set; }
        public string? ObstacleType { get; set; } // Type of obstacle (e.g., "circle", "line", etc.)    
        public double? ObstacleRadius { get; set; } // For circular obstacles
        public string? ObstacleGeometry { get; set; } // JSON representation of the obstacle's geometry
        public string? ObstacleLineCoordinates { get; set; } // For line obstacles, stores coordinates as a string
        public string? ObstacleLineLength { get; set; }

        public string? ImagePath { get; set; } // Path to the uploaded image file
        public DateTime ObstacleRegistrationTime { get; set; } = DateTime.UtcNow; //saves in UTC

        [NotMapped]
        public DateTime ObstacleRegistrationLocalTime =>
            TimeZoneInfo.ConvertTimeFromUtc(
                ObstacleRegistrationTime,
                TimeZoneInfo.FindSystemTimeZoneById("Central European Standard Time")
            );

        public bool IsDraft { get; set; } = true; // Report is draft by default

        // public string? GeometryGeoJson { get; set; } // Felt som beholder koordinatene til hinderets lokasjon?


    }
}
