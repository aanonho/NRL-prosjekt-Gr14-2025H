// This file describes an organization that owns users and reports.
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebApplication1.Models.Entities
{
    [Table("Organization")]
    public class Organization
    {
        // Keys and identity fields that name the organization in the database.
        [Key]
        [Column("OrganizationID")]
        public int OrganizationID { get; set; }

        [Required, StringLength(45)]
        public string Name { get; set; } = string.Empty;

        [StringLength(100)]
        public string? Address { get; set; }

        // Navigaiton properties for linking organization to users and reports.
        public ICollection<UserEntity> Users { get; set; } = new List<UserEntity>();
        public ICollection<ReportItem> Reports { get; set; } = new List<ReportItem>();
    }
}
