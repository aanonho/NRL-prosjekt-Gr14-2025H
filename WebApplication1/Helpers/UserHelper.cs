using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;

namespace WebApplication1.Helpers
{
    public class UserHelper
    {
        public static string? GetCurrentUserEmail(Controller controller)
        {
            // Try TempData first
            if (controller.TempData.TryGetValue("CurrentUserEmail", out var emailObj) && emailObj is string email && !string.IsNullOrWhiteSpace(email))
            {
                return email;
            }

            // Alternative: Use HttpContext.User when real login is implemented
            var identityEmail = controller.User?.Identity?.Name;
            if (!string.IsNullOrWhiteSpace(identityEmail))
            {
                return identityEmail;
            }

            return null;
        }
    }
}
