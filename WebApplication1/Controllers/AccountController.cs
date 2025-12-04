// Denne filen styrer innlogging, utlogging og passordreset for brukere.
using System.Security.Claims;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApplication1.DataInfrastructure;
using WebApplication1.Models;
using WebApplication1.Models.Entities;
using WebApplication1.Services;
using System.Security.Cryptography;
using Microsoft.AspNetCore.WebUtilities;
using System.Text;

namespace WebApplication1.Controllers
{
    public class AccountController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IEmailSender _emailSender;
        private readonly PasswordHasher<UserEntity> _passwordHasher = new();
        private readonly ILogger<AccountController> _logger;


        public AccountController(ApplicationDbContext context, IEmailSender emailSender, ILogger<AccountController> logger)
        {
            _context = context;
            _emailSender = emailSender;
            _logger = logger;
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

        [HttpGet]
        public IActionResult ForgotPassword()
        {
            return View(new ForgotPasswordViewModel());
        }

        // Håndterer forespørsel om glemt passord og lager ny token om nødvendig.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var normalizedEmail = model.Email!.Trim();
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == normalizedEmail);
            const string confirmationMessage = "If an account exists for the provided email, a password reset link has been sent.";

            if (user == null)
            {
                TempData["SuccessMessage"] = confirmationMessage;
                return RedirectToAction(nameof(ForgotPassword));
            }

            await InvalidateExistingTokens(user.UserID);
            await _context.SaveChangesAsync();

            var rawToken = GenerateSecureToken();
            var tokenHash = HashToken(rawToken);

            var resetToken = new PasswordResetToken
            {
                UserId = user.UserID,
                TokenHash = tokenHash,
                CreatedAtUtc = DateTime.UtcNow,
                ExpiresAtUtc = DateTime.UtcNow.AddHours(1)
            };

            _context.PasswordResetTokens.Add(resetToken);

            var resetLink = Url.Action(nameof(ResetPassword), "Account", new { token = rawToken, email = user.Email }, Request.Scheme)!;

            try
            {
                await _emailSender.SendPasswordResetAsync(user.Email!, user.Name ?? user.Email!, resetLink);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send reset email: {Message}", ex.Message);
                _context.PasswordResetTokens.Remove(resetToken);
                await _context.SaveChangesAsync();

                ModelState.AddModelError(string.Empty, "We were unable to send the reset email. Please try again later.");
                return View(model);
            }

            TempData["SuccessMessage"] = confirmationMessage;
            return RedirectToAction(nameof(ForgotPassword));
        }

        // Validerer innloggingsskjema og oppretter cookie ved suksess.
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

            if (user.Role == "Pilot")
            {
                return RedirectToAction("Dataform", "Obstacle");
            }

            if (user.Role == "Registrar")
            {

                return RedirectToAction("Index", "Reports");

            }

            return RedirectToLocal(model.ReturnUrl);
        }

        // Viser skjemaet for å legge inn nytt passord basert på token fra e-post.
        [HttpGet]
        public async Task<IActionResult> ResetPassword(string? token, string? email)
        {
            if (string.IsNullOrWhiteSpace(token) || string.IsNullOrWhiteSpace(email))
            {
                TempData["ErrorMessage"] = "Invalid password reset link.";
                return RedirectToAction(nameof(Login));
            }

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email.Trim());
            if (user == null)
            {
                TempData["ErrorMessage"] = "Invalid password reset link.";
                return RedirectToAction(nameof(Login));
            }

            var tokenHash = HashToken(token);
            var tokenEntity = await _context.PasswordResetTokens
                .Where(t => t.UserId == user.UserID && t.TokenHash == tokenHash)
                .FirstOrDefaultAsync();

            if (tokenEntity == null || tokenEntity.RedeemedAtUtc != null || tokenEntity.ExpiresAtUtc < DateTime.UtcNow)
            {
                TempData["ErrorMessage"] = "This password reset link is no longer valid.";
                return RedirectToAction(nameof(Login));
            }

            var viewModel = new ResetPasswordViewModel
            {
                Email = email.Trim(),
                Token = token
            };

            return View(viewModel);
        }

        // Tar imot nytt passord og markerer token som brukt.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var normalizedEmail = model.Email!.Trim();
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == normalizedEmail);
            if (user == null)
            {
                TempData["ErrorMessage"] = "Unable to reset password with the provided information.";
                return RedirectToAction(nameof(Login));
            }

            var tokenHash = HashToken(model.Token!);
            var tokenEntity = await _context.PasswordResetTokens
                .Where(t => t.UserId == user.UserID && t.TokenHash == tokenHash)
                .FirstOrDefaultAsync();

            if (tokenEntity == null || tokenEntity.RedeemedAtUtc != null || tokenEntity.ExpiresAtUtc < DateTime.UtcNow)
            {
                TempData["ErrorMessage"] = "This password reset link is no longer valid.";
                return RedirectToAction(nameof(Login));
            }

            user.PasswordHash = _passwordHasher.HashPassword(user, model.Password!);
            tokenEntity.RedeemedAtUtc = DateTime.UtcNow;

            var otherTokens = _context.PasswordResetTokens.Where(t => t.UserId == user.UserID && t.RedeemedAtUtc == null && t.Id != tokenEntity.Id);
            _context.PasswordResetTokens.RemoveRange(otherTokens);

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Your password has been reset. Please sign in with your new password.";
            return RedirectToAction(nameof(Login));
        }

        // Logger brukeren ut og rydder midlertidige data.
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

        // Lager en engangstoken for e-postlenker og hasher den før lagring.
        private static string GenerateSecureToken()
        {
            var bytes = RandomNumberGenerator.GetBytes(48);
            return WebEncoders.Base64UrlEncode(bytes);
        }

        // Hash the token before storing it in DB
        private static string HashToken(string token)
        {
            using var sha = SHA256.Create();
            var hash = sha.ComputeHash(Encoding.UTF8.GetBytes(token));
            return WebEncoders.Base64UrlEncode(hash);
        }

        private async Task InvalidateExistingTokens(int userId)
        {
            var existingTokens = await _context.PasswordResetTokens
                .Where(t => t.UserId == userId)
                .ToListAsync();

            if (existingTokens.Any())
            {
                _context.PasswordResetTokens.RemoveRange(existingTokens);
            }
        }
    }
}
