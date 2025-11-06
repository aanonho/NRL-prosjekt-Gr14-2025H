using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApplication1.DataInfrastructure;
using WebApplication1.Models;
using WebApplication1.Models.Entities;

namespace WebApplication1.Controllers
{
    public class ObstacleController : Controller
    {
        private readonly ApplicationDbContext _context; // Database context

        public ObstacleController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet("read")]
        public IActionResult ReadAll()
        {
            var data = _context.Obstacles.ToList();
            return Ok(data);
        }

        // === SHOW FORM ===
        [HttpGet]
        public ActionResult DataForm()
        {
            return View();
        }

        // === EDIT EXISTING REPORT (VIEW) ===
        [HttpGet]
        public IActionResult Edit(int id)
        {
            var report = ReportStore.GetAll().FirstOrDefault(r => r.ReportID == id);
            if (report == null)
            {
                return RedirectToAction("UserProfile", "User", new { email = UserController.GetCurrentUser()?.Email });
            }

            var obstacleData = report.Obstacle;
            if (obstacleData == null)
            {
                return RedirectToAction("UserProfile", "User");
            }

            ViewBag.IsEditing = true;
            return View("Details", report);
        }

        // === HANDLE SUBMIT / SAVE DRAFT ===
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DataForm(ValidatedObstacleData validatedData, IFormFile? imageFile, string submitType)
        {
            // 1) Image upload
            if (imageFile != null && imageFile.Length > 0)
            {
                var directory = Path.Combine("wwwroot", "images");
                if (!Directory.Exists(directory))
                    Directory.CreateDirectory(directory);

                var fileName = Path.GetFileName(imageFile.FileName);
                var imagePath = Path.Combine(directory, fileName);

                using (var stream = new FileStream(imagePath, FileMode.Create))
                    await imageFile.CopyToAsync(stream);

                validatedData.ImagePath = "/images/" + fileName;
            }

            // 2) Current user (required)
            var currentUser = UserController.GetCurrentUser();
            if (currentUser == null)
                return RedirectToAction("UserForm", "User");

            validatedData.ObstacleRegistrationTime = DateTime.Now;

            // 3) Resolve or create Organization
            int? organizationId = null;
            if (!string.IsNullOrWhiteSpace(currentUser.Organization))
            {
                var orgName = currentUser.Organization.Trim();
                if (orgName.Length > 45) orgName = orgName.Substring(0, 45);

                var org = await _context.Organizations.FirstOrDefaultAsync(o => o.Name == orgName);
                if (org == null)
                {
                    org = new Organization { Name = orgName };
                    _context.Organizations.Add(org);
                    await _context.SaveChangesAsync();
                }
                organizationId = org.OrganizationID;
            }

            // 4) Resolve or create UserEntity (by email) and ensure Pilot exists
            var emailKey = (currentUser.Email ?? string.Empty).Trim();
            if (emailKey.Length > 45) emailKey = emailKey.Substring(0, 45);

            var userEntity = await _context.Users.FirstOrDefaultAsync(u => u.Email == emailKey);
            if (userEntity == null)
            {
                userEntity = new UserEntity
                {
                    Name = currentUser.Name,
                    Email = emailKey,
                    Phone = currentUser.Phone,
                    Role = string.IsNullOrWhiteSpace(currentUser.Role) ? "Pilot" : currentUser.Role,
                    OrganizationID = organizationId
                };
                _context.Users.Add(userEntity);
                await _context.SaveChangesAsync();
            }
            else
            {
                if (userEntity.OrganizationID != organizationId)
                {
                    userEntity.OrganizationID = organizationId;
                    _context.Users.Update(userEntity);
                    await _context.SaveChangesAsync();
                }
            }

            var pilot = await _context.Pilots.FindAsync(userEntity.UserID);
            if (pilot == null)
            {
                _context.Pilots.Add(new Pilot { UserID = userEntity.UserID, ULicenseNumber = "N/A", AircraftType = "N/A" });
                await _context.SaveChangesAsync();
            }

            // 5) Create & save ReportItem (no magic IDs)
            var report = new ReportItem
            {
                CreatedAt = validatedData.ObstacleRegistrationTime,
                Status = (submitType == "Submit") ? "Pending" : "Draft",
                OrganizationID = organizationId,                // nullable FK
                PilotID = userEntity.UserID,                    // required FK
                IsDraft = submitType != "Submit",
                CreatedBy = currentUser.Email,
                SubmittedByEmail = currentUser.Email,
                SubmittedByName = currentUser.Name,
                Organization = currentUser.Organization ?? "Unknown"
            };

            _context.ReportStore.Add(report);
            await _context.SaveChangesAsync();

            // 6) Save the obstacle and link it
            var obstacle = new ObstacleData
            {
                ObstacleName = validatedData.ObstacleName,
                ObstacleHeight = validatedData.ObstacleHeight,
                ObstacleDescription = validatedData.ObstacleDescription,
                ObstacleLatitude = validatedData.ObstacleLatitude,
                ObstacleLongitude = validatedData.ObstacleLongitude,
                ObstacleType = validatedData.ObstacleType,
                ObstacleRadius = validatedData.ObstacleRadius,
                ObstacleGeoJson = validatedData.ObstacleGeoJson,
                ObstacleLineCoordinates = validatedData.ObstacleLineCoordinates,
                ObstacleLineLength = validatedData.ObstacleLineLength,
                ImagePath = validatedData.ImagePath,
                ObstacleRegistrationTime = validatedData.ObstacleRegistrationTime,
                IsDraft = report.IsDraft,
                ReportID = report.ReportID // FK
            };

            _context.Obstacles.Add(obstacle);
            await _context.SaveChangesAsync();

            // 7) Update Report with its obstacle record
            report.ReportObstacle = obstacle;
            _context.ReportStore.Update(report);
            await _context.SaveChangesAsync();

            // 8) Redirect
            return RedirectToAction("UserProfile", "User", new { email = currentUser.Email ?? string.Empty });
        }

        // === DETAILS VIEW ===
        [HttpGet]
        public IActionResult Details(int id, string? returnUrl = null)
        {
            var report = ReportStore.GetAll().FirstOrDefault(r => r.ReportID == id);
            if (report == null)
            {
                return NotFound();
            }

            var loggedUser = UserController.GetCurrentUser();

            if (string.IsNullOrEmpty(returnUrl))
            {
                returnUrl = loggedUser?.Role == "Registrar"
                    ? Url.Action("RegistrarDashboard", "User")
                    : Url.Action("UserProfile", "User", new { email = loggedUser?.Email });
            }

            ViewBag.ReturnUrl = returnUrl;
            ViewBag.CurrentUser = loggedUser;

            return View(report);
        }

        // === SUBMIT DRAFT (FROM DETAILS) ===
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult SubmitDraft(int id, string actionType, ValidatedObstacleData updatedData)
        {
            var report = ReportStore.GetAll().FirstOrDefault(r => r.ReportID == id);
            if (report == null)
            {
                var fallbackEmail = UserController.GetCurrentUser()?.Email ?? string.Empty;
                return RedirectToAction("UserProfile", "User", new { email = fallbackEmail });
            }

            if (report.Obstacle == null)
                report.Obstacle = new ValidatedObstacleData();

            // Update some draft fields
            report.Obstacle.ObstacleName = updatedData.ObstacleName;
            report.Obstacle.ObstacleHeight = updatedData.ObstacleHeight;
            report.Obstacle.ObstacleDescription = updatedData.ObstacleDescription;

            if (string.Equals(actionType, "Submit", StringComparison.OrdinalIgnoreCase))
            {
                report.IsDraft = false;
                report.Status = "Pending";
            }

            ReportStore.Update(report.ReportID, report);
            return RedirectToAction("Details", new { id = report.ReportID });
        }

        // === EDITING AN EXISTING DRAFT (FORM POST) ===
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult UpdateDraft(int id, string ObstacleName, double ObstacleHeight, double? ObstacleLatitude, double? ObstacleLongitude, string ObstacleDescription, string? submitType)
        {
            var currentUser = UserController.GetCurrentUser();
            if (currentUser == null || string.IsNullOrWhiteSpace(currentUser.Email))
                return RedirectToAction("UserForm", "User");

            var report = ReportStore.GetReportsByUser(currentUser.Email!)
                                    .FirstOrDefault(r => r.ReportID == id);

            if (report == null)
            {
                TempData["ErrorMessage"] = "Draft not found.";
                return RedirectToAction("UserProfile", "User", new { email = currentUser.Email! });
            }

            // Update draft values
            if (report.Obstacle != null)
            {
                report.Obstacle.ObstacleName = ObstacleName;
                report.Obstacle.ObstacleHeight = ObstacleHeight;
                // You can also update coordinates/description if desired:
                // report.Obstacle.ObstacleLatitude = ObstacleLatitude;
                // report.Obstacle.ObstacleLongitude = ObstacleLongitude;
                // report.Obstacle.ObstacleDescription = ObstacleDescription;
            }

            if (string.Equals(submitType, "Submit", StringComparison.OrdinalIgnoreCase))
            {
                report.Status = "Pending";
                report.IsDraft = false;
                TempData["SuccessMessage"] = "Draft submitted successfully.";
            }
            else
            {
                report.Status = "Draft";
                TempData["SuccessMessage"] = "Draft updated.";
            }

            return RedirectToAction("Details", "Obstacle", new { id = report.ReportID });
        }

        // ------------ HELPERS (CLASS SCOPE) ------------
        // These ensure the related rows exist when you need safe defaults.
        // Keep them until the whole app always registers users/orgs before submit.

        private async Task<int?> GetOrCreateDefaultOrganizationIdAsync()
        {
            const string defaultOrgName = "Default Organization";
            var org = await _context.Organizations.FirstOrDefaultAsync(o => o.Name == defaultOrgName);

            if (org == null)
            {
                org = new Organization { Name = defaultOrgName };
                _context.Organizations.Add(org);
                await _context.SaveChangesAsync();
            }
            return org.OrganizationID;
        }

        private async Task<int> GetOrCreateDefaultPilotIdAsync()
        {
            const string email = "default.pilot@nrl.local";

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
            if (user == null)
            {
                user = new UserEntity
                {
                    Name = "Default Pilot",
                    Email = email,
                    Role = "Pilot",
                    OrganizationID = await GetOrCreateDefaultOrganizationIdAsync()
                };
                _context.Users.Add(user);
                await _context.SaveChangesAsync();
            }

            var pilot = await _context.Pilots.FindAsync(user.UserID);
            if (pilot == null)
            {
                _context.Pilots.Add(new Pilot { UserID = user.UserID, ULicenseNumber = "N/A", AircraftType = "N/A" });
                await _context.SaveChangesAsync();
            }

            return user.UserID; // ReportItem.PilotID references Pilot(UserID)
        }
    }
}
