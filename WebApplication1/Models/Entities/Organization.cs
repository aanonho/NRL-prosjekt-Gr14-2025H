using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebApplication1.Models.Entities
{
    [Table("Organization")]
    public class Organization
    {
        [Key]
        [Column("OrganizationID")]
        public int OrganizationID { get; set; }

        [Required, StringLength(45)]
        public string Name { get; set; } = string.Empty;

        [StringLength(100)]
        public string? Address { get; set; }

        public ICollection<UserEntity> Users { get; set; } = new List<UserEntity>();
        public ICollection<ReportItem> Reports { get; set; } = new List<ReportItem>();
    }
}
