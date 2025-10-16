using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using WebApplication1.Models;

namespace WebApplication1.Controllers
{
    public class ReportsController : Controller
    {
        [HttpGet]
        public IActionResult Index(string status = "all", string sort = "date_desc", string organization = "")
        {
            var all = ReportStore.GetAll();

            var filtered = all.AsEnumerable();

            // Filter by status if one of the known values is chosen
            var statusNormalized = (status ?? "all").Trim();
            if (!string.IsNullOrWhiteSpace(statusNormalized) &&
                !statusNormalized.Equals("all", StringComparison.OrdinalIgnoreCase))
            {
                if (statusNormalized.Equals("draft", StringComparison.OrdinalIgnoreCase))
                {
                    filtered = filtered.Where(r => r.IsDraft);
                }
                else if (statusNormalized.Equals("submitted", StringComparison.OrdinalIgnoreCase))
                {
                    filtered = filtered.Where(r => !r.IsDraft);
                }
                else
                {
                    filtered = filtered.Where(r =>
                        string.Equals(r.Status, statusNormalized, StringComparison.OrdinalIgnoreCase));
                }
            }

            // Filter by organization (contains, case-insensitive)
            if (!string.IsNullOrWhiteSpace(organization))
            {
                var org = organization.Trim();
                filtered = filtered.Where(r =>
                    (r.Organization ?? "").Contains(org, StringComparison.OrdinalIgnoreCase));
            }

            // Sorting
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
    }
}
