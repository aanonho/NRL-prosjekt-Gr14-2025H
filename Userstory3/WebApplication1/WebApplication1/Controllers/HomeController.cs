using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using MySqlConnector;
using WebApplication1.Models;

namespace WebApplication1.Controllers
{
    public class HomeController : Controller
    {
        private readonly string _connectionString;

        public HomeController(IConfiguration config)
        {
            _connectionString =
                config.GetConnectionString("Default") ??
                config.GetConnectionString("DefaultConnection") ??
                throw new InvalidOperationException("Mangler connection string.");
        }

        public IActionResult Index() => View();
        public IActionResult ContactUs() => View();
        public IActionResult Privacy() => View();

        // Hvis du har "User.cshtml"
        public IActionResult UserPage() => View("User");

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error() =>
            View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });

        // DB-helse: /Home/DbCheck
        [HttpGet]
        public async Task<IActionResult> DbCheck()
        {
            try
            {
                await using var conn = new MySqlConnection(_connectionString);
                await conn.OpenAsync();
                await using var cmd = new MySqlCommand("SELECT NOW();", conn);
                var now = (DateTime?)await cmd.ExecuteScalarAsync();
                return Content($"DB OK – server time: {now:O}");
            }
            catch (Exception ex)
            {
                return Content("DB FEIL: " + ex.Message);
            }
        }

        // ---- Skjema for hinderregistrering ----
        [HttpGet]
        public ActionResult DataForm() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DataForm(ObstacleData obstacleData)
        {
            if (!ModelState.IsValid) return View(obstacleData);

            if (obstacleData.ReporterLatitude.HasValue &&
                obstacleData.ReporterLongitude.HasValue &&
                obstacleData.CapturedAt == null)
            {
                obstacleData.CapturedAt = DateTime.UtcNow;
            }

            // (Lagring kan legges inn senere)
            return View("ObstacleRegistrationOverview", obstacleData);
        }

        // ---- UserForm (om du bruker den) ----
        [HttpGet]
        public ViewResult UserForm() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ViewResult UserForm(UserData userData)
        {
            if (!ModelState.IsValid) return View(userData);
            return View("RegistrationOverview", userData);
        }
    }
}
