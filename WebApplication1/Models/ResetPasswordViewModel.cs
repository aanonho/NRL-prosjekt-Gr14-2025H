// ViewModelen kapsler feltene som trengs når en bruker skal tilbakestille passordet sitt.
using System.ComponentModel.DataAnnotations;

namespace WebApplication1.Models
{
    // Filformål: ViewModel som håndterer innsending av nytt passord gjennom tilsendt reset-lenke.
    public class ResetPasswordViewModel
    {
        // E-postadressen identifiserer hvilket konto passordet skal oppdateres for.
        [Required]
        [EmailAddress]
        public string? Email { get; set; }

        // Token følger lenken fra e-posten og brukes for å verifisere at forespørselen er gyldig.
        [Required]
        public string? Token { get; set; }

        // Nytt passord må oppfylle lengdekrav og sendes som skjult felt fra skjemaet.
        [Required(ErrorMessage = "Password is required")]
        [StringLength(30, MinimumLength = 8, ErrorMessage = "Password must be between 8 and 30 characters")]
        [DataType(DataType.Password)]
        public string? Password { get; set; }

        // Bekrefter at bruker skriver samme passord to ganger før vi lagrer endringen.
        [Required(ErrorMessage = "Please confirm your password")]
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string? ConfirmPassword { get; set; }
    }
}
