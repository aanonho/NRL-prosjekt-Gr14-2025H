using System.ComponentModel.DataAnnotations;

namespace WebApplication1.Models
{
    // Using partial class to separate user-specific extensions
    public partial class ReportItem
    {
        public class UserLink
        {
            [Key]
            public Guid  Id { get; set; } = Guid.NewGuid();

            public string SubmittedByEmail { get; set; } = string.Empty;

            public string SubmittedByName { get; set; } = string.Empty;
            public string OrganizationName { get; set; } = string.Empty;
            public string? ReviewMessage { get; set; }
            // Method for debugging/logging purposes
            public string GetDisplayName() => $"{SubmittedByName} ({OrganizationName})";
        }
    }
}
