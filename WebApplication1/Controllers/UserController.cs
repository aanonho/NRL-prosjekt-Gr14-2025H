using Microsoft.AspNetCore.Mvc;
using WebApplication1.Models;

namespace WebApplication1.Controllers
{
    public class UserController : Controller
    {
        [HttpGet]
        public ActionResult UserForm()
        {
            return View();
        }

        [HttpPost]
        public ActionResult UserForm(UserData userData)
        {
            return View("UserRegistrationOverview", userData);
        }

        public IActionResult User()
        {
            return View();
        }

    }
}
