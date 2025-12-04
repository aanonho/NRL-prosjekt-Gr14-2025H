// Filen modellerer pilot-rollen til en bruker og hvilke rapporter piloten har skrevet.
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebApplication1.Models.Entities
{
    [Table("Pilot")]
    public class Pilot
    {
        // Primærnøkkel som også fungerer som fremmednøkkel mot UserEntity i henhold til ER-diagrammet.
        [Key, ForeignKey(nameof(User))]
        [Column("UserData_UserID")]
        public int UserID { get; set; }

        // Pilotspesifikke felt hentet fra lisens og flytype.
        [StringLength(45)]
        public string? ULicenseNumber { get; set; }

        [StringLength(45)]
        public string? AircraftType { get; set; }

        // Navigasjonskoblinger til brukeren og alle rapporter piloten har levert.
        public virtual UserEntity User { get; set; } = null!;

        public virtual ICollection<ReportItem> Reports { get; set; } = new List<ReportItem>();
    }
}
