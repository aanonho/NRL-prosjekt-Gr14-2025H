using Microsoft.AspNetCore.Mvc;
using WebApplication1.Models;
using MySqlConnector;
using Microsoft.Extensions.Configuration;

namespace WebApplication1.Controllers
{
    public class ObstacleController : Controller
    {

        //Legger til connectionstring for å lagre data til Reports
        private readonly string _connectionString = "server=db;port=3306;database=nrl;user=nrl;password=nrlpass;";

        public ObstacleController(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        // Blir kalt etter at vi trykker på "Register Obstacle"
        [HttpGet]
        public ActionResult DataForm()
        {
            return View();
        }

        // Blir kalt etter at vi trykker på "Submit Data"
        [HttpPost]
        public ActionResult DataForm(ObstacleData obstacleData)
        {
            if (!ModelState.IsValid)
            {
                // Hvis valideringen feiler, returner til skjemaet med valideringsfeil
                return View(obstacleData);
            }

            /* Legg inn i enkel minneliste slik at Raports-siden kan vise det som blir sendt inn
             var item = new ReportItem
            {
               Title = obstacleData.ObstacleName,
               Description = obstacleData.ObstacleDescription,
               Height = obstacleData.ObstacleHeight,
               Latitude = obstacleData.ObstacleLatitude,
               Longitude = obstacleData.ObstacleLongitude,
               Type = obstacleData.ObstacleType,
               CreatedAt = obstacleData.ObstacleRegistrationTime,
               Status = "Pending",            // foreløpig fast verdi
               Organization = "Unknown"       // foreløpig fast verdi
             };
             ReportStore.Add(item);
            */


         //temp ole slett
            using (var conn = new MySqlConnection(_connectionString))
            {
                Console.WriteLine("➡️ Attempting to connect to MariaDB...");
                try
                {
                    conn.Open();
                    Console.WriteLine("✅ Connected successfully to MariaDB!");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"❌ Failed to connect to MariaDB: {ex.Message}");
                }
            }            //temp ole 


            using (var conn = new MySqlConnection(_connectionString))
            {
                conn.Open();
                var sql = @"INSERT INTO Reports (Type, Organization, ReportName, DataReported, Status)
                          VALUES (@Type, @Org, @ReporterName, @DateReported, @Status)";
                //ole test over

                //var sql = @"INSERT INTO Reports (Type, Organization, ReportName, DataReported, Status)
                //          VALUES (@Type, @Org, @ReporterName, @DateReported, @StatusCode)";
                using (var cmd = new MySqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@Type", obstacleData.ObstacleType);
                    cmd.Parameters.AddWithValue("@Org", "Unknown");
                    cmd.Parameters.AddWithValue("@ReporterName", obstacleData.ObstacleName);
                    cmd.Parameters.AddWithValue("@DateReported", DateTime.Now);
                    cmd.Parameters.AddWithValue("@Status", "Pending");
                                        cmd.ExecuteNonQuery();
                }
                conn.Close();
            }

                // Vis oversiktssiden som før
                return View("ObstacleRegistrationOverview", obstacleData);
        }
    }
}
