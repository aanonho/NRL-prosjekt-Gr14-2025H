// ViewModel used in admin forms to create and update users.
using System.ComponentModel.DataAnnotations;

namespace WebApplication1.Models
{
    // File purpose: ViewModel for collecting and validating fields when an admin creates or edits users.
    public class UserData
    {
        // Contact information the user must fill out to be identified.
        [Required(ErrorMessage = "Name is required")]
        [StringLength(45, ErrorMessage = "Name cannot exceed 45 characters")]
        public string? Name { get; set; }

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Please enter valid email address")]
        [StringLength(45, ErrorMessage = "Email cannot exceed 45 characters")]
        [RegularExpression(@"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$", ErrorMessage = "Please enter a valid email address")]
        public string? Email { get; set; }

        [Required(ErrorMessage = "Phone is required")]
        [Phone(ErrorMessage = "Please enter a valid phone number")]
        [RegularExpression(@"^(?=(?:\D*\d){8,15}\D*$)[\d\s\-\(\)\+]+$", ErrorMessage = "Please enter a valid phone number with 8 to 15 digits")]
        public string? Phone { get; set; }

        // Optional adress information displayed in user details.
        [StringLength(200, ErrorMessage = "Address cannot exceed 200 characters")]
        public string? Address { get; set; }

        // Roles and affiliation determine which parts of the system the user can access.
        [Required(ErrorMessage = "Role is required")]
        [StringLength(45, ErrorMessage = "Role cannot exceed 45 characters")]
        public string? Role { get; set; }

        [Required(ErrorMessage = "Organization is required")]
        [StringLength(45, ErrorMessage = "Organization name cannot exceed 45 characters")]
        public string? Organization { get; set; }

        // New password requirements for account creation and confirmation before saving.
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
