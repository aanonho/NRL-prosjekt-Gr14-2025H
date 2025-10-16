using System;
using System.Diagnostics.Eventing.Reader;

namespace WebApplication1.Models
{
    // Very simple "report" record used only for listing on the Raports page.
    // We copy the most relevant fields from ObstacleData and add placeholders
    // for fields we will support later (Status, Organization).
    public class ReportItem
    {
        /*
        
        public string? Title { get; set; }              // From ObstacleName
        public string? Description { get; set; }        // From ObstacleDescription
        public double? Height { get; set; }             // From ObstacleHeight
        public double? Latitude { get; set; }           // From ObstacleLatitude
        public double? Longitude { get; set; }          // From ObstacleLongitude
        public string? ObstacleType { get; set; }               // From ObstacleType (marker/circle/line)
        
        public double? Radius { get; set; }             // Is circle
        public string? LineCoords { get; set; }         // If line
        */
        public Guid Id { get; set; } = Guid.NewGuid();
        public ObstacleData Obstacle { get; set; } = new(); // Composition instead of duplication and inheritance
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public bool IsDraft { get; set; } = false;      // From IsDraft, here default false

        // Not in the form yet — added now to make the list "future-ready":
        // Valid values we expect later: "Pending", "Approved", "Rejected".
        public string Status { get; set; } = "Pending";

        // Also not in the form yet. We'll default to "Unknown" so the filter still works.
        public string Organization { get; set; } = "Unknown";
    }
}

