// ViewModel for capturing username, password, and navigation info from the login form.
using System.ComponentModel.DataAnnotations;

namespace WebApplication1.Models
{
    // File purpose: Simple viewmodel that collects the login fields submitted from the login form.
    public class LoginViewModel
    {
        // Required email address used as username for authentication.
        [Required]
        [EmailAddress]
        public string? Email { get; set; }

        // Password field marked as secret and validated to ensure user fills it out.
        [Required]
        [DataType(DataType.Password)]
        public string? Password { get; set; }

        // The "Remember me" flag determines wheter cookie gets longer lifetime between visits.
        [Display(Name = "Remember me")]
        public bool RememberMe { get; set; }

        // URL to return to after login so user can go back to desired page.
        public string? ReturnUrl { get; set; }
    }
}
