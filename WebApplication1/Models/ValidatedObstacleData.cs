using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebApplication1.Models
{
    // Derived class with validation attributes
    public class ValidatedObstacleData : ObstacleData
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

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
