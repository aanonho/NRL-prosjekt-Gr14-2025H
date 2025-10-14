using Microsoft.AspNetCore.Mvc;

namespace WebApplication1.Controllers
{
    public class AuthController : Controller
    {
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Login(string username, string password)
        {
            // Enkel autentisering (for demo-formål)
            var users = new List<(string Username, string Passoword, string Role)>
            {
                ("admin", "root", "Admin"),
                ("registrar", "1234", "Registrar"),
                ("pilot", "abcd", "Pilot")
            };

            var user = users.FirstOrDefault(u =>
            u.Username.Equals(username, StringComparison.OrdinalIgnoreCase)
            && u.Passoword == password);

            if (user == default)
            {
                ViewBag.Error = "Invalid username or password.";
                return View();
            }
            
            //Lagre brukerdata i session
            HttpContext.Session.SetString("Username", user.Username);
            HttpContext.Session.SetString("Role", user.Role);

            //Sendes videre til Reports-siden etter innlogging
            return RedirectToAction("Index", "Reports");
        }

        [HttpGet]
        public IActionResult Logout()
        {
            //Tøm session
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }
    }
}
