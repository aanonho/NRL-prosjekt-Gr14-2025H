using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebApplication1.Models
{
    // Very simple "report" record used only for listing on the Raports page.
    // We copy the most relevant fields from ObstacleData and add placeholders
    // for fields we will support later (Status, Organization).

    [Table("Reports")]
    public class ReportItem
    {
        //  public Guid Id { get; set; } = Guid.NewGuid();
        //  public string? Title { get; set; }              // From ObstacleName
        //  public string? Description { get; set; }        // From ObstacleDescription
        //   public double? Height { get; set; }             // From ObstacleHeight
        //    public double? Latitude { get; set; }           // From ObstacleLatitude
        //    public double? Longitude { get; set; }          // From ObstacleLongitude
        //   public string? Type { get; set; }               // From ObstacleType (marker/circle/line)

        // public DateTime CreatedAt { get; set; } = DateTime.Now;

        // Not in the form yet — added now to make the list "future-ready":
        // Valid values we expect later: "Pending", "Approved", "Rejected".
        // public string Status { get; set; } = "Pending";

        // Also not in the form yet. We'll default to "Unknown" so the filter still works.
        //  public string Organization { get; set; } = "Unknown";


        // NY KODE!! - Sabrine

        [Key]
        public int Id { get; set; }

        [MaxLength(100)]
        public string Type { get; set; } = string.Empty;


        // Kolonnen heter "Organization" i databasen. Bruker feltet som "Organization"/tittel i filt/visnign
        [Column("Organization")]
        [MaxLength(100)]
        public string Organization { get; set; } = string.Empty;

        [MaxLength(100)]
        public string ReporterName { get; set; } = string.Empty;

        // Samme som over, bare "DateReported" i databasen
        [Column("DateReported")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        [MaxLength(50)]
        public string Status { get; set; } = "Pending";
    }
}

