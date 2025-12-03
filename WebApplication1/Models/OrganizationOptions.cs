using System;
using System.Collections.Generic;
using System.Linq;

namespace WebApplication1.Models
{
    public static class OrganizationOptions
    {
        public static readonly IReadOnlyList<string> AllowedOrganizations = new List<string>
        {
            "Avinor",
            "Luftfartstilsynet",
            "Forsvaret",
            "Norsk Luftambulanse",
            "Politiets helikoptertjeneste",
            "Kartverket"
        };

        public static string? NormalizeName(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return null;
            }

            return AllowedOrganizations
                .FirstOrDefault(o => string.Equals(o, value.Trim(), StringComparison.OrdinalIgnoreCase));
        }

        public static bool IsValid(string? value) => NormalizeName(value) != null;
    }
}
