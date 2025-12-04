// ViewModel brukt i administrasjonsskjemaene for å opprette og oppdatere brukere.
using System.ComponentModel.DataAnnotations;

namespace WebApplication1.Models
{
    // Filformål: ViewModel for å samle inn og validere felter når en admin oppretter eller redigerer brukere.
    public class UserData
    {
        // Kontaktinformasjon brukeren må fylle ut for å kunne identifiseres.
        [Required(ErrorMessage = "Name is required")]
        [StringLength(45, ErrorMessage = "Name cannot exceed 45 characters")]
        public string? Name { get; set; }

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Please enter valid email address")]
        [StringLength(45, ErrorMessage = "Email cannot exceed 45 characters")]
        public string? Email { get; set; }

        [Required(ErrorMessage = "Phone is required")]
        [Phone(ErrorMessage = "Please enter a valid phone number")]
        [StringLength(45, ErrorMessage = "Phone number cannot exceed 45 characters")]
        public string? Phone { get; set; }

        // Valgfri adresseinformasjon som vises i brukerdetaljer.
        [StringLength(200, ErrorMessage = "Address cannot exceed 200 characters")]
        public string? Address { get; set; }

        // Roller og tilknytning styrer hvilke deler av systemet brukeren får tilgang til.
        [Required(ErrorMessage = "Role is required")]
        [StringLength(45, ErrorMessage = "Role cannot exceed 45 characters")]
        public string? Role { get; set; }

        [Required(ErrorMessage = "Organization is required")]
        [StringLength(45, ErrorMessage = "Organization name cannot exceed 45 characters")]
        public string? Organization { get; set; }

        // Nye passordkrav for konto-opprettelse og bekreftelse før lagring.
        [Required(ErrorMessage = "Password is required")]
        [StringLength(30, MinimumLength = 8, ErrorMessage = "Password must be between 8 and 30 characters")]
        [DataType(DataType.Password)]
        public string? Password { get; set; }

        [Required(ErrorMessage = "Please confirm your password")]
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string? ConfirmPassword { get; set; }
    }
}
