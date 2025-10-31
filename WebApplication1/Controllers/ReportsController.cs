using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using WebApplication1.Models;
using WebApplication1.DataInfrastructure;

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
        public IActionResult Index(string status = "all", string sort = "date_desc", string organization = "")
        {
            // Get Reports from DB
            var all = _context.ReportStore
                .Include(r => r.ReportObstacle)
                .ToList();

            var filtered = all.AsEnumerable();

            // --- Filter by status ---
            var statusNormalized = (status ?? "all").Trim();
            if (!string.IsNullOrWhiteSpace(statusNormalized) &&
                !statusNormalized.Equals("all", StringComparison.OrdinalIgnoreCase))
            {
                if (statusNormalized.Equals("draft", StringComparison.OrdinalIgnoreCase))
                    filtered = filtered.Where(r => r.IsDraft);
                else if (statusNormalized.Equals("submitted", StringComparison.OrdinalIgnoreCase))
                    filtered = filtered.Where(r => !r.IsDraft);
                else
                    filtered = filtered.Where(r =>
                        string.Equals(r.Status, statusNormalized, StringComparison.OrdinalIgnoreCase));
            }

            // --- Filter by organization ---
            if (!string.IsNullOrWhiteSpace(organization))
            {
                var org = organization.Trim();
                filtered = filtered.Where(r =>
                    (r.Organization ?? "").Contains(org, StringComparison.OrdinalIgnoreCase));
            }

            // --- Sorting ---
            var sortNormalized = (sort ?? "date_desc").Trim().ToLowerInvariant();
            filtered = sortNormalized switch
            {
                "date_asc" => filtered.OrderBy(r => r.CreatedAt),
                _ => filtered.OrderByDescending(r => r.CreatedAt) // default
            };

            // --- Prepare view model ---
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

        [HttpGet]
        public IActionResult ReportStatus(string status = "all")
        {
            var allReports = _context.ReportStore.ToList();

            var filtered = status.ToLower() switch
            {
                "pending" => allReports.Where(r => r.Status == "Pending"),
                "approved" => allReports.Where(r => r.Status == "Approved"),
                "rejected" => allReports.Where(r => r.Status == "Rejected"),
                _ => allReports
            };

            return View(filtered.ToList());
        }

        [HttpPost]
        public IActionResult UpdateStatus(int id, string status, string message)
        {
            var report = _context.ReportStore.FirstOrDefault(r => r.ReportID == id);
            if (report == null)
                return NotFound();

            report.Status = status;
            report.ReviewMessage = message;
            report.ReviewedAt = DateTime.Now;

            _context.SaveChanges();

            return RedirectToAction("Index");
        }

        [HttpGet]
        public IActionResult Details(int id)
        {
            var report = _context.ReportStore.FirstOrDefault(r => r.ReportID == id);
            if (report == null)
                return NotFound();

            return View(report);
        }
    }
}
