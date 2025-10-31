using Microsoft.AspNetCore.Mvc;
using WebApplication1.Models;
using WebApplication1.DataInfrastructure;

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

        // Blir kalt etter at vi trykker på "Register Obstacle"

        // For showing the form for obstacle data submission
        [HttpGet]
        public ActionResult DataForm()
        {
            return View();
        }


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

            // Flags editing mode in the view
            ViewBag.IsEditing = true;

            return View("Details", report);
        }



        // For handling form submission and draft saving for obstacle data
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DataForm(ValidatedObstacleData validatedData, IFormFile? imageFile, string submitType)
        {
            // 🖼️ Handle image upload
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

            // 👤 Get current user
            var currentUser = UserController.GetCurrentUser();
            if (currentUser == null)
                return RedirectToAction("UserForm", "User");

            // Common metadata
            validatedData.ObstacleRegistrationTime = DateTime.Now;

            // Create base ReportItem
            var report = new ReportItem
            {
                CreatedAt = validatedData.ObstacleRegistrationTime,
                Status = (submitType == "Submit") ? "Pending" : "Draft",
                OrganizationID = 1,
                PilotID = 1,
                IsDraft = submitType != "Submit",
                CreatedBy = currentUser.Email,
                SubmittedByEmail = currentUser.Email,
                SubmittedByName = currentUser.Name,
                Organization = currentUser.Organization ?? "Unknown"
            };

            // Save report to DB first (so we get the ReportID)
            _context.ReportStore.Add(report);
            await _context.SaveChangesAsync();

            // Convert ValidatedObstacleData → ObstacleData
            var obstacle = new ObstacleData
            {
                ObstacleName = validatedData.ObstacleName,
                ObstacleHeight = validatedData.ObstacleHeight,
                ObstacleDescription = validatedData.ObstacleDescription,
                ObstacleLatitude = validatedData.ObstacleLatitude,
                ObstacleLongitude = validatedData.ObstacleLongitude,
                ObstacleType = validatedData.ObstacleType,
                ObstacleRadius = validatedData.ObstacleRadius,
                ObstacleGeometry = validatedData.ObstacleGeometry,
                ObstacleLineCoordinates = validatedData.ObstacleLineCoordinates,
                ObstacleLineLength = validatedData.ObstacleLineLength,
                ImagePath = validatedData.ImagePath,
                ObstacleRegistrationTime = validatedData.ObstacleRegistrationTime,
                IsDraft = report.IsDraft,
                ReportID = report.ReportID // FK
            };

            // Save obstacle in DB
            _context.Obstacles.Add(obstacle);
            await _context.SaveChangesAsync();

            // Update ReportStore
            report.ReportObstacle = obstacle;
            _context.ReportStore.Update(report);
            await _context.SaveChangesAsync();

            // Redirect to user profile after save
            return RedirectToAction("UserProfile", "User", new { email = currentUser.Email });
        }


        [HttpGet]
        public IActionResult Details(int id, string? returnUrl = null)
        {
            // Try to find the report (search both drafts and submitted)
            var report = ReportStore.GetAll().FirstOrDefault(r => r.ReportID == id);

            if (report == null)
            {
                return NotFound();
            }

            var loggedUser = UserController.GetCurrentUser();

            // If the logged-in user is a registrar, redirect back to their dashboard
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

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult SubmitDraft(int id, string actionType, ValidatedObstacleData updatedData)
        {
            var report = ReportStore.GetAll().FirstOrDefault(r => r.ReportID == id);
            if (report == null)
                return RedirectToAction("UserProfile", "User");

            if (report.Obstacle == null)
                report.Obstacle = new ValidatedObstacleData();

            // Update the draft
            report.Obstacle.ObstacleName = updatedData.ObstacleName;
            report.Obstacle.ObstacleHeight = updatedData.ObstacleHeight;
            report.Obstacle.ObstacleDescription = updatedData.ObstacleDescription;

            if (actionType == "Submit")
            {
                report.IsDraft = false;
                report.Status = "Pending";
            }

            ReportStore.Update(report.ReportID, report);

            return RedirectToAction("Details", new { id = report.ReportID });
        }


        // === EDITING AN EXISTING DRAFT ===
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult UpdateDraft(int id, string ObstacleName, double ObstacleHeight, double? ObstacleLatitude, double? ObstacleLongitude, string ObstacleDescription, string? submitType)
        {
            var currentUser = UserController.GetCurrentUser();
            var report = ReportStore.GetReportsByUser(currentUser.Email)
                                    .FirstOrDefault(r => r.ReportID == id);

            if (report == null)
            {
                TempData["ErrorMessage"] = "Draft not found.";
                return RedirectToAction("UserProfile", "User", new { email = currentUser.Email });
            }

            // Update draft values
            if (report.Obstacle != null)
            {
                report.Obstacle.ObstacleName = ObstacleName;
                report.Obstacle.ObstacleHeight = ObstacleHeight;
            }

            if (submitType == "Submit")
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


    }
}
