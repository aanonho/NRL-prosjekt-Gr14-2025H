using System.ComponentModel.DataAnnotations;

namespace WebApplication1.Models
{
    public class UserData
    {
        [Required(ErrorMessage = "Name is required")]
        [StringLength(45, ErrorMessage = "Name cannot exceed 45 characters")]
        [RegularExpression("^[A-Za-zæøåÆØÅ\\- ]+$", ErrorMessage = "Name may only contain letters, spaces, and hyphens")]
        public string? Name { get; set; }

        [Required(ErrorMessage = "Email is required")]
        [StringLength(45, ErrorMessage = "Email cannot exceed 45 characters")]
        [DataType(DataType.EmailAddress)]
        [RegularExpression("^[A-Za-z0-9._%+-]+@[A-Za-z0-9.-]+\\.[A-Za-z]{2,}$", ErrorMessage = "Please enter a valid email address")]
        public string? Email { get; set; }

        [Required(ErrorMessage = "Phone is required")]
        [StringLength(45, ErrorMessage = "Phone cannot exceed 45 characters")]
        [DataType(DataType.PhoneNumber)]
        [RegularExpression(@"^(\+?\d{1,3}[\s-]?)?\d{8,12}$", ErrorMessage = "Phone must contain 8–12 digits and may include country code (+47 or 0047)")]
        public string? Phone { get; set; }

        //Not in db yet(UserEntity needs Address)
        [StringLength(200, ErrorMessage = "Address cannot exceed 200 characters")]
        [RegularExpression(@"^[A-Za-z0-9ÆØÅæøå .,\-\/#]*$", ErrorMessage = "Address contains invalid characters")]
        public string? Address { get; set; }

        // Additional properties
        [Required(ErrorMessage = "Role is required")]
        [StringLength(45, ErrorMessage = "Role cannot exceed 45 characters")]
        public string? Role { get; set; }

        [Required(ErrorMessage = "Organization is required")]
        [StringLength(45, ErrorMessage = "Organization name cannot exceed 45 characters")]
        public string? Organization { get; set; }

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