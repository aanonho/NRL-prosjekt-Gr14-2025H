using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using WebApplication1.Services;

namespace WebApplication1.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly IEmailSender _emailSender; 

        public AccountController(
            UserManager<IdentityUser> userManager,
            SignInManager<IdentityUser> signInManager,
            IEmailSender emailSender) 
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _emailSender = emailSender;
        }

        // ---------- VMs ----------
        public class RegisterVm { public string Email { get; set; } = ""; public string Password { get; set; } = ""; public string ConfirmPassword { get; set; } = ""; }
        public class LoginVm { public string Email { get; set; } = ""; public string Password { get; set; } = ""; public bool RememberMe { get; set; } public string? ReturnUrl { get; set; } }
        public class ForgotPasswordVm { public string Email { get; set; } = ""; }
        public class ResetPasswordVm { public string Email { get; set; } = ""; public string Password { get; set; } = ""; public string ConfirmPassword { get; set; } = ""; public string Token { get; set; } = ""; }

        // ---------- Register ----------
        [HttpGet] public IActionResult Register() => View(new RegisterVm());

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterVm model)
        {
            if (!ModelState.IsValid) return View(model);
            if (model.Password != model.ConfirmPassword) { ModelState.AddModelError("", "Passwords do not match."); return View(model); }

            var email = (model.Email ?? "").Trim();
            if (string.IsNullOrWhiteSpace(email)) { ModelState.AddModelError("", "Email is required."); return View(model); }

            var existing = await _userManager.FindByEmailAsync(email);
            if (existing != null) { ModelState.AddModelError("", "This account already exists. Please sign in."); return View(model); }

            var user = new IdentityUser { UserName = email, Email = email };
            var create = await _userManager.CreateAsync(user, model.Password);
            if (!create.Succeeded) { foreach (var e in create.Errors) ModelState.AddModelError("", e.Description); return View(model); }

            // Email confirmation (previewed on Home)
            var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            var link = Url.Action("ConfirmEmail", "Account", new { userId = user.Id, token }, Request.Scheme)!;

            var html =
                $"<p style='margin:0 0 8px'>Click to confirm: " +
                $"<a href=\"{link}\" style='color:#1d4ed8;text-decoration:underline'>Open confirmation link</a></p>" +
                $"<p style='margin:0;color:#6b7280;font-size:12px'>Direct URL (dev only): " +
                $"{System.Net.WebUtility.HtmlEncode(link)}</p>";

            await _emailSender.SendEmailAsync(email, "Confirm your account", html);
            
            return RedirectToAction("Login");
        }

        [HttpGet]
        public async Task<IActionResult> ConfirmEmail(string userId, string token)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return NotFound();

            var result = await _userManager.ConfirmEmailAsync(user, token);
            if (!result.Succeeded) return BadRequest("Invalid confirmation.");

            TempData["Msg"] = "Email confirmed. You can log in now.";
            return RedirectToAction("Login");
        }

        // ---------- Login / Logout ----------
        [HttpGet] public IActionResult Login(string? returnUrl = null) => View(new LoginVm { ReturnUrl = returnUrl });

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginVm model)
        {
            if (!ModelState.IsValid) return View(model);

            var result = await _signInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, lockoutOnFailure: true);
            if (result.Succeeded)
                return string.IsNullOrEmpty(model.ReturnUrl) ? RedirectToAction("Index", "Home") : Redirect(model.ReturnUrl);

            if (result.IsNotAllowed)
                ModelState.AddModelError("", "Please confirm your email first (see DEV Email Preview on Home).");
            else
                ModelState.AddModelError("", "Invalid login attempt.");

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }

        // ---------- Forgot / Reset ----------
        [HttpGet] public IActionResult ForgotPassword() => View(new ForgotPasswordVm());

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordVm model)
        {
            if (!ModelState.IsValid) return View(model);
            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user is null || !(await _userManager.IsEmailConfirmedAsync(user)))
                return RedirectToAction("ForgotPasswordConfirmation"); // don’t reveal existence

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var link = Url.Action("ResetPassword", "Account", new { token, email = model.Email }, Request.Scheme)!;

            var html =
                $"<p style='margin:0 0 8px'>Reset your password: " +
                $"<a href=\"{link}\" style='color:#1d4ed8;text-decoration:underline'>Open reset link</a></p>" +
                $"<p style='margin:0;color:#6b7280;font-size:12px'>Direct URL (dev only): " +
                $"{System.Net.WebUtility.HtmlEncode(link)}</p>";

            await _emailSender.SendEmailAsync(model.Email, "Reset your password", html);
            return RedirectToAction("ForgotPasswordConfirmation");
        }

        [HttpGet] public IActionResult ForgotPasswordConfirmation() => View();

        [HttpGet] public IActionResult ResetPassword(string token, string email) => View(new ResetPasswordVm { Token = token, Email = email });

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(ResetPasswordVm model)
        {
            if (!ModelState.IsValid) return View(model);
            if (model.Password != model.ConfirmPassword) { ModelState.AddModelError("", "Passwords do not match."); return View(model); }

            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user is null) return RedirectToAction("ResetPasswordConfirmation");

            var result = await _userManager.ResetPasswordAsync(user, model.Token, model.Password);
            if (result.Succeeded) return RedirectToAction("ResetPasswordConfirmation");

            foreach (var e in result.Errors) ModelState.AddModelError("", e.Description);
            return View(model);
        }

        [HttpGet] public IActionResult ResetPasswordConfirmation() => View();
    }
}
