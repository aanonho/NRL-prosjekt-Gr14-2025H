using Microsoft.AspNetCore.Mvc;
using WebApplication1.Models;

namespace WebApplication1.Controllers
{
    public class UserController : Controller
    {
        // Mock data (works w/o connected db)
        private static List<UserData> _users = new List<UserData>();
        // Static user - shows in user profile
        private static UserData? _currentUser = null;

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
            if (_currentUser != null)
            {
                return View("UserProfile", _currentUser);
            }

            return RedirectToAction("UserForm");
        }

    [HttpPost]
        public IActionResult SaveUser(UserData userData)
        {
            // Validation
            if(!ModelState.IsValid)
            {
                return View("UserForm", userData);
            }

            // Add user in mock data list
            if (!_users.Any(u => u.Email == userData.Email))
            {
                _users.Add(userData);
            }

            // Setting current user
            _currentUser = userData;
            return RedirectToAction("UserProfile", new { email = userData.Email });
        }

        // Displaying user profile based on email
        [HttpGet]
        public IActionResult UserProfile(string email)
        {
            var user = _users.FirstOrDefault(u => u.Email == email);
            if (user == null)
            {
                return RedirectToAction("UserForm");
            }
            return View("UserProfile", user);
        }

        // Static method (other controllers can access mock-users)
        public static List<UserData> GetMockUsers()
        {
            return _users;
        }

    }
}
