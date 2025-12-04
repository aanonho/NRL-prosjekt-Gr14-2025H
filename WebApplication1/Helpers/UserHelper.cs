// Små hjelpefunksjoner for å hente ut brukerinformasjon i kontrollerne
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;

namespace WebApplication1.Helpers
{
    public class UserHelper
    {
        // Forsøker å finne e-posten til innlogget bruker fra tilgjengelige kilder
        public static string? GetCurrentUserEmail(Controller controller)
        {
            // Først sjekker vi om TempData har e-posten lagret etter en redirect
            if (controller.TempData.TryGetValue("CurrentUserEmail", out var emailObj) && emailObj is string email && !string.IsNullOrWhiteSpace(email))
            {
                return email;
            }

            // Som backup bruker vi identiteten fra HttpContext når ekte login er aktivert
            var identityEmail = controller.User?.Identity?.Name;
            if (!string.IsNullOrWhiteSpace(identityEmail))
            {
                return identityEmail;
            }

            // Ingen treff, så vi signaliserer at e-posten ikke er tilgjengelig
            return null;
        }
    }
}
