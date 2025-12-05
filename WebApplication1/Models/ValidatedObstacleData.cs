// Extenced obstacle class combining database level with model validation for the forms.
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using WebApplication1.Models.Entities;

namespace WebApplication1.Models
{
    // File purpose: Inherits ObstacleData and adds validation rules for the forms that manage obstacles.
    [NotMapped]
    public class ValidatedObstacleData : ObstacleData
    {
        // Name, height, and description must be filled out within defined limits before they can be saved to the database.
        [Required(ErrorMessage = "Obstacle Name is required")]
        [MaxLength(100, ErrorMessage = "Name cannot exceed 100 characters")]
        public new string? ObstacleName { get; set; }

        [Required(ErrorMessage = "Obstacle Height is required")]
        [Range(0.0, 1000, ErrorMessage = "Height must be between 0 and 1000 meters")]
        public new double? ObstacleHeight { get; set; }

        [Required(ErrorMessage = "Obstacle Description is required")]
        [MaxLength(1000, ErrorMessage = "Descpriction cannot exceed 1000 characters")]
        public new string? ObstacleDescription { get; set; }

        // Holds a comma-separated list of image files to be deleted along with the update.
        public string? ImagesToRemove { get; set; }

    }
}
