// Viser en brukers profilinformasjon i dashboardet.
using System;
using WebApplication1.Models.Entities;

namespace WebApplication1.Models
{
    public class UserProfileViewModel
    {
        // Grunnleggende kontaktinformasjon og rolle hentet fra databasen.
        public string? Name { get; set; }
        public string? Email { get; set; }
        public string? Phone { get; set; }
        public string? Role { get; set; }
        public string? OrganizationName { get; set; }

        // Hjelpeflagg for å vite om profilen har en tilknyttet bruker.
        public bool HasUser => !string.IsNullOrWhiteSpace(Email);

        // Rollesjekker for å styre hvilke seksjoner som vises i viewet.
        public bool IsPilot => string.Equals(Role, "Pilot", StringComparison.OrdinalIgnoreCase);
        public bool IsRegistrar => string.Equals(Role, "Registrar", StringComparison.OrdinalIgnoreCase);

        // Viser fallback-tekst når rolle eller organisasjon mangler.
        public string RoleDisplay => string.IsNullOrWhiteSpace(Role) ? "Unknown" : Role!;
        public string OrganizationDisplay => string.IsNullOrWhiteSpace(OrganizationName) ? "Unknown" : OrganizationName!;

        // Mapper databasenhet til ViewModel slik at Razor-siden får rene strenger å jobbe med.
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
