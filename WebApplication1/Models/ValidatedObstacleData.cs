using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebApplication1.Models
{
    public class ValidatedObstacleData : ObstacleData
    {
        [Required(ErrorMessage = "Field is required")]
        [MaxLength(100)]
        public new string? ObstacleName { get; set; }
        [Required(ErrorMessage = "Field is required")]
        [Range(0, 200)]
        public new double? ObstacleHeight { get; set; }
        [Required(ErrorMessage = "Field is required")]
        [MaxLength(1000)]
        public new string? ObstacleDescription { get; set; }
    }
}
