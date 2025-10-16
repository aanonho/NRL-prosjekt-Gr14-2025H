using Microsoft.AspNetCore.Mvc;
using WebApplication1.Models;
using WebApplication1.DataInfrastructure;

namespace WebApplication1.Controllers
{
    public class ObstacleController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ObstacleController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Blir kalt etter at vi trykker på "Register Obstacle"
        [HttpGet]
        public ActionResult DataForm()
        {
            return View();
        }

        // Blir kalt når vi trykker på "Submit Data" / "Save Draft"
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DataForm(ValidatedObstacleData validatedData, string submitType)
        {
            if (submitType == "Submit")
            {
                if (!ModelState.IsValid)
                {
                    return View(validatedData);
                }

                validatedData.IsDraft = false;
                _context.Add(validatedData);
                await _context.SaveChangesAsync();

                // Legg inn i enkel minneliste slik at Raports-siden kan vise det som blir sendt inn
                var item = new ReportItem
                {
                    /*
                    Title = validatedData.ObstacleName,
                    Description = validatedData.ObstacleDescription,
                    Height = validatedData.ObstacleHeight,
                    Latitude = validatedData.ObstacleLatitude,
                    Longitude = validatedData.ObstacleLongitude,
                    ObstacleType = validatedData.ObstacleType,
                    */
                    Obstacle = validatedData,
                    CreatedAt = validatedData.ObstacleRegistrationTime,
                    Status = "Pending",            // foreløpig fast verdi
                    Organization = "Unknown"       // foreløpig fast verdi
                };
                ReportStore.Add(item);

                return View("ObstacleRegistrationOverview", validatedData);
            }

            else if (submitType == "SaveDraft")
            {
                var draft = new ReportItem
                {
                    /*
                    Title = validatedData.ObstacleName,
                    Height = validatedData.ObstacleHeight,
                    Description = validatedData.ObstacleDescription,
                    Latitude = validatedData.ObstacleLatitude,
                    Longitude = validatedData.ObstacleLongitude,
                    ObstacleType = validatedData.ObstacleType,
                    Radius = validatedData.ObstacleRadius,
                    LineCoords = validatedData.ObstacleLineCoords,
                    IsDraft = true
                    */
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

