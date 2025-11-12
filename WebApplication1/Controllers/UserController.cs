using Microsoft.AspNetCore.Mvc;
using WebApplication1.Models;
using WebApplication1.Models.Entities;
using System.Collections.Generic;
using System.Linq;
using WebApplication1.DataInfrastructure;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace WebApplication1.Controllers
{
    public class UserController : Controller
    {
        private readonly ApplicationDbContext _context;

        public UserController(ApplicationDbContext context)
        {
            _context = context;
        }   

        [HttpGet]
        public IActionResult UserForm()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> UserForm(UserEntity userData)
        {
            if (!ModelState.IsValid)
            {
                return View(userData);
            }

            // Add user if not already in list
            var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == userData.Email);
            if (existingUser != null)
            {
                // If user exists, check role consistency
                if (!string.Equals(existingUser.Role, userData.Role, StringComparison.OrdinalIgnoreCase))
                {
                    ModelState.AddModelError("Role", "This email is already registered with another role.");
                    return View(userData);
                }
            }
            else
            {
                if (string.IsNullOrWhiteSpace(userData.Organization?.ToString()))
                {
                    userData.Organization = new Organization { Name = "Unknown" }; // Alternative; set OrgID = null
                }

                _context.Users.Add(userData);
                await _context.SaveChangesAsync();
            }


            // Set as current user
            TempData["CurrentUserEmail"] = userData.Email;

            // Redirect based on role
            return userData.Role?.ToLower() == "registrar"
        ? RedirectToAction("RegistrarDashboard")
        : RedirectToAction("UserProfile", new { email = userData.Email });
        }


        public async Task<IActionResult> Index()
        {
            var email = TempData["CurrentUserEmail"] as string;
            if (!string.IsNullOrEmpty(email))
            {
                // Use Task.FromResult to provide an awaitable task
                return await Task.FromResult(RedirectToAction("UserProfile", new { email }));
            }

            return await Task.FromResult(RedirectToAction("UserForm"));
        }

        // === USER PROFILE VIEW ===
        [HttpGet]
        public async Task<IActionResult> UserProfile(string email)
        {
            var user = await _context.Users
                .Include(u => u.Organization)
                .FirstOrDefaultAsync(u => u.Email == email);

            if (user == null)
                return RedirectToAction("UserForm");

            var reports = await _context.ReportItems
                .Where(r => r.SubmittedByEmail == email)
                .Include(r => r.ReportObstacle)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();

            // Sends user and reports to the view
            ViewBag.User = user;
            return View(reports);
        }

        // === REGISTRAR VIEW ===
        [HttpGet]
        public async Task<IActionResult> RegistrarDashboard(string status = "all", string sort = "date_desc")
        {
            // Get only submitted reports (exclude drafts)
            var reports = await _context.ReportItems
                .Where(r => !r.IsDraft)
                .Include(r => r.ReportObstacle)
                .ToListAsync();


            // Filter by status if a valid one is selected
            if (!string.IsNullOrEmpty(status) && status.ToLower() != "all")
            {
                reports = reports
                    .Where(r => string.Equals(r.Status, status, StringComparison.OrdinalIgnoreCase))
                    .ToList();
            }

            // Sort by date
            reports = sort == "date_asc"
                ? reports.OrderBy(r => r.CreatedAt).ToList()
                : reports.OrderByDescending(r => r.CreatedAt).ToList();

            return View("RegistrarDashboard", reports);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateReportStatus(int id, string status, string? message)
        {
            // Only allow registrar to act on submitted (non-draft) reports
            var report = await _context.ReportItems
                .FirstOrDefaultAsync(r => r.ReportID == id && !r.IsDraft);

            if (report == null)
            {
                TempData["ErrorMessage"] = "Only submitted reports can be reviewed by registrars.";
                return RedirectToAction("RegistrarDashboard");
            }

            // Update registrar-specific fields
            report.Status = status;
            report.IsDraft = false;
            report.ReviewedAt = DateTime.Now;
            report.ReviewMessage = message ?? "";

            // Persist update safely
            _context.ReportItems.Update(report);
            await _context.SaveChangesAsync();

            // Show confirmation
            TempData["SuccessMessage"] = $"Report {status.ToLower()} successfully.";
            return RedirectToAction("RegistrarDashboard");
        }




    }
}
