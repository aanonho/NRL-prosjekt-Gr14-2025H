// Reprresents registrar accounts that are linked one-to-one with a UserEntity.
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebApplication1.Models.Entities
{
    [Table("Registrar")]
    public class Registrar
    {
        // Primary key is also a foreign key to UserEntity.
        [Key]
        [Column("UserData_UserID")]
        public int UserID { get; set; }

        // The internal department the registrar belongs to.
        [StringLength(45)]
        public string? Department { get; set; }

        // Navigation link to the user so that EF can load profile and role.
        [ForeignKey(nameof(UserID))]
        public virtual UserEntity User { get; set; } = null!;
    }
}
