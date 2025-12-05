// This file models the pilot role of a user and which reports the pilot has written.
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebApplication1.Models.Entities
{
    [Table("Pilot")]
    public class Pilot
    {      
        // Primary key that also serves as foreign key to UserEntity according to the ER diagram.
        [Key, ForeignKey(nameof(User))]
        [Column("UserData_UserID")]
        public int UserID { get; set; }

        // Pilot specific fields fetched from license and aircraft type.
        [StringLength(45)]
        public string? ULicenseNumber { get; set; }

        [StringLength(45)]
        public string? AircraftType { get; set; }

        // Navigation links to the user and all reports submitted by the pilot.
        public virtual UserEntity User { get; set; } = null!;

        public virtual ICollection<ReportItem> Reports { get; set; } = new List<ReportItem>();
    }
}
