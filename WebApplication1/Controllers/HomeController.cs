// Denne filen håndterer startsiden og feilsiden for løsningen.
using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using WebApplication1.Models.Entities;
using WebApplication1.Models;
using MySqlConnector;

namespace WebApplication1.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger; // Logger for generell informasjon selv om den ikke er tatt i bruk ennå
        private readonly string _connectionString;

        // Setter opp logger og henter connection string én gang i konstruktøren.
        public HomeController(IConfiguration config)
        {
            _connectionString = config.GetConnectionString("DefaultConnection")!;
        }
        public IActionResult Index()
        {
            // Sender brukeren rett til skjemaet for hinderrapporter.
            return RedirectToAction("Dataform", "Obstacle");
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error() // Viser standard feilmelding med sporfølger-ID.
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
