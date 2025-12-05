// The model is used for the "forgot password" form and contains only the email address with validation.
using System.ComponentModel.DataAnnotations;

namespace WebApplication1.Models
{
    public class ForgotPasswordViewModel
    {
        // Required email to send reset link and validated to a valid email format.
        [Required]
        [EmailAddress]
        public string? Email { get; set; }
    }
}
