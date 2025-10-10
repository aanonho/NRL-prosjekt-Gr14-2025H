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
            }
            else if (submitType == "Submit" && ModelState.IsValid)
            {
                obstacleData.IsDraft = false;

                _context.Add(obstacleData);
                await _context.SaveChangesAsync();
            }
            else
            {
                // Hvis valideringen feiler, returner til skjemaet med valideringsfeil
                return View(obstacleData);
            }

            // Legg inn i enkel minneliste slik at Raports-siden kan vise det som blir sendt inn
            var item = new ReportItem
            {
                Title = obstacleData.ObstacleName,
                Description = obstacleData.ObstacleDescription,
                Height = obstacleData.ObstacleHeight,
                Latitude = obstacleData.ObstacleLatitude,
                Longitude = obstacleData.ObstacleLongitude,
                Type = obstacleData.ObstacleType,
                CreatedAt = obstacleData.ObstacleRegistrationTime,
                Status = "Pending",            // foreløpig fast verdi
                Organization = "Unknown"       // foreløpig fast verdi
            };
            ReportStore.Add(item);

            // Viser oversiktssiden som før --> endre til at visningen går til ObstacleRegistrationOverview, hvordan??
            return View("ObstacleRegistrationOverview", obstacleData);

        }
    }
}
