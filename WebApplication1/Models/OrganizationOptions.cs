// Hjelpeklasse som samler navn på godkjente organisasjoner og tilbyr valideringsmetoder.
using System;
using System.Collections.Generic;
using System.Linq;

namespace WebApplication1.Models
{
    public static class OrganizationOptions
    {
        // Fast liste over organisasjoner som har tilgang til systemet.
        public static readonly IReadOnlyList<string> AllowedOrganizations = new List<string>
        {
            "Avinor",
            "Luftfartstilsynet",
            "Forsvaret",
            "Norsk Luftambulanse",
            "Politiets helikoptertjeneste",
            "Kartverket"
        };

        // Fjerner mellomrom og matcher med eksisterende navn uavhengig av små/store bokstaver.
        public static string? NormalizeName(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return null;
            }

            return AllowedOrganizations
                .FirstOrDefault(o => string.Equals(o, value.Trim(), StringComparison.OrdinalIgnoreCase));
        }

        // Rask validering som gjenbruker normaliseringen for konsistens.
        public static bool IsValid(string? value) => NormalizeName(value) != null;
    }
}