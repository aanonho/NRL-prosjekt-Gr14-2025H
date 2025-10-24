using Microsoft.AspNetCore.Mvc;
using WebApplication1.Models;

namespace WebApplication1.Controllers
{
    public class UserController : Controller
    {
        // Registration form for user data
        [HttpGet]
        public ActionResult UserForm()
        {
            return View();
        }
     
        // Getting user data from the form submission, then displaying an overview
        [HttpPost]
        public ActionResult UserForm(UserData userData)
        {
            return View("UserRegistrationOverview", userData);
        }

        // Displaying the user view
        [HttpGet]
        public IActionResult Index()
        {
            return View("User");
        }
    }
}
