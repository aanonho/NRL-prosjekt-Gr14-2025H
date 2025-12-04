// Filen representerer basisbrukeren i systemet med kontaktinfo og roller.
using Microsoft.Win32;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebApplication1.Models.Entities
{
    [Table("UserData")]
    public class UserEntity
    {
        // Primærnøkkel og grunnleggende kontaktdata for en bruker.
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

        // Organisasjonstilknytning for å koble bruker til eier i databasen.
        [Column("Organization_OrganizationID")]
        public int? OrganizationID { get; set; }
        public virtual Organization? Organization { get; set; }

        // 1–1-relasjoner til pilot- og registrarrollen for samme bruker.
        public virtual Pilot? Pilot { get; set; }
        public virtual Registrar? Registrar { get; set; }
    }
}