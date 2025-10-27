using Microsoft.AspNetCore.Mvc;
using WebApplication1.Models;
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
            if (!_users.Any(u => u.Email == userData.Email))
            {
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
            var reports = ReportStore.GetAll() ?? new List<ReportItem>();

            if
                (!string.IsNullOrEmpty(status) && status.ToLower() != "all")
            {
                reports = reports
                    .Where(r => string.Equals(r.Status, status, StringComparison.OrdinalIgnoreCase))
                    .ToList();
            }

            // Date sorting
            reports = sort == "date_asc"
                ? reports.OrderBy(r => r.CreatedAt).ToList()
                : reports.OrderByDescending(r => r.CreatedAt).ToList();

            return View("RegistrarDashboard", reports);
        }

        [HttpPost]
        public IActionResult UpdateReportStatus(Guid id, string status, string message)
        {
            ReportStore.UpdateStatus(id, status, message);
            return RedirectToAction("RegistrarDashboard");
        }

    }
}
