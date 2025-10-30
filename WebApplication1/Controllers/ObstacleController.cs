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

        // For showing the form for obstacle data submission
        [HttpGet]
        public ActionResult DataForm()
        {
            return View();
        }


        [HttpGet]
        public IActionResult Edit(Guid id)
        {
            var report = ReportStore.GetAll().FirstOrDefault(r => r.Id == id);

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
            if (imageFile != null && imageFile.Length > 0)
            {
                var directory = Path.Combine("wwwroot", "images");
               
                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }
                var fileName = Path.GetFileName(imageFile.FileName);

                var imagePath = Path.Combine(directory, fileName);

                using (var stream = new FileStream(imagePath, FileMode.Create))
                {
                    await imageFile.CopyToAsync(stream);
                }

                validatedData.ImagePath = "/images/" + fileName;
            }

            //// Save the image file to a specific location and get the path
            //var imagePath = Path.Combine("wwwroot/images", imageFile.FileName);
            //using (var stream = new FileStream(imagePath, FileMode.Create))
            //{
            //    await imageFile.CopyToAsync(stream);
            //}

            //// Set the ImagePath property in the database model
            //validatedData.ImagePath = "/images/" + imageFile.FileName;

            // Determine action based on submitType
            var currentUser = UserController.GetCurrentUser();
            if (currentUser == null)
                return RedirectToAction("UserForm", "User");

            // Common metadata setup
            validatedData.ObstacleRegistrationTime = DateTime.Now;

            // Get existing draft (if any)
            var existing = ReportStore.GetReportsByUser(currentUser.Email)
                                      .FirstOrDefault(r => r.Id == validatedData.Id);

            // === SUBMIT FINAL REPORT ===
            if (submitType == "Submit")
            {
                if (!ModelState.IsValid)
                    return View(validatedData);

                validatedData.IsDraft = false;
                
                    
                if (existing != null)
                {
                    // Update existing draft
                    existing.Obstacle = validatedData;
                    existing.Status = "Pending";
                    existing.IsDraft = false;
                    ReportStore.Update(existing.Id, existing);

                }
                else
                {
                    // Create new submitted report
                    var newReport = new ReportItem
                    {
                        Id = Guid.NewGuid(),
                        Obstacle = validatedData,
                        CreatedAt = validatedData.ObstacleRegistrationTime,
                        CreatedBy = currentUser.Email,
                        Status = "Pending",
                        IsDraft = false,
                        SubmittedByEmail = currentUser.Email,
                        SubmittedByName = currentUser.Name,
                        Organization = currentUser.Organization ?? "Unknown"
                    };
                    ReportStore.Add(newReport);
                }

                // Persist obstacle (local db)
                _context.Add(validatedData);
                await _context.SaveChangesAsync();

                return RedirectToAction("UserProfile", "User", new { email = currentUser.Email });
            }

            // === SAVE AS DRAFT ===
            else if (submitType == "SaveDraft")
            {
                validatedData.IsDraft = true;

                if (existing != null)
                {
                    // Update draft instead of creating new
                    existing.Obstacle = validatedData;
                    existing.Status = "Draft";
                    existing.IsDraft = true;
                }
                else
                {
                    // Create new draft
                    var draft = new ReportItem
                    {
                        Id = Guid.NewGuid(),
                        Obstacle = validatedData,
                        CreatedAt = validatedData.ObstacleRegistrationTime,
                        CreatedBy = currentUser.Email,
                        Status = "Draft",
                        IsDraft = true,
                        SubmittedByEmail = currentUser.Email,
                        SubmittedByName = currentUser.Name,
                        Organization = currentUser.Organization ?? "Unknown"
                    };
                    ReportStore.Add(draft);
                }

               
            }

            _context.Add(validatedData);
            await _context.SaveChangesAsync();

                return RedirectToAction("UserProfile", "User", new { email = currentUser.Email });
            

            // === FALLBACK ===
            return View(validatedData);
        }


        [HttpGet]
        public IActionResult Details(Guid id, string? returnUrl = null)
        {
            // Try to find the report (search both drafts and submitted)
            var report = ReportStore.GetAll().FirstOrDefault(r => r.Id == id);

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
        public IActionResult SubmitDraft(Guid id, string actionType, ValidatedObstacleData updatedData)
        {
            var report = ReportStore.GetAll().FirstOrDefault(r => r.Id == id);
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

            ReportStore.Update(report.Id, report);

            return RedirectToAction("Details", new { id = report.Id });
        }


        // === EDITING AN EXISTING DRAFT ===
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult UpdateDraft(Guid id, string ObstacleName, double ObstacleHeight, double? ObstacleLatitude, double? ObstacleLongitude, string ObstacleDescription, string? submitType)
        {
            var currentUser = UserController.GetCurrentUser();
            var report = ReportStore.GetReportsByUser(currentUser.Email)
                                    .FirstOrDefault(r => r.Id == id);

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

            return RedirectToAction("Details", "Obstacle", new { id = report.Id });
        }


    }
}   