using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using WebApplication1.Models.Entities;

namespace WebApplication1.Models
{
    // Derived class with validation attributes
    [NotMapped]
    public class ValidatedObstacleData : ObstacleData
    {
        [Required(ErrorMessage = "Obstacle Name is required")]
        [MaxLength(100, ErrorMessage = "Name cannot exceed 100 characters")]
        public new string? ObstacleName { get; set; }

        [Required(ErrorMessage = "Obstacle Height is required")]
        [Range(0, 1000, ErrorMessage = "Height must be between 0 and 1000 meters")]
        public new double? ObstacleHeight { get; set; }

        [Required(ErrorMessage = "Obstacle Description is required")]
        [MaxLength(1000, ErrorMessage = "Descpriction cannot exceed 1000 characters")]
        public new string? ObstacleDescription { get; set; }

    }
}
