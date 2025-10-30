using System.ComponentModel.DataAnnotations;

namespace WebApplication1.Models
{
    public class UserData
    {
        [Required(ErrorMessage = "Name is required")]
        [StringLength(100, ErrorMessage = "Name cannot exceed 100 characters")]
        public string? Name { get; set; }

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Please enter valid email address")]
        public string? Email { get; set; }

        [Phone(ErrorMessage = "Please enter a valid phone number")]
        [StringLength(15, ErrorMessage = "Phone number cannot exceed 15 characters")]
        public string? Phone { get; set; }

        [StringLength(200, ErrorMessage = "Address cannot exceed 200 characters")]
        public string? Address { get; set; }

        // Additional properties
        [Required(ErrorMessage = "Role is required")]
        public string? Role { get; set; }

        [StringLength(100, ErrorMessage = "Organization name cannot exceed 100 characters")]
        public string? Organization { get; set; }
    }
}