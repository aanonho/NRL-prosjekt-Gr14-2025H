// ViewModel for å hente inn brukernavn, passord og navigasjonsinfo fra login-skjemaet.
using System.ComponentModel.DataAnnotations;

namespace WebApplication1.Models
{
    // Filformål: Enkel viewmodel som samler innloggingsfeltene som sendes fra login-skjemaet.
    public class LoginViewModel
    {
        // Påkrevd e-postadresse som brukes som brukernavn i autentisering.
        [Required]
        [EmailAddress]
        public string? Email { get; set; }

        // Passordfelt merket som hemmelig og validert for at brukeren må fylle det ut.
        [Required]
        [DataType(DataType.Password)]
        public string? Password { get; set; }

        // Husk meg flagget styrer om cookie får lengre levetid mellom besøk.
        [Display(Name = "Remember me")]
        public bool RememberMe { get; set; }

        // URL å returnere til etter innlogging slik at bruker kan gå tilbake til ønsket side.
        public string? ReturnUrl { get; set; }
    }
}
