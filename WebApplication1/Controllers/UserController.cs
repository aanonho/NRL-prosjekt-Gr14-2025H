using Microsoft.AspNetCore.Mvc;
using WebApplication1.Models;
using System.Collections.Generic;
using System.Linq;

namespace WebApplication1.Controllers
{
    public class UserController : Controller
    {
        // Mock data (works w/o connected db)
        private static List<UserData> _users = new List<UserData>();
        // Static user - shows in user profile
        private static UserData? _currentUser = null;

        // Registration form for user data
        private static readonly List<UserData> _registeredUsers = new();


        [HttpGet]
        public ActionResult UserForm()
        {
            return View();
        }

        // Getting user data from the form submission, then displaying an overview

        [HttpPost]
        public ActionResult UserForm(UserData userData)
        {
            if (ModelState.IsValid)
            {
                // Legg til brukeren i lista
                _registeredUsers.Add(userData);

                // Send videre til oversiktsside
                return RedirectToAction("UserProfile", new { email = userData.Email });
            }

            // Hvis validering feiler, vis skjema på nytt
            return View(userData);
        }

        // 👤 Viser en spesifikk brukerprofil
        [HttpGet]
        public ActionResult UserProfile(string email)
        {
            var user = _registeredUsers.FirstOrDefault(u => u.Email == email);
            if (user == null)
            {
                return NotFound("User not found.");
            }

            return View(user);
        }

        // 📋 Gir tilgang til registrerte brukere (fra andre controllere)
        public static List<UserData> GetRegisteredUsers()
        {
            return _registeredUsers;
        }

        // Brukes for User-menyen
        public ActionResult Index()
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
