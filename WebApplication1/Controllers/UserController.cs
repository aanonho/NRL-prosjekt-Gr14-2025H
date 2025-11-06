using Microsoft.AspNetCore.Mvc;
using WebApplication1.Models;
using WebApplication1.Models.Entities;
using System.Collections.Generic;
using System.Linq;

namespace WebApplication1.Controllers
{
    public class UserController : Controller
    {
        // Mock user list
        private static List<UserData> _users = new List<UserData>();

        // Currently "logged in" user (temp)
        private static UserData? _currentUser = null;

        [HttpGet]
        public IActionResult UserForm()
        {
            return View();
        }

        [HttpPost]
        public IActionResult UserForm(UserData userData)
        {
            if (!ModelState.IsValid)
            {
                return View(userData);
            }

            // Add user if not already in list
            var existingUser = _users.FirstOrDefault(u => u.Email == userData.Email);
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
                if (string.IsNullOrWhiteSpace(userData.Organization))
                {
                    userData.Organization = "Unknown";
                }

                _users.Add(userData);
            }


            // Set as current user
            _currentUser = userData;

            // Redirect based on role
            if (userData.Role != null && userData.Role.ToLower() == "registrar")
            {
                return RedirectToAction("RegistrarDashboard");
            }
            else
            {
                return RedirectToAction("UserProfile", new { email = userData.Email });
            }
        }


        // == SUPPORTING METHODS FOR MOCK AUTHENTICATION ==
        public static List<UserData> GetRegisteredUsers() => _users;

        // Returns the currently "logged in" user
        public static UserData? GetCurrentUser() => _currentUser;

        public IActionResult Index()
        {
            if (_currentUser != null)
                return RedirectToAction("UserProfile", _currentUser);

            return RedirectToAction("UserForm");
        }

        // === USER PROFILE VIEW ===
        [HttpGet]
        public IActionResult UserProfile(string email)
        {
            var user = _users.FirstOrDefault(u => u.Email == email);
            if (user == null)
                return RedirectToAction("UserForm");

            var reports = ReportStore.GetReportsByUser(email);

            // Sends user and reports to the view
            ViewBag.User = user;
            return View(reports);
        }

        // === REGISTRAR VIEW ===
        [HttpGet]
        public IActionResult RegistrarDashboard(string status = "all", string sort = "date_desc")
        {
            // Get only submitted reports (exclude drafts)
            var reports = ReportStore.GetAll() ?? new List<ReportItem>();
            reports = reports.Where(r => !r.IsDraft).ToList();


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
        public IActionResult UpdateReportStatus(int id, string status, string? message)
        {
            // Only allow registrar to act on submitted (non-draft) reports
            var report = ReportStore.GetAll()
                .FirstOrDefault(r => r.ReportID == id && !r.IsDraft);

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
            ReportStore.Update(report.ReportID, report);

            // Show confirmation
            TempData["SuccessMessage"] = $"Report {status.ToLower()} successfully.";
            return RedirectToAction("RegistrarDashboard");
        }




    }
}
