using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.Eventing.Reader;
using System.Linq;





namespace WebApplication1.Models
{
    // Very simple "report" record used only for listing on the Raports page.
    // We copy the most relevant fields from ObstacleData and add placeholders
    // for fields we will support later (Status, Organization).
    public partial class ReportItem
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
       // public ObstacleData Obstacle { get; set; } = new(); // Composition instead of duplication and inheritance
        public bool IsDraft { get; set; } = false;
        public string Status { get; set; } = "Pending";

        //Basic linking info
        
        public string? Organization { get; set; } = "Unknown";// Also not in the form yet. We'll default to "Unknown" so the filter still works.
        public string? SubmittedByName { get; set; }
        public string? SubmittedByEmail { get; set; }
        public string? CreatedBy { get; set; }
        public DateTime? ReviewedAt { get; set; } // when registrar reviewed it
        public string? ReviewMessage { get; set; } // registrar comment


        //Optional nested user info (from partial Userlink)
        public UserLink? UserInfo { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public ValidatedObstacleData? Obstacle {  get; set; }

        //public static implicit operator ReportItem(ReportItem v)
        //{
        //    throw new NotImplementedException();
        //}
    }
}

