using Microsoft.AspNetCore.Mvc;
using WebApplication1.Models;

namespace WebApplication1.Controllers
{
    public class ObstacleController : Controller
    {
        //Blir kalt etter at vi trykkr p� "Register Obstacle" lenken i Index viewet 
        [HttpGet]
        public ActionResult DataForm()
        {
            return View();
        }

        //Blir kalt etter at vi trykkr p� "Submit Data" lenken i viewet 
        [HttpPost]
        public ActionResult DataForm(ObstacleData obstacleData)
        {
            if (!ModelState.IsValid)
            {
                // Hvis valideringen feiler, returner til skjemaet med valideringsfeil
                return View(obstacleData);
            }
            return View("ObstacleRegistrationOverview", obstacleData);
        }
       
    }
}
