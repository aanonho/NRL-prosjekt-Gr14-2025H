using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using WebApplication1.Models.Entities;
using WebApplication1.Models;
using MySqlConnector;

namespace WebApplication1.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger; // Logger for logging information not used yet
        private readonly string _connectionString;

        // Constructor to initialize logger and connection string
        public HomeController(IConfiguration config)
        {          
            _connectionString = config.GetConnectionString("DefaultConnection")!;
        }         
        public IActionResult Index()
        {       
            return RedirectToAction("Login", "Account");
        }   
    
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error() // Error handling action
        {           
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
