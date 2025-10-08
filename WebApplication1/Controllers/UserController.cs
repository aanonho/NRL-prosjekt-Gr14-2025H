using Microsoft.AspNetCore.Mvc;
using WebApplication1.Models;

namespace WebApplication1.Controllers
{
    public class UserController : Controller
    {
        // Viser skjema for brukerdata
        [HttpGet]
        public ActionResult UserForm()
        {
            return View();
        }

        // Mottar skjema; viser oppsummeringen med innsendte data
        [HttpPost]
        public ActionResult UserForm(UserData userData)
        {
            return View("UserRegistrationOverview", userData);
        }


        [HttpGet]
        public IActionResult Index()
        {
            return View("User");
        }
    }
}
