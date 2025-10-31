using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.Eventing.Reader;
using System.Linq;

namespace WebApplication1.Models
{
    // Using partial class to separate user-specific extensions
    public partial class ReportItem { }

    public class UserLink
    {
        [Key]
        public int Id { get; set; }

        public string SubmittedByEmail { get; set; } = string.Empty;
        public string SubmittedByName { get; set; } = string.Empty;
        public string OrganizationName { get; set; } = string.Empty;
        public string? ReviewMessage { get; set; }

        // Helper display function
        public string GetDisplayName() => $"{SubmittedByName} ({OrganizationName})";
    }


}
