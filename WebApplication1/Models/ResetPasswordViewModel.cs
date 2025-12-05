// The ViewModel encapsulates the fields needed when a user is resetting their password.
using System.ComponentModel.DataAnnotations;

namespace WebApplication1.Models
{
    // File purpose: ViewModel that handles submission of a new password via a sent reset link.
    public class ResetPasswordViewModel
    {
        // E-mail address identifies which account the password is to be updated for.
        [Required]
        [EmailAddress]
        public string? Email { get; set; }

        // Token follows the link from the email and is used to verify that the request is valid.
        [Required]
        public string? Token { get; set; }

        // New password must meet length requirements and is sent as a hidden field from the form.
        [Required(ErrorMessage = "Password is required")]
        [StringLength(30, MinimumLength = 8, ErrorMessage = "Password must be between 8 and 30 characters")]
        [DataType(DataType.Password)]
        public string? Password { get; set; }

        // Confirms that the user enters the same password twice before saving the change.
        [Required(ErrorMessage = "Please confirm your password")]
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string? ConfirmPassword { get; set; }
    }
}
