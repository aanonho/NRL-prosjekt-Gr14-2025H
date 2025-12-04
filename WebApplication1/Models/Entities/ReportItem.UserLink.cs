// Ekstra helper-klasse for rapporter som gir brukerspesifikk visning av innsender.
using System.ComponentModel.DataAnnotations;

namespace WebApplication1.Models
{
    // Deler opp ReportItem i en partial for å knytte sammen rapport og brukerinfo i view-modellen.
    public partial class ReportItem { }

    public class UserLink
    {
        // Egen primærnøkkel for helperen.
        [Key]
        public int Id { get; set; }

        // Navn og e-post til innsender, samt organisasjon slik de skal vises.
        public string SubmittedByEmail { get; set; } = string.Empty;
        public string SubmittedByName { get; set; } = string.Empty;
        public string OrganizationName { get; set; } = string.Empty;
        public string? ReviewMessage { get; set; }

        // Helper for å vise navnet konsistent i tabeller og lister.
        public string GetDisplayName() => $"{SubmittedByName} ({OrganizationName})";
    }


}
