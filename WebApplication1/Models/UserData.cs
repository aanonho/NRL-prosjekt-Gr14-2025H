using System.ComponentModel.DataAnnotations;

namespace WebApplication1.Models
{
    public class UserData
    {
        public string? Name { get; set; }
        public string? Email { get; set; }
        public string? Phone { get; set; }
        public string? Address { get; set; }

        //Til registrar for auth - endring i rapportstatus
        [Required]
        public string Username { get; set; }

        [Required]
        public string Password { get; set; } //kan endres eller fjernes hvis det ikke er prio

        [Required]
        public string Role { get; set; } = "Pilot"; //Default rolle er pilot
    }
}