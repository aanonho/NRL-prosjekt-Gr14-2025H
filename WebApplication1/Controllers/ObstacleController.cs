// This file allows pilots and registrars to read, create, and update obstacle reports.
using System;
using System.Collections.Generic;
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
        private readonly ApplicationDbContext _context;

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

        // Dislays the form for creating a new obstacle report or editing an existing one.
        [HttpGet]
        public async Task<ActionResult> DataForm(int? id)
        {
            if (User.IsInRole("Registrar"))
            {
                ViewBag.IsRegistrarViewer = true;
            }

            if (!id.HasValue)
            {
                return View(new ObstacleData());
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
                ViewBag.IsEditing = false;
                ViewBag.ReadOnly = true;
                return View(report.ReportObstacle);
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

            return View(report.ReportObstacle);
        }

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

        // Handles submission and storage of obstacle reports, including image uploads.    
        private const long MaxImageSizeBytes = 10 * 1024 * 1024; // 10 MB limit for safety
        private static readonly string[] AllowedImageContentTypes = new[] { "image/jpeg", "image/png" };
        private static readonly string[] AllowedImageExtensions = new[] { ".jpg", ".jpeg", ".png" };

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DataForm(ValidatedObstacleData validatedData, List<IFormFile>? imageFiles, string submitType)
        {
            if (!User.IsInRole("Pilot"))
            {
                TempData["ErrorMessage"] = "Only pilots can create or save obstacle reports.";
                return RedirectToAction("Index", "Reports");
            }

            submitType ??= "Submit";
            var isSubmitRequest = string.Equals(submitType.Trim(), "Submit", StringComparison.OrdinalIgnoreCase);

            var email = UserHelper.GetCurrentUserEmail(this);
            if (string.IsNullOrEmpty(email))
                return RedirectToAction("UserForm", "User");

            var dbUser = await _context.Users
                .Include(u => u.Organization)
                .FirstOrDefaultAsync(u => u.Email == email);
            if (dbUser == null)
                return RedirectToAction("UserForm", "User");

            var isEditing = validatedData.ReportID > 0;

            // Handles updating of existing report with ownership validation.
            if (isEditing)
            {
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

                var storedImagePaths = ParseImagePaths(editReport.ReportObstacle.ImagePath);
                var imagesToRemove = ParseImagePaths(validatedData.ImagesToRemove);
                var remainingImagePaths = storedImagePaths
                    .Where(path => !imagesToRemove.Contains(path, StringComparer.OrdinalIgnoreCase))
                    .ToList();

                var updatedImagePaths = await ProcessImageUploadsAsync(imageFiles, remainingImagePaths);

                validatedData.ImagePath = string.Join(',', storedImagePaths);
                validatedData.ImagesToRemove = string.Join(',', imagesToRemove);

                if (!isSubmitRequest)
                {
                    ModelState.Clear();
                }

                if (!ModelState.IsValid)
                {
                    ViewBag.IsEditing = true;
                    ViewBag.ReportStatus = editReport.Status;
                    ViewBag.ReviewMessage = editReport.ReviewMessage;
                    validatedData.IsDraft = editReport.IsDraft;
                    ViewBag.ImagesToRemove = validatedData.ImagesToRemove;
                    return View(validatedData);
                }

                var editObstacle = editReport.ReportObstacle;

                editObstacle.ObstacleName = validatedData.ObstacleName;
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
                editObstacle.ImagePath = string.Join(',', updatedImagePaths);


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

                await DeleteImagesFromDiskAsync(imagesToRemove);

                return RedirectToAction("Index", "Reports");
            }

            // Creates a  new report and saves any uploaded images before validation.
            var newUploadPaths = await ProcessImageUploadsAsync(imageFiles, ParseImagePaths(validatedData.ImagePath));
            validatedData.ImagePath = string.Join(',', newUploadPaths);

            var hasImageErrors = ModelState.TryGetValue(nameof(ValidatedObstacleData.ImagePath), out var imageState)
                && imageState.Errors.Count > 0;

            if (isSubmitRequest)
            {
                ValidateSubmissionRequirements(validatedData);

                if (!ModelState.IsValid)
                {
                    ViewBag.ErrorMessage = "Please fill all required fields before submitting.";
                    ViewBag.IsEditing = false;
                    ViewBag.ReportStatus = "Draft";
                    ViewBag.ReviewMessage = string.Empty;
                    return View(validatedData);
                }
            }
            else if (hasImageErrors)
            {
                ViewBag.ErrorMessage = "Fix the highlighted image issues before saving.";
                ViewBag.IsEditing = false;
                ViewBag.ReportStatus = "Draft";
                ViewBag.ReviewMessage = string.Empty;
                return View(validatedData);
            }

            validatedData.ObstacleRegistrationTime = DateTime.Now;

            var organizationId = dbUser.OrganizationID;
            var organizationName = dbUser.Organization?.Name;

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

            var report = new ReportItem
            {
                CreatedAt = validatedData.ObstacleRegistrationTime,
                Status = isSubmitRequest ? "Pending" : "Draft",
                OrganizationID = organizationId,                // nullable FK
                PilotID = userEntity.UserID,                    // required FK
                IsDraft = !isSubmitRequest,
                CreatedBy = dbUser.Email,
                SubmittedByEmail = dbUser.Email,
                SubmittedByName = dbUser.Name,
                Organization = organizationName ?? "Unknown"
            };

            _context.ReportItems.Add(report);
            await _context.SaveChangesAsync();

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
                ObstacleHasLight = validatedData.ObstacleHasLight,
                ReportID = report.ReportID
            };

            _context.Obstacles.Add(obstacle);
            await _context.SaveChangesAsync();

            report.ReportObstacle = obstacle;
            _context.ReportItems.Update(report);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = string.Equals(submitType, "Submit", StringComparison.OrdinalIgnoreCase)
                ? "Your report has been submitted successfully."
                : "Draft saved successfully.";

            return RedirectToAction("Index", "Reports");
        }

        // Helper methods for image handling and required field validation.     
        private List<string> ParseImagePaths(string? storedPaths)
        {
            if (string.IsNullOrWhiteSpace(storedPaths))
            {
                return new List<string>();
            }

            return storedPaths
                .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .ToList();
        }

        private async Task DeleteImagesFromDiskAsync(IEnumerable<string> imagePaths)
        {
            if (imagePaths == null)
            {
                return;
            }

            foreach (var path in imagePaths)
            {
                if (string.IsNullOrWhiteSpace(path)) continue;

                var trimmed = path.TrimStart('/').Replace('/', Path.DirectorySeparatorChar);
                var fullPath = Path.Combine("wwwroot", trimmed);

                if (System.IO.File.Exists(fullPath))
                {
                    try
                    {
                        System.IO.File.Delete(fullPath);
                    }
                    catch (IOException)
                    {
                    }
                    catch (UnauthorizedAccessException)
                    {
                    }
                }
            }

            await Task.CompletedTask;
        }

        private async Task<List<string>> ProcessImageUploadsAsync(IEnumerable<IFormFile>? imageFiles, List<string> existingPaths)
        {
            var combinedPaths = new List<string>(existingPaths ?? new List<string>());

            if (imageFiles == null)
            {
                return combinedPaths;
            }

            foreach (var file in imageFiles)
            {
                if (file == null || file.Length == 0)
                {
                    continue;
                }

                var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
                var contentType = (file.ContentType ?? string.Empty).ToLowerInvariant();

                if (!AllowedImageExtensions.Contains(extension) || !AllowedImageContentTypes.Contains(contentType))
                {
                    ModelState.AddModelError(nameof(ValidatedObstacleData.ImagePath), "Only PNG and JPEG images are allowed.");
                    continue;
                }

                if (file.Length > MaxImageSizeBytes)
                {
                    ModelState.AddModelError(nameof(ValidatedObstacleData.ImagePath), $"Images must be {MaxImageSizeBytes / (1024 * 1024)} MB or smaller.");
                    continue;
                }

                var directory = Path.Combine("wwwroot", "images");
                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                var uniqueName = $"{Guid.NewGuid()}{extension}";
                var imagePath = Path.Combine(directory, uniqueName);

                using (var stream = new FileStream(imagePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                combinedPaths.Add($"/images/{uniqueName}");
            }

            return combinedPaths;
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

        [HttpGet]
        public async Task<IActionResult> Details(int id, string? returnUrl = null)
        {
            var report = await _context.ReportItems
                                       .Include(r => r.ReportObstacle)
                                       .Include(r => r.OrganizationRef)
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

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SubmitDraft(int id, string actionType, ValidatedObstacleData updatedData)
        {
            var report = await _context.ReportItems
                                       .Include(r => r.ReportObstacle)
                                       .Include(r => r.OrganizationRef)
                                       .FirstOrDefaultAsync(r => r.ReportID == id);
            if (report == null)
            {
                var fallbackEmail = TempData["CurrentUserEmail"] as string ?? string.Empty;
                return RedirectToAction("UserProfile", "User", new { email = fallbackEmail });
            }

            if (report.Obstacle == null)
                report.Obstacle = new ValidatedObstacleData();

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

            if (report.Obstacle != null)
            {
                report.Obstacle.ObstacleName = ObstacleName;
                report.Obstacle.ObstacleHeight = ObstacleHeight;
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
    }
}
