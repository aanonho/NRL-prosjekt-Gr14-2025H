using Microsoft.AspNetCore.Mvc;
using MySqlConnector;
using WebApplication1.ViewModels;
using WebApplication1.Models;

namespace WebApplication1.Controllers

{
    public class ReportsController : Controller
    {
        private readonly string _cs;

        public ReportsController(IConfiguration config)
        {
            _cs = config.GetConnectionString("DefaultConnection");
        }

        [HttpGet]
        public async Task<IActionResult> Index(string? type, string? organization, string? reporterName, string? status, DateTime? fromDate, DateTime? toDate)
        {
            var reports = new List<Report>();

            await using var conn = new MySqlConnection(_cs);

            await conn.OpenAsync();
            var sql = "SELECT  Id, Type, Organization, ReporterName, DateReported, Status FROM Reports WHERE 1=1";
            
            if (!string.IsNullOrEmpty(type))
                sql += " AND Type = @Type";

            if (!string.IsNullOrEmpty(organization))
                sql += " AND Organization = @Org";

            if (!string.IsNullOrEmpty(reporterName))
                sql += " AND ReporterName = @Reporter";

            if (!string.IsNullOrEmpty(status))
                sql += " AND Status = @Status";

            if (fromDate.HasValue)
                sql += " AND DateReported >= @FromDate";

            if (toDate.HasValue)
                sql += " AND DateReported <= @ToDate";

            using var cmd = new MySqlCommand(sql, conn);

            if (!string.IsNullOrEmpty(type))
                cmd.Parameters.AddWithValue("@Type", type);

            if (!string.IsNullOrEmpty(organization))
                cmd.Parameters.AddWithValue("@Org", organization);

            if (!string.IsNullOrEmpty(reporterName))
                cmd.Parameters.AddWithValue("@Reporter", reporterName);

            if (!string.IsNullOrEmpty(status))
                cmd.Parameters.AddWithValue("@Status", status);
            
            if (fromDate.HasValue)
                cmd.Parameters.AddWithValue("@FromDate", fromDate.Value);

            if (toDate.HasValue)
                cmd.Parameters.AddWithValue("@ToDate", toDate.Value);

            using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                reports.Add(new Report
                {
                    Id = reader.GetInt32("Id"),
                    Type = reader.GetString("Type"),
                    Organization = reader.GetString("Organization"),
                    ReporterName = reader.GetString("ReporterName"),
                    DateReported = reader.GetDateTime("DateReported"),
                    Status = reader.GetString("Status")
                });
            }

            await conn.CloseAsync();

            var repvm = new ReportFilterViewModel
            {
                Reports = reports,
                Type = type,
                Organization = organization,
                ReporterName = reporterName,
                Status = status,
                ToDate = toDate,
                FromDate = fromDate
            };

            return View("ReportIndex", repvm);

        }

        [HttpPost]

        public IActionResult Filter(ReportFilterViewModel model)
            {
            return RedirectToAction("Index", new
            {
                type = model.Type,
                organization = model.Organization,
                reporterName = model.ReporterName,
                status = model.Status,
                fromDate = model.FromDate?.ToString("yyyy-MM-dd"),
                toDate = model.ToDate?.ToString("yyyy-MM-dd")
            });
        }
    }
}
