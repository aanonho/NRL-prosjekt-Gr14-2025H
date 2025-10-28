using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebApplication1.Models
{
    // Derived class with validation attributes
    public class ValidatedObstacleData : ObstacleData
    {
        [Required(ErrorMessage = "Obstacle Name is required")]
        [MaxLength(100, ErrorMessage = "Name cannot exceed 100 characters")]
        public override string? ObstacleName { get; set; }

        [Required(ErrorMessage = "Obstacle Height is required")]
        [Range(0, 200, ErrorMessage = "Height must be between 0 and 200 meters")]     
        public override double? ObstacleHeight { get; set; }

        [Required(ErrorMessage = "Obstacle Description is required")]
        [MaxLength(1000, ErrorMessage ="Descpriction cannot exceed 1000 characters")]
        public override string? ObstacleDescription { get; set; }    

    }
}
