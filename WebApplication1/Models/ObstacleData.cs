using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebApplication1.Models
{
    public class ObstacleData
    {
        public int ObstacleID { get; set; } // trengs for å kunne sette en PK i database

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
        public bool IsDraft { get; set; } = true; // Default at rapport lagres som utkast


        // Nye egenskaper for å støtte ulike former
        public string? ObstacleType { get; set; } // "marker", "circle" eller "line"
        public double? ObstacleRadius { get; set; } // bare relevant for sirkel
        public string? ObstacleLineCoords { get; set; }

    }
}
