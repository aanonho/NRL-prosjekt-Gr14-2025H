// Representerer registrarkontoer som er koblet én-til-én med en UserEntity.
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebApplication1.Models.Entities
{
    [Table("Registrar")]
    public class Registrar
    {
        // Primærnøkkelen er også en fremmednøkkel til UserEntity.
        [Key]
        [Column("UserData_UserID")]
        public int UserID { get; set; }

        // Intern avdeling registraren tilhører.
        [StringLength(45)]
        public string? Department { get; set; }

        // Navigasjonslenke til brukeren slik at EF kan laste profil og rolle.
        [ForeignKey(nameof(UserID))]
        public virtual UserEntity User { get; set; } = null!;
    }
}
