// Displays a user's profile in the dashboard.
using System;
using WebApplication1.Models.Entities;

namespace WebApplication1.Models
{
    public class UserProfileViewModel
    {
        // Basic contact info and role retrieved from the database.
        public string? Name { get; set; }
        public string? Email { get; set; }
        public string? Phone { get; set; }
        public string? Role { get; set; }
        public string? OrganizationName { get; set; }

        // Help flag to know if the profile has an associated user.
        public bool HasUser => !string.IsNullOrWhiteSpace(Email);

        // Role checks to control which sections are displayed in the view.
        public bool IsPilot => string.Equals(Role, "Pilot", StringComparison.OrdinalIgnoreCase);
        public bool IsRegistrar => string.Equals(Role, "Registrar", StringComparison.OrdinalIgnoreCase);

        // Displays fallback text for missing contact info.
        public string RoleDisplay => string.IsNullOrWhiteSpace(Role) ? "Unknown" : Role!;
        public string OrganizationDisplay => string.IsNullOrWhiteSpace(OrganizationName) ? "Unknown" : OrganizationName!;

        // Maps the database entity to the ViewModel so the Razor page gets clean strings to work with.
        public static UserProfileViewModel FromEntity(UserEntity entity)
        {
            return new UserProfileViewModel
            {
                Name = entity.Name,
                Email = entity.Email,
                Phone = entity.Phone,
                Role = entity.Role,
                OrganizationName = entity.Organization?.Name
            };
        }
    }
}
