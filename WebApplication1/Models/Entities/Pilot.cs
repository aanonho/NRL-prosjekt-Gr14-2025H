using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebApplication1.Models.Entities
{
    [Table("Pilot")]
    public class Pilot
    {
        // PK = FK to UserEntity per diagram
        [Key, ForeignKey(nameof(User))]
        [Column("UserData_UserID")]
        public int UserID { get; set; }

        [StringLength(45)]
        public string? ULicenseNumber { get; set; }

        [StringLength(45)]
        public string? AircraftType { get; set; }

        public UserEntity User { get; set; } = null!;

        // Reports written by this pilot
        public ICollection<ReportItem> Reports { get; set; } = new List<ReportItem>();
    }
}
