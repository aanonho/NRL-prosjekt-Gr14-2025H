// Denne filen håndterer registrering, profilvisning og rollestyring for brukere.
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using WebApplication1.Models;
using WebApplication1.Models.Entities;
using System.Collections.Generic;
using System.Linq;
using WebApplication1.DataInfrastructure;
using System.Threading.Tasks;
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace WebApplication1.Controllers
{
    public class UserController : Controller
    {
        private readonly ApplicationDbContext _context;

        public UserController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Viser påmeldingsskjemaet der pilot eller registrar registrerer seg.
        [HttpGet]
        public IActionResult UserForm()
        {
            PopulateOrganizationOptions();
            return View(new UserData());
        }

        // Tar imot registreringsskjemaet, oppretter bruker og setter riktig rolle.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UserForm(UserData userData)
        {
            PopulateOrganizationOptions();

            var normalizedOrganization = OrganizationOptions.NormalizeName(userData.Organization);
            if (normalizedOrganization == null)
            {
                ModelState.AddModelError(nameof(userData.Organization), "Please select a valid organization.");
            }
            else
            {
                userData.Organization = normalizedOrganization;
            }

            if (!ModelState.IsValid)
            {
                return View(userData);
            }

            var normalizedEmail = userData.Email!.Trim();
            var normalizedRole = userData.Role!.Trim();
            var isRegistrar = IsRegistrarRole(normalizedRole);
            var isPilot = IsPilotRole(normalizedRole);

            var organization = await ResolveOrganizationAsync(normalizedOrganization!);

            var existingUser = await _context.Users
                .Include(u => u.Organization)
                .Include(u => u.Pilot)
                .Include(u => u.Registrar)
                .FirstOrDefaultAsync(u => u.Email == normalizedEmail);

            var passwordHasher = new PasswordHasher<UserEntity>();

            if (existingUser != null)
            {
                if (!string.Equals(existingUser.Role, normalizedRole, StringComparison.OrdinalIgnoreCase))
                {
                    ModelState.AddModelError("Role", "This email is already registered with another role.");
                    return View(userData);
                }

                existingUser.Name = userData.Name?.Trim();
                existingUser.Email = normalizedEmail;
                existingUser.Phone = userData.Phone?.Trim();
                existingUser.Role = normalizedRole;
                existingUser.PasswordHash = passwordHasher.HashPassword(existingUser, userData.Password!);

                existingUser.Organization = organization;
                existingUser.OrganizationID = organization.OrganizationID;

                if (isPilot && existingUser.Pilot == null)
                {
                    existingUser.Pilot = new Pilot { User = existingUser };
                }

                if (isRegistrar && existingUser.Registrar == null)
                {
                    existingUser.Registrar = new Registrar { User = existingUser };
                }
            }
            else
            {
                var newUser = new UserEntity
                {
                    Name = userData.Name?.Trim(),
                    Email = normalizedEmail,
                    Phone = userData.Phone?.Trim(),
                    Role = normalizedRole,
                    Organization = organization,
                    OrganizationID = organization.OrganizationID
                };

                newUser.PasswordHash = passwordHasher.HashPassword(newUser, userData.Password!);

                if (isPilot)
                {
                    newUser.Pilot = new Pilot { User = newUser };
                }

                if (isRegistrar)
                {
                    newUser.Registrar = new Registrar { User = newUser };
                }

                _context.Users.Add(newUser);
            }

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Account created successfully. Please sign in.";

            return RedirectToAction("Login", "Account");
        }


        public async Task<IActionResult> Index()
        {
            var email = TempData["CurrentUserEmail"] as string;
            if (!string.IsNullOrEmpty(email))
            {
                return await Task.FromResult(RedirectToAction("UserProfile", new { email }));
            }

            return await Task.FromResult(RedirectToAction("UserForm"));
        }

        // Viser profilsiden for innlogget eller valgt bruker.
        [Authorize]
        [HttpGet]
        public async Task<IActionResult> UserProfile(string? email)
        {
            var resolvedEmail = ResolveEmail(email);
            if (string.IsNullOrWhiteSpace(resolvedEmail))
            {
                return RedirectToAction("Login", "Account", new { returnUrl = Url.Action("UserProfile", "User") });
            }

            var normalizedEmail = resolvedEmail.Trim();
            var normalizedEmailLower = normalizedEmail.ToLowerInvariant();

            var user = await _context.Users
                .Include(u => u.Organization)
                .FirstOrDefaultAsync(u => u.Email != null && u.Email.ToLower() == normalizedEmailLower);

            if (user == null)
            {
                TempData["ErrorMessage"] = "User profile could not be found.";
                return RedirectToAction("UserForm");
            }

            var viewModel = UserProfileViewModel.FromEntity(user);

            return View(viewModel);
        }

        // Registrar-dashboard som lister innsendte rapporter for oppfølging.
        [HttpGet]
        public async Task<IActionResult> RegistrarDashboard(string status = "all", string sort = "date_desc")
        {
            var reports = await _context.ReportItems
                .Where(r => !r.IsDraft)
                .Include(r => r.ReportObstacle)
                .Include(r => r.OrganizationRef)
                .ToListAsync();

            if (!string.IsNullOrEmpty(status) && status.ToLower() != "all")
            {
                reports = reports
                    .Where(r => string.Equals(r.Status, status, StringComparison.OrdinalIgnoreCase))
                    .ToList();
            }

            reports = sort == "date_asc"
                ? reports.OrderBy(r => r.CreatedAt).ToList()
                : reports.OrderByDescending(r => r.CreatedAt).ToList();

            return View("RegistrarDashboard", reports);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateReportStatus(int id, string status, string? message)
        {
            // Registrar låser endringer til innsendte rapporter og lagrer vurderingen.
            var report = await _context.ReportItems
                .FirstOrDefaultAsync(r => r.ReportID == id && !r.IsDraft);

            if (report == null)
            {
                TempData["ErrorMessage"] = "Only submitted reports can be reviewed by registrars.";
                return RedirectToAction("RegistrarDashboard");
            }

            report.Status = status;
            report.IsDraft = false;
            report.ReviewedAt = DateTime.Now;
            report.ReviewMessage = message ?? "";

            _context.ReportItems.Update(report);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Report {status.ToLower()} successfully.";
            return RedirectToAction("RegistrarDashboard");
        }

        private async Task<Organization> ResolveOrganizationAsync(string organizationName)
        {
            var existingOrganization = await _context.Organizations
                .FirstOrDefaultAsync(o => o.Name.ToLower() == organizationName.ToLower());

            if (existingOrganization != null)
            {
                return existingOrganization;
            }

            var newOrganization = new Organization
            {
                Name = organizationName
            };

            _context.Organizations.Add(newOrganization);
            return newOrganization;
        }

        private void PopulateOrganizationOptions()
        {
            ViewBag.OrganizationOptions = OrganizationOptions.AllowedOrganizations
                .Select(o => new SelectListItem
                {
                    Text = o,
                    Value = o
                })
                .ToList();
        }

        private static bool IsRegistrarRole(string role) =>
            string.Equals(role, "registrar", StringComparison.OrdinalIgnoreCase);

        private static bool IsPilotRole(string role) =>
            string.Equals(role, "pilot", StringComparison.OrdinalIgnoreCase);

        private string? ResolveEmail(string? email)
        {
            var identityEmail = User?.Identity?.Name;
            if (!string.IsNullOrWhiteSpace(identityEmail))
            {
                return identityEmail;
            }

            if (!string.IsNullOrWhiteSpace(email))
            {
                return email;
            }

            if (TempData.ContainsKey("CurrentUserEmail"))
            {
                var tempEmail = TempData.Peek("CurrentUserEmail") as string;
                if (!string.IsNullOrWhiteSpace(tempEmail))
                {
                    return tempEmail;
                }
            }

            return null;
        }

    }
}

