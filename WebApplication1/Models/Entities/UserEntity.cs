using Microsoft.Win32;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebApplication1.Models.Entities
{
    [Table("UserData")]
    public class UserEntity
    {
        [Key]
        [Column("UserID")]
        public int UserID { get; set; }

        [StringLength(45)]
        public string? Name { get; set; }

        [StringLength(45)]
        public string? Email { get; set; }

        [StringLength(45)]
        public string? Phone { get; set; }

        [StringLength(45)]
        public string? Role { get; set; }

        // Matches the diagram’s column name
        [Column("Organization_OrganizationID")]
        public int? OrganizationID { get; set; }
        public  virtual Organization? Organization { get; set; }

        public virtual Pilot? Pilot { get; set; }          // 1–1
        public virtual Registrar? Registrar { get; set; }  // 1–1
    }
}
