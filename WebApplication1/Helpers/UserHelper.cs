// Small helper functions to retrieve user information in controllers
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;

namespace WebApplication1.Helpers
{
    public class UserHelper
    {
        // Tries to get the email of the currently logged-in user from available sources.
        public static string? GetCurrentUserEmail(Controller controller)
        {
            // Checking TempData first for the email stored after a redirect.
            if (controller.TempData.TryGetValue("CurrentUserEmail", out var emailObj) && emailObj is string email && !string.IsNullOrWhiteSpace(email))
            {
                return email;
            }

            // As fallback, we use indentity from HttpContext when real login is enabled.
            var identityEmail = controller.User?.Identity?.Name;
            if (!string.IsNullOrWhiteSpace(identityEmail))
            {
                return identityEmail;
            }

            // No matches found, so we indicate that email is not available.
            return null;
        }
    }
}
