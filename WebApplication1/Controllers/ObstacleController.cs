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

        // For handling form submission and draft saving for obstacle data
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DataForm(ValidatedObstacleData validatedData, string submitType)
        {
            // Hent brukeren som er "innlogget" (fra mock-data)
            var currentUser = UserController.GetCurrentUser();

            if (submitType == "Submit")
            {
                if (!ModelState.IsValid)
                {
                    return View(validatedData);
                }

                validatedData.IsDraft = false;

                // Lag nytt report-item med brukerlink
                var item = new ReportItem
                {
                    Obstacle = validatedData,
                    CreatedAt = validatedData.ObstacleRegistrationTime,
                    Status = "Pending",
                    SubmittedByEmail = currentUser?.Email,
                    SubmittedByName = currentUser?.Name,
                    Organization = currentUser?.Organization ?? "Unknown"
                };

                // Legg til i reportstore (midlertidig minne)
                ReportStore.Add(item);

                // Saving validated data (local)
                _context.Add(validatedData);
                await _context.SaveChangesAsync();

                return View("ObstacleRegistrationOverview", validatedData);
            }

            else if (submitType == "SaveDraft")
            {
                validatedData.IsDraft = true;

                var draft = new ReportItem
                {
                    Obstacle = validatedData,
                    CreatedAt = validatedData.ObstacleRegistrationTime,
                    Status = "Draft",
                    SubmittedByEmail = currentUser?.Email,
                    SubmittedByName = currentUser?.Name,
                    Organization = currentUser?.Organization ?? "Unknown"
                };

                ReportStore.Add(draft);

                _context.Add(validatedData);
                await _context.SaveChangesAsync();

                return View("ObstacleRegistrationOverview", validatedData);
            }

            return View(validatedData);
        }
    }
 }   