using System.ComponentModel.DataAnnotations;

namespace WebApplication1.Models
{
    public class ObstacleData
    {
        [Required(ErrorMessage = "Field is required")]
        [MaxLength(100)]
        public string? ObstacleName { get; set; }
        [Required(ErrorMessage = "Field is required")]
        [Range(0, 200)]
        public double? ObstacleHeight { get; set; }
        [Required(ErrorMessage = "Field is required")]
        [MaxLength(1000)]
        public string? ObstacleDescription { get; set; }
        public double? ObstacleLatitude { get; set; }
        public double? ObstacleLongitude { get; set; }

        public DateTime ObstacleRegistrationTime { get; set; } = DateTime.Now;

        // Nye egenskaper for å støtte ulike former
        public string? ObstacleType { get; set; } // "marker", "circle" eller "line"
        public double? ObstacleRadius { get; set; } // bare relevant for sirkel
        public string? ObstacleLineCoords { get; set; }

    }
}
