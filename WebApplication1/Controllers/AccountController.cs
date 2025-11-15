using System.Security.Claims;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApplication1.DataInfrastructure;
using WebApplication1.Models;
using WebApplication1.Models.Entities;

namespace WebApplication1.Controllers
{
    public class AccountController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly PasswordHasher<UserEntity> _passwordHasher = new();

        public AccountController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            if (User?.Identity?.IsAuthenticated ?? false)
            {
                return RedirectToLocal(returnUrl);
            }

            var model = new LoginViewModel
            {
                ReturnUrl = returnUrl
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var normalizedEmail = model.Email!.Trim();
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == normalizedEmail);

            if (user == null)
            {
                ModelState.AddModelError(string.Empty, "Invalid email or password.");
                return View(model);
            }

            var verification = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash ?? string.Empty, model.Password!);
            if (verification == PasswordVerificationResult.Failed)
            {
                ModelState.AddModelError(string.Empty, "Invalid email or password.");
                return View(model);
            }

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.UserID.ToString()),
                new Claim(ClaimTypes.Name, user.Email ?? normalizedEmail),
            };

            if (!string.IsNullOrWhiteSpace(user.Role))
            {
                claims.Add(new Claim(ClaimTypes.Role, user.Role));
            }

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

            var authProperties = new AuthenticationProperties
            {
                IsPersistent = model.RememberMe
            };

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity),
                authProperties);

            TempData["CurrentUserEmail"] = normalizedEmail;

            return RedirectToLocal(model.ReturnUrl);
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            TempData.Remove("CurrentUserEmail");
            return RedirectToAction("Index", "Home");
        }

        private IActionResult RedirectToLocal(string? returnUrl)
        {
            if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }

            var email = TempData["CurrentUserEmail"] as string;
            if (!string.IsNullOrWhiteSpace(email))
            {
                return RedirectToAction("UserProfile", "User", new { email });
            }

            var identityEmail = User?.Identity?.Name;
            if (!string.IsNullOrWhiteSpace(identityEmail))
            {
                return RedirectToAction("UserProfile", "User", new { email = identityEmail });
            }

            return RedirectToAction("Index", "Home");
        }
    }
}
