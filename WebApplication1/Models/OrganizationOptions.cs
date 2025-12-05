// Helper class that collects the names of approved organizations and provides validation methods.
using System;
using System.Collections.Generic;
using System.Linq;

namespace WebApplication1.Models
{
    public static class OrganizationOptions
    {
        // Fixed list of organizations allowed access to the system.
        public static readonly IReadOnlyList<string> AllowedOrganizations = new List<string>
        {
            "Avinor",
            "Luftfartstilsynet",
            "Forsvaret",
            "Norsk Luftambulanse",
            "Politiets helikoptertjeneste",
            "Kartverket"
        };

        // Removes whitespace and matches against existing names regardless of case.
        public static string? NormalizeName(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return null;
            }

            return AllowedOrganizations
                .FirstOrDefault(o => string.Equals(o, value.Trim(), StringComparison.OrdinalIgnoreCase));
        }

        // Quick validation that reuses normalization for consistency.
        public static bool IsValid(string? value) => NormalizeName(value) != null;
    }
}
