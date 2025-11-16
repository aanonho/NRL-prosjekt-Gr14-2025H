using System;
using WebApplication1.Models.Entities;

namespace WebApplication1.Models
{
    public class UserProfileViewModel
    {
        public string? Name { get; set; }
        public string? Email { get; set; }
        public string? Phone { get; set; }
        public string? Role { get; set; }
        public string? OrganizationName { get; set; }

        public bool HasUser => !string.IsNullOrWhiteSpace(Email);

        public bool IsPilot => string.Equals(Role, "Pilot", StringComparison.OrdinalIgnoreCase);
        public bool IsRegistrar => string.Equals(Role, "Registrar", StringComparison.OrdinalIgnoreCase);

        public string RoleDisplay => string.IsNullOrWhiteSpace(Role) ? "Unknown" : Role!;
        public string OrganizationDisplay => string.IsNullOrWhiteSpace(OrganizationName) ? "Unknown" : OrganizationName!;

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
