// Modellen brukes for "glemt passord"-skjemaet og holder kun e-postadresse med validering.
using System.ComponentModel.DataAnnotations;

namespace WebApplication1.Models
{
    public class ForgotPasswordViewModel
    {
        // Påkrevd e-post for å sende reset-lenke og validert til gyldig e-postformat.
        [Required]
        [EmailAddress]
        public string? Email { get; set; }
    }
}
