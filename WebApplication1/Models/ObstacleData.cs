using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebApplication1.Models
{
    public class ObstacleData
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public string? ObstacleName { get; set; }

        public double? ObstacleHeight { get; set; }
 
        public string? ObstacleDescription { get; set; }
        public double? ObstacleLatitude { get; set; }
        public double? ObstacleLongitude { get; set; }

        public DateTime ObstacleRegistrationTime { get; set; } = DateTime.Now;
        public bool IsDraft { get; set; } = true; // Default at rapport lagres som utkast

        // public string? GeometryGeoJson { get; set; } // Felt som beholder koordinatene til hinderets lokasjon?

        // Nye egenskaper for å støtte ulike former
        public string? ObstacleType { get; set; } // "marker", "circle" eller "line"
        public double? ObstacleRadius { get; set; } // bare relevant for sirkel
        public string? ObstacleLineCoords { get; set; }

    }
}
