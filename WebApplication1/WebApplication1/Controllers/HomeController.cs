using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using WebApplication1.Models;
using MySqlConnector;

namespace WebApplication1.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        //public HomeController(ILogger<HomeController> logger)
        //{
        //    _logger = logger;
        //}

        private readonly string _connectionString;

        public HomeController(IConfiguration config)
        {
            _connectionString = config.GetConnectionString("DefaultConnection")!;
        }

        //public async Task<IActionResult> Index()
        //{
        //    try
        //    {
        //        await using var conn = new MySqlConnection(_connectionString);
        //        await conn.OpenAsync();
        //        return Content("Conncected to MariaDB Successfully!");
        //    }
        //    catch (Exception ex)
        //    {
        //        return Content("Failed to connect to MariDB: " + ex.Message);
        //    }
        //}

        [HttpGet]
        public ViewResult UserForm()
        {
            return View();
        }

        [HttpPost]
        public ViewResult UserForm(Userdata userData)
        {
            return View("RegistrationOverview", userData);
        }
        public IActionResult User()
        {
            return View();
        }
        public IActionResult ContactUs()
        {
            return View();
        }
        public IActionResult Index()
        {
            return View();
        }

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
            return View("ObstacleRegistrationOverview", obstacleData);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
