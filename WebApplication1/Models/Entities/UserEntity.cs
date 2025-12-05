// This file represents the base user in the system with contact info and roles.
using Microsoft.Win32;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebApplication1.Models.Entities
{
    [Table("UserData")]
    public class UserEntity
    {
        // Primary key and basic contact data for a user.
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

        [StringLength(256)]
        public string? PasswordHash { get; set; }

        // Organization affiliation to link user to owner in the database.
        [Column("Organization_OrganizationID")]
        public int? OrganizationID { get; set; }
        public virtual Organization? Organization { get; set; }

        // 1–1-relation to the pilot and registrar roles for same user.
        public virtual Pilot? Pilot { get; set; }
        public virtual Registrar? Registrar { get; set; }
    }
}
