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

        // Blir kalt etter at vi trykker på "Submit Data"
        /*
        [HttpPost]
        [ValidateAntiForgeryToken] // for å sikre at rapporter er gyldige
        public async Task<IActionResult> DataForm(ObstacleData obstacleData, string submitType)
        {
            if (submitType == "SaveDraft")
            {
                // Tillat lagring av utkast selv om validering feiler
                obstacleData.IsDraft = true;

                _context.Add(obstacleData);
                await _context.SaveChangesAsync();

                return View("ObstacleRegistrationOverview", obstacleData);
            }
            else if (submitType == "Submit")
            {
                
                if (!ModelState.IsValid)
                {
                    foreach (var entry in ModelState)
                    {
                        foreach (var error in entry.Value.Errors)
                        {
                            Console.WriteLine($"Valideringsfeil i {entry.Key}: {error.ErrorMessage}");
                        }
                    }

                    // Hvis valideringen feiler, returner til skjemaet med valideringsfeil
                    return View(obstacleData);
                }
                

                obstacleData.IsDraft = false;

                _context.Add(obstacleData);
                await _context.SaveChangesAsync();

                // Legg inn i enkel minneliste slik at Raports-siden kan vise det som blir sendt inn
                var item = new ReportItem
                {
                    Title = obstacleData.ObstacleName,
                    Description = obstacleData.ObstacleDescription,
                    Height = obstacleData.ObstacleHeight,
                    Latitude = obstacleData.ObstacleLatitude,
                    Longitude = obstacleData.ObstacleLongitude,
                    ObstacleType = obstacleData.ObstacleType,
                    CreatedAt = obstacleData.ObstacleRegistrationTime,
                    Status = "Pending",            // foreløpig fast verdi
                    Organization = "Unknown"       // foreløpig fast verdi
                };
                ReportStore.Add(item);

                return View("ObstacleRegistrationOverview", obstacleData);
            }
            
            else
            {
                return View(obstacleData);
            }
        */

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

                return View("ObstacleRegistrationOverview", validatedData);
            }

            else if (submitType == "SaveDraft")
            {
                var draft = new ObstacleData
                {
                    ObstacleName = validatedData.ObstacleName,
                    ObstacleHeight = validatedData.ObstacleHeight,
                    ObstacleDescription = validatedData.ObstacleDescription,
                    ObstacleLatitude = validatedData.ObstacleLatitude,
                    ObstacleLongitude = validatedData.ObstacleLongitude,
                    ObstacleType = validatedData.ObstacleType,
                    ObstacleRadius = validatedData.ObstacleRadius,
                    ObstacleLineCoords = validatedData.ObstacleLineCoords,
                    IsDraft = true
                };

                _context.Add(draft);
                await _context.SaveChangesAsync();

                return View("ObstacleRegistrationOverview", draft);
            }

            return View(validatedData);
        }

    }
}

