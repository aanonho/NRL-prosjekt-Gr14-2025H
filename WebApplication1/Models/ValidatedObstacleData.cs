using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebApplication1.Models
{
    public class ValidatedObstacleData : ObstacleData
    {
        [Required(ErrorMessage = "Field is required")]
        [MaxLength(100)]
        public override string? ObstacleName { get; set; }
        [Required(ErrorMessage = "Field is required")]
        [Range(0, 200)]
        public override double? ObstacleHeight { get; set; }
        [Required(ErrorMessage = "Field is required")]
        [MaxLength(1000)]
        public override string? ObstacleDescription { get; set; }
    }
}
