// This file handles the home page and error page for the solution.
using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using WebApplication1.Models.Entities;
using WebApplication1.Models;
using MySqlConnector;

namespace WebApplication1.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger; // Logger for general informasjon even if not used yet.
                                                         
        private readonly string _connectionString;

        // Setting up logger and getting connection string once in the constructor.
        public HomeController(IConfiguration config)
        {
            _connectionString = config.GetConnectionString("DefaultConnection")!;
        }
        public IActionResult Index()
        {
            // Sends user to registration form      
            return RedirectToAction("Dataform", "Obstacle");
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error() // Displays default error message with trace ID.                                     
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
