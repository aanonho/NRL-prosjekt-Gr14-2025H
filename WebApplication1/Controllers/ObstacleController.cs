using Microsoft.AspNetCore.Mvc;
using WebApplication1.Models;

namespace WebApplication1.Controllers
{
    public class ObstacleController : Controller
    {
        // Blir kalt etter at vi trykker på "Register Obstacle"
        [HttpGet]
        public ActionResult DataForm()
        {
            return View();
        }

        // Blir kalt etter at vi trykker på "Submit Data"
        [HttpPost]
        public ActionResult DataForm(ObstacleData obstacleData)
        {
            if (!ModelState.IsValid)
            {
                // Hvis valideringen feiler, returner til skjemaet med valideringsfeil
                return View(obstacleData);
            }

            // Legg inn i enkel minneliste slik at Raports-siden kan vise det som blir sendt inn
            var item = new ReportItem
            {
                //Title = obstacleData.ObstacleName,
                //Description = obstacleData.ObstacleDescription,
                //Height = obstacleData.ObstacleHeight,
                //Latitude = obstacleData.ObstacleLatitude,
                //Longitude = obstacleData.ObstacleLongitude,
                //Type = obstacleData.ObstacleType,

                Obstacle = obstacleData, //Composition for ObstacleData
                CreatedAt = obstacleData.ObstacleRegistrationTime,
                Status = "Pending",            // foreløpig fast verdi
                Organization = "Unknown"       // foreløpig fast verdi
            };
            ReportStore.Add(item);

            // Vis oversiktssiden som før
            return View("ObstacleRegistrationOverview", obstacleData);
        }
    }
}
