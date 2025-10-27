using Microsoft.AspNetCore.Mvc;
using WebApplication1.Models;
using WebApplication1.DataInfrastructure;

namespace WebApplication1.Controllers
{
    public class ObstacleController : Controller
    {
        private readonly ApplicationDbContext _context; // Database context     

        public ObstacleController(ApplicationDbContext context)
        {
            _context = context;          
        }

        // For showing the form for obstacle data submission
        [HttpGet]
        public ActionResult DataForm()
        {            
            return View();
        }

        // For handling form submission and draft saving for obstacle data
        [HttpPost]
        [ValidateAntiForgeryToken]

        public async Task<IActionResult> DataForm(ValidatedObstacleData validatedData, string submitType)
        {
            // Determine action based on submitType
            if (submitType == "Submit")
            {              

                if (!ModelState.IsValid)
                {                  
                    return View(validatedData);
                }

                validatedData.IsDraft = false;
                _context.Add(validatedData);
                await _context.SaveChangesAsync();
             
                // Create a new ReportItem to store the submitted obstacle data
                var item = new ReportItem
                {
                    /*
                    Title = validatedData.ObstacleName,
                    Description = validatedData.ObstacleDescription,
                    Height = validatedData.ObstacleHeight,
                    Latitude = validatedData.ObstacleLatitude,
                    Longitude = validatedData.ObstacleLongitude,
                    ObstacleType = validatedData.ObstacleType,
                    */
                    Obstacle = validatedData,
                    CreatedAt = validatedData.ObstacleRegistrationTime,
                    Status = "Pending",            // foreløpig fast verdi
                    Organization = currentUser?.OrganizationName ?? "Unknown",       // foreløpig fast verdi

                    UserLink = new ReportItem.UserLink
                    {
                        SubmittedByEmail = currentUser?.Email ?? "Unknown",
                        SubmittedByName = currentUser?.Name ?? "Unknown",
                        OrganizationName = currentUser?.Organization ?? "Uknown"
                    }
                };

                // Connecting report to user (temp)
                var currentUser = UserController.GetRegisteredUsers().FirstOrDefault();
                if (currentUser != null)
                {
                    item.SubmittedByEmail = currentUser.Email;
                    item.SubmittedByName = currentUser.Name;
                    item.Organization = currentUser.Organization;
                    item.OrganizationName = currentUser.Organization;
                }

                // Add the new ReportItem to the ReportStore
                ReportStore.Add(item);

                return View("ObstacleRegistrationOverview", validatedData);
            }        
            else if (submitType == "SaveDraft")
            {             

                // Create a new ReportItem to store the draft obstacle data
                var draft = new ReportItem
                {
                    /*
                    Title = validatedData.ObstacleName,
                    Height = validatedData.ObstacleHeight,
                    Description = validatedData.ObstacleDescription,
                    Latitude = validatedData.ObstacleLatitude,
                    Longitude = validatedData.ObstacleLongitude,
                    ObstacleType = validatedData.ObstacleType,
                    Radius = validatedData.ObstacleRadius,
                    LineCoords = validatedData.ObstacleLineCoords,
                    IsDraft = true
                    */
                };
                ReportStore.Add(draft);

                _context.Add(validatedData);
                await _context.SaveChangesAsync();

                return View("ObstacleRegistrationOverview", validatedData);

                
                var currentUser = UserController.GetRegisteredUsers().FirstOrDefault();
                if (currentUser != null)
                {
                    draft.SubmittedByEmail = currentUser.Email;
                    draft.SubmittedByName = currentUser.Name;
                    draft.Organization = currentUser.Organization;
                    draft.OrganizationName = currentUser.Organization;
                }
                    ReportStore.Add(draft);
            }

            return View(validatedData);
        }

    }
}

