using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using MySqlConnector;
using System.Linq;
using WebApplication1.Models;
using WebApplication1.Views;

namespace WebApplication1.Controllers
{
    public class ReportsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ReportsController(ApplicationDbContext context)
        {
            _context = context;
        }


        [HttpGet]

        public async Task<IActionResult> Index(
            string? type,
            string? organization,
            string? reporterName,
            string? status,
            DateTime? fromDate,
            DateTime? toDate,
            string? sort = "date_desc")
        {
            //query mot database
            var query = _context.Reports.AsNoTracking().AsQueryable();

            // --- FILTRERING ---
            if (!string.IsNullOrWhiteSpace(type))
                query = query.Where(r => r.Type == type);

            if (!string.IsNullOrWhiteSpace(organization))
                query = query.Where(r => r.Organization.Contains(organization));

            if (!string.IsNullOrWhiteSpace(reporterName))
                query = query.Where(r => r.ReporterName.Contains(reporterName));

            if (!string.IsNullOrWhiteSpace(status))
                query = query.Where(r => r.Status == status);

            if (fromDate.HasValue)
                query = query.Where(r => r.CreatedAt >= fromDate);

            if (toDate.HasValue)
                query = query.Where(r => r.CreatedAt <= toDate);

            // --- SORTERING ---

            query = sort switch
            {
                "date_asc" => query.OrderBy(r => r.CreatedAt),
                _ => query.OrderByDescending(query => query.CreatedAt) // default: date_desc
            };

            // --- EXECUTE ---

            var reports = await query.ToListAsync();

            // --- VIEW MODEL ---

            var vm = new ReportFilterViewModel
            {
                Reports = reports,
                Type = type,
                Organization = organization,
                ReporterName = reporterName,
                Status = status,
                FromDate = fromDate,
                ToDate = toDate
            };

            // Bruk egen view-fil for denne (1E) sabrine
            return View("ReportIndex", vm);


        }

        [HttpPost]
        public IActionResult Filter(ReportFilterViewModel model)
        {
            //samme POST Redirect GET som er brukt før
            return RedirectToAction("Index", new
                {
                type = model.Type,
                organization = model.Organization,
                reporterName = model.ReporterName,
                status = model.Status,
                fromDate = model.FromDate?.ToString("yyyy-MM-dd"),
                toDate = model.ToDate?.ToString("yyyy-MM-dd"),
                sort = model.Sort ?? "date_desc"
            });
        }


        /*  public IActionResult Index(string status = "all", string sort = "date_desc", string organization = "")
         {
           var all = ReportStore.GetAll();

        var filtered = all.AsEnumerable();

        // Filter by status if one of the known values is chosen
        var statusNormalized = (status ?? "all").Trim();
        if (!string.IsNullOrWhiteSpace(statusNormalized) &&
          !statusNormalized.Equals("all", StringComparison.OrdinalIgnoreCase))
        {
          filtered = filtered.Where(r =>
            string.Equals(r.Status, statusNormalized, StringComparison.OrdinalIgnoreCase));
        }

         Filter by organization (contains, case-insensitive)
        if (!string.IsNullOrWhiteSpace(organization))
        {
          var org = organization.Trim();
        filtered = filtered.Where(r =>
          (r.Organization ?? "").Contains(org, StringComparison.OrdinalIgnoreCase));
        }

         Sorting
        var sortNormalized = (sort ?? "date_desc").Trim().ToLowerInvariant();
        filtered = sortNormalized switch
        {
          "date_asc" => filtered.OrderBy(r => r.CreatedAt),
        _ => filtered.OrderByDescending(r => r.CreatedAt) // default newest first
        };

        var vm = new ReportsIndexViewModel
         {
            Reports = filtered.ToList(),
            Status = statusNormalized,
            Sort = sortNormalized,
            Organization = organization ?? "",
            TotalCount = all.Count,
            FilteredCount = filtered.Count()
         };

        return View(vm);
       }
        */
    }
}
