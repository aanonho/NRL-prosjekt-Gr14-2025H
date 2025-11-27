using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.EntityFrameworkCore;
using WebApplication1.DataInfrastructure;
using WebApplication1.Models;
using WebApplication1.Models.Entities;
using WebApplication1.Helpers;

namespace WebApplication1.Controllers
{
    [Authorize]
    public class ObstacleController : Controller
    {
        private readonly ApplicationDbContext _context; // Database context

        public ObstacleController(ApplicationDbContext context)
        {
            _context = context;
        }

        [AllowAnonymous]
        [HttpGet("read")]
        public IActionResult ReadAll()
        {
            var data = _context.Obstacles.ToList();
            return Ok(data);
        }

        // === SHOW FORM ===
        [HttpGet]
        public async Task<ActionResult> DataForm(int? id)
        {
            if (User.IsInRole("Registrar"))
            {
                ViewBag.IsRegistrarViewer = true;
            }

            if (!id.HasValue)
            {
                return View(new ValidatedObstacleData{ObstacleType = "point"});
            }

            var email = UserHelper.GetCurrentUserEmail(this);
            if (string.IsNullOrEmpty(email))
            {
                return RedirectToAction("UserForm", "User");
            }

            var report = await _context.ReportItems
                .Include(r => r.ReportObstacle)
                .FirstOrDefaultAsync(r => r.ReportID == id.Value);

            if (report == null || report.ReportObstacle == null)
            {
                TempData["ErrorMessage"] = "Report not found.";
                return RedirectToAction("Index", "Reports");
            }
            if (!report.IsDraft && string.Equals(report.Status, "Approved", StringComparison.OrdinalIgnoreCase))
            {
                TempData["ErrorMessage"] = "Approved reports cannot be edited.";
                return RedirectToAction("Index", "Reports");
            }

            var normalizedEmail = email.Trim().ToLowerInvariant();
            var submittedEmail = (report.SubmittedByEmail ?? string.Empty).Trim().ToLowerInvariant();

            if (User.IsInRole("Pilot") && !User.IsInRole("Registrar") && normalizedEmail != submittedEmail)
            {
                TempData["ErrorMessage"] = "You can only edit reports that you submitted.";
                return RedirectToAction("Index", "Reports");
            }

            ViewBag.IsEditing = true;
            ViewBag.ReportIsDraft = report.IsDraft;
            ViewBag.ReportStatus = report.Status;
            ViewBag.ReviewMessage = report.ReviewMessage;

            report.ReportObstacle.IsDraft = report.IsDraft;
            report.ReportObstacle.ReportID = report.ReportID;

            var validatedData = new ValidatedObstacleData
            {
                ReportID = report.ReportID,
                ObstacleID = report.ReportObstacle.ObstacleID,
                ObstacleName = report.ReportObstacle.ObstacleName,
                ObstacleHeight = report.ReportObstacle.ObstacleHeight,
                ObstacleDescription = report.ReportObstacle.ObstacleDescription,
                ObstacleLatitude = report.ReportObstacle.ObstacleLatitude,
                ObstacleLongitude = report.ReportObstacle.ObstacleLongitude,
                ObstacleType = report.ReportObstacle.ObstacleType,
                ObstacleRadius = report.ReportObstacle.ObstacleRadius,
                ObstacleGeoJson = report.ReportObstacle.ObstacleGeoJson,
                ObstacleLineCoordinates = report.ReportObstacle.ObstacleLineCoordinates,
                ObstacleLineLength = report.ReportObstacle.ObstacleLineLength,
                ObstacleHasLight = report.ReportObstacle.ObstacleHasLight,
                ImagePath = report.ReportObstacle.ImagePath,
                IsDraft = report.IsDraft // Add any other necessary fields
            };

            return View(validatedData); ;
        }

        // === EDIT EXISTING REPORT (VIEW) ===
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var email = UserHelper.GetCurrentUserEmail(this);
            if (string.IsNullOrEmpty(email))
                return RedirectToAction("UserForm", "User");

            var currentUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
            if (currentUser == null)
                return RedirectToAction("UserForm", "User");

            var report = await _context.ReportItems
                .Include(r => r.ReportObstacle)
                .FirstOrDefaultAsync(r => r.ReportID == id);

            if (report == null || report.ReportObstacle == null)
            {
                return RedirectToAction("UserProfile", "User", new { email });
            }

            ViewBag.IsEditing = true;
            return View("Details", report);
        }

        // === HANDLE SUBMIT / SAVE DRAFT ===
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DataForm(ValidatedObstacleData validatedData, IFormFile? imageFile, string submitType)
        {
            if (!User.IsInRole("Pilot"))
            {
                TempData["ErrorMessage"] = "Only pilots can create or save obstacle reports.";
                return RedirectToAction("Index", "Reports");
            }

            submitType ??= "Submit";
            var isSubmitRequest = string.Equals(submitType.Trim(), "Submit", StringComparison.OrdinalIgnoreCase);

            if (string.Equals(submitType, "SaveDraft", StringComparison.OrdinalIgnoreCase))
            {
                // Remove ALL model errors so draft can save incomplete data
                ModelState.Clear();

                validatedData.IsDraft = true;
            }
            else
            {
                if (!ModelState.IsValid)
                {
                    // keep user on the form, showing validation errors
                    ViewBag.IsEditing = validatedData.ReportID > 0;
                    return View(validatedData);
                }

                validatedData.IsDraft = false;
            }

            // 1) Current user (required)
            var email = UserHelper.GetCurrentUserEmail(this);
            if (string.IsNullOrEmpty(email))
                return RedirectToAction("UserForm", "User");

            var dbUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
            if (dbUser == null)
                return RedirectToAction("UserForm", "User");

            var isEditing = validatedData.ReportID > 0;

            // Editing existing report/obstacle
            if (isEditing)
            {
                bool isDraftSave = string.Equals(submitType, "SaveDraft", StringComparison.OrdinalIgnoreCase);

                if (isDraftSave)
                {
                    // prevent any required validation from running
                    ModelState.Clear();
                }

                var editReport = await _context.ReportItems
                    .Include(r => r.ReportObstacle)
                    .FirstOrDefaultAsync(r => r.ReportID == validatedData.ReportID);

                if (editReport == null || editReport.ReportObstacle == null)
                {
                    TempData["ErrorMessage"] = "Report not found.";
                    return RedirectToAction("Index", "Reports");
                }


                var normalizedEmail = (dbUser.Email ?? string.Empty).Trim().ToLowerInvariant();
                var submittedEmail = (editReport.SubmittedByEmail ?? string.Empty).Trim().ToLowerInvariant();

                if (normalizedEmail != submittedEmail)
                {
                    TempData["ErrorMessage"] = "You can only edit reports that you submitted.";
                    return RedirectToAction("Index", "Reports");
                }

                if (isSubmitRequest)
                {
                    ValidateSubmissionRequirements(validatedData);
                }

                if (isSubmitRequest && !ModelState.IsValid)
                {
                    ViewBag.IsEditing = true;
                    ViewBag.ReportStatus = editReport.Status;
                    ViewBag.ReviewMessage = editReport.ReviewMessage;
                    validatedData.IsDraft = editReport.IsDraft;
                    return View(validatedData);
                }

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

                var editObstacle = editReport.ReportObstacle;

                editObstacle.ObstacleName = validatedData.ObstacleName ?? "(no title)";
                editObstacle.ObstacleHeight = validatedData.ObstacleHeight;
                editObstacle.ObstacleDescription = validatedData.ObstacleDescription;
                editObstacle.ObstacleHasLight = validatedData.ObstacleHasLight;
                editObstacle.ObstacleLatitude = validatedData.ObstacleLatitude;
                editObstacle.ObstacleLongitude = validatedData.ObstacleLongitude;
                editObstacle.ObstacleType = validatedData.ObstacleType;
                editObstacle.ObstacleRadius = validatedData.ObstacleRadius;
                editObstacle.ObstacleGeoJson = validatedData.ObstacleGeoJson;
                editObstacle.ObstacleLineCoordinates = validatedData.ObstacleLineCoordinates;
                editObstacle.ObstacleLineLength = validatedData.ObstacleLineLength;
                editObstacle.ImagePath = validatedData.ImagePath ?? editObstacle.ImagePath;


                if (editReport.IsDraft)
                {
                    if (isSubmitRequest)
                    {
                        editReport.IsDraft = false;
                        editReport.Status = "Pending";
                        editObstacle.IsDraft = false;
                        TempData["SuccessMessage"] = "Report submitted successfully.";
                    }
                    else
                    {
                        editReport.IsDraft = true;
                        editReport.Status = "Draft";
                        editObstacle.IsDraft = true;
                        TempData["SuccessMessage"] = "Draft updated.";
                    }
                }
                else
                {
                    editReport.IsDraft = false;
                    editObstacle.IsDraft = false;
                    if (isSubmitRequest)
                    {
                        editReport.Status = "Pending";
                        TempData["SuccessMessage"] = "Report updated and resubmitted for review.";
                    }
                    else
                    {
                        TempData["SuccessMessage"] = " The changes have been successfully saved.";
                    }
                }

                _context.ReportItems.Update(editReport);
                _context.Obstacles.Update(editObstacle);
                await _context.SaveChangesAsync();

                return RedirectToAction("Index", "Reports");
            }

            // === Creating a new report ===

            if (isSubmitRequest)
            {
                ValidateSubmissionRequirements(validatedData);
                if (!ModelState.IsValid)
                {
                    ViewBag.IsEditing = false;
                    ViewBag.ReportStatus = "Draft";
                    ViewBag.ReviewMessage = string.Empty;
                    return View(validatedData);
                }
            }

            // 2) Image upload
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

            validatedData.ObstacleRegistrationTime = DateTime.Now;

            // 3) Resolve or create Organization
            int? organizationId = null;
            if (dbUser.Organization != null && !string.IsNullOrWhiteSpace(dbUser.Organization.Name))
            {
                var orgName = dbUser.Organization.Name.Trim();
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
            var emailKey = (dbUser.Email ?? string.Empty).Trim();
            if (emailKey.Length > 45) emailKey = emailKey.Substring(0, 45);

            var userEntity = await _context.Users.FirstOrDefaultAsync(u => u.Email == emailKey);
            if (userEntity == null)
            {
                userEntity = new UserEntity
                {
                    Name = dbUser.Name,
                    Email = emailKey,
                    Phone = dbUser.Phone,
                    Role = string.IsNullOrWhiteSpace(dbUser.Role) ? "Pilot" : dbUser.Role,
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
                ObstacleName = validatedData.ObstacleName ?? "(no title)",
                Status = isSubmitRequest ? "Pending" : "Draft",
                OrganizationID = organizationId,                // nullable FK
                PilotID = userEntity.UserID,                    // required FK
                IsDraft = !isSubmitRequest,
                CreatedBy = dbUser.Email,
                SubmittedByEmail = dbUser.Email,
                SubmittedByName = dbUser.Name,
                Organization = dbUser.Organization != null ? dbUser.Organization.Name : "Unknown"
            };

            _context.ReportItems.Add(report);
            await _context.SaveChangesAsync();

            // 6) Save the obstacle and link it
            var obstacle = new ValidatedObstacleData
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
                ObstacleHasLight = validatedData.ObstacleHasLight,
                ReportID = report.ReportID // FK
            };

            _context.Obstacles.Add(obstacle);
            await _context.SaveChangesAsync();

            // 7) Update Report with its obstacle record
            report.ReportObstacle = obstacle;
            _context.ReportItems.Update(report);
            await _context.SaveChangesAsync();

            // 8) Redirect
            TempData["SuccessMessage"] = string.Equals(submitType, "Submit", StringComparison.OrdinalIgnoreCase)
                ? "Your report has been submitted successfully."
                : "Draft saved successfully.";

            return RedirectToAction("Index", "Reports");
        }
        private void ValidateSubmissionRequirements(ValidatedObstacleData data)
        {
            if (string.IsNullOrWhiteSpace(data.ObstacleType))
            {
                ModelState.AddModelError(nameof(data.ObstacleType), "Please select an obstacle type before submitting.");
            }

            var normalizedType = data.ObstacleType?.Trim().ToLowerInvariant();

            if (normalizedType == "line")
            {
                if (string.IsNullOrWhiteSpace(data.ObstacleLineCoordinates))
                {
                    ModelState.AddModelError(nameof(data.ObstacleLineCoordinates), "Line coordinates are required to submit.");
                }
            }
            else if (normalizedType == "point" || normalizedType == "area")
            {
                if (!data.ObstacleLatitude.HasValue || !data.ObstacleLongitude.HasValue)
                {
                    ModelState.AddModelError(nameof(data.ObstacleLatitude), "Latitude and longitude are required to submit.");
                }

                if (string.IsNullOrWhiteSpace(data.ObstacleGeoJson))
                {
                    ModelState.AddModelError(nameof(data.ObstacleGeoJson), "Location data is required to submit.");
                }
            }
        }

        // === DETAILS VIEW ===
        [HttpGet]
        public async Task<IActionResult> Details(int id, string? returnUrl = null)
        {
            var report = await _context.ReportItems
                                       .Include(r => r.ReportObstacle)
                                       .FirstOrDefaultAsync(r => r.ReportID == id);
            if (report == null)
            {
                return NotFound();
            }

            var email = TempData["CurrentUserEmail"] as string;
            if (string.IsNullOrEmpty(email))
            {
                return RedirectToAction("UserForm", "User");
            }

            var loggedUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
            if (loggedUser == null)
            {
                return RedirectToAction("UserForm", "User");
            }

            if (string.IsNullOrEmpty(returnUrl))
            {
                returnUrl = loggedUser.Role?.ToLower() == "registrar"
                    ? Url.Action("RegistrarDashboard", "User")
                    : Url.Action("UserProfile", "User", new { email = loggedUser.Email });
            }

            ViewBag.ReturnUrl = returnUrl;
            ViewBag.CurrentUser = loggedUser;

            return View(report);
        }

        // === SUBMIT DRAFT (FROM DETAILS) ===
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SubmitDraft(int id, string actionType, ValidatedObstacleData updatedData)
        {
            var report = await _context.ReportItems
                                       .Include(r => r.ReportObstacle)
                                       .FirstOrDefaultAsync(r => r.ReportID == id);
            if (report == null)
            {
                var fallbackEmail = TempData["CurrentUserEmail"] as string ?? string.Empty;
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

            _context.ReportItems.Update(report);
            return RedirectToAction("Details", new { id = report.ReportID });
        }

        // === EDITING AN EXISTING DRAFT (FORM POST) ===
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateDraftAsync(int id, string ObstacleName, double ObstacleHeight, double? ObstacleLatitude, double? ObstacleLongitude, string ObstacleDescription, string? submitType)
        {
            var email = TempData["CurrentUserEmail"] as string;
            if (string.IsNullOrEmpty(email))
                return RedirectToAction("UserForm", "User");

            var currentUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
            if (currentUser == null)
                return RedirectToAction("UserForm", "User");

            var report = await _context.ReportItems
                                       .Include(r => r.ReportObstacle)
                                       .FirstOrDefaultAsync(r => r.ReportID == id && r.SubmittedByEmail == currentUser.Email);

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

            _context.ReportItems.Update(report);
            await _context.SaveChangesAsync();

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
