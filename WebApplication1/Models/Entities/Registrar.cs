using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebApplication1.Models.Entities
{
    [Table("Registrar")]
    public class Registrar
    {
        // PK = FK to UserEntity per diagram
        [Key]
        [Column("UserData_UserID")]
        public int UserID { get; set; }

        [StringLength(45)]
        public string? Department { get; set; }

        // Navigation property to user
        [ForeignKey(nameof(UserID))]
        public virtual UserEntity User { get; set; } = null!;
    }
}
