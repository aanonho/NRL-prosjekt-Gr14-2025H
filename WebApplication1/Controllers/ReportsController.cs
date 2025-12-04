// Denne filen lar brukere liste, filtrere og administrere rapporter etter rolle.
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using WebApplication1.Models;
using WebApplication1.DataInfrastructure;
using System.Threading.Tasks;

namespace WebApplication1.Controllers
{
    [Authorize]
    public class ReportsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ReportsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Henter oversikt over rapporter med filtrering på status, sortering og organisasjon.
        [HttpGet]
        public async Task<IActionResult> Index(string status = "all", string sort = "date_desc", string organization = "")
        {
            var isPilot = User.IsInRole("Pilot");
            var isRegistrar = User.IsInRole("Registrar");

            var currentEmail = User?.Identity?.Name;
            if (string.IsNullOrWhiteSpace(currentEmail) && TempData.ContainsKey("CurrentUserEmail"))
            {
                currentEmail = TempData.Peek("CurrentUserEmail") as string;
            }

            if (!string.IsNullOrWhiteSpace(currentEmail))
            {
                currentEmail = currentEmail.Trim();
            }

            var query = _context.ReportItems
                .Include(r => r.ReportObstacle)
                .Include(r => r.OrganizationRef)
                .AsQueryable();

            if (isPilot)
            {
                if (!string.IsNullOrWhiteSpace(currentEmail))
                {
                    var normalizedEmail = currentEmail.ToLowerInvariant();
                    query = query.Where(r => r.SubmittedByEmail != null && r.SubmittedByEmail.ToLower() == normalizedEmail);
                }
                else
                {
                    query = query.Where(r => false);
                }
            }

            var accessibleReports = await query.ToListAsync();
            var filtered = accessibleReports.AsEnumerable();

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

            if (!string.IsNullOrWhiteSpace(organization))
            {
                var org = organization.Trim();
                filtered = filtered.Where(r =>
                    (r.Organization ?? r.OrganizationRef?.Name ?? string.Empty)
                        .Contains(org, StringComparison.OrdinalIgnoreCase));
            }

            var sortNormalized = (sort ?? "date_desc").Trim().ToLowerInvariant();
            filtered = sortNormalized switch
            {
                "date_asc" => filtered.OrderBy(r => r.CreatedAt),
                _ => filtered.OrderByDescending(r => r.CreatedAt)
            };

            var filteredList = filtered.ToList();

            var vm = new ReportsIndexViewModel
            {
                Reports = filteredList,
                Status = statusNormalized,
                Sort = sortNormalized,
                Organization = organization ?? string.Empty,
                TotalCount = accessibleReports.Count,
                FilteredCount = filteredList.Count,
                IsPilotView = isPilot && !isRegistrar,
                IsRegistrarView = isRegistrar,
                CurrentUserEmail = currentEmail
            };

            return View(vm);
        }

        // Enkel statusvisning for alle rapporter uten rollefiltrering.
        [HttpGet]
        public IActionResult ReportStatus(string status = "all")
        {
            var allReports = _context.ReportItems.ToList();

            var filtered = status.ToLower() switch
            {
                "pending" => allReports.Where(r => r.Status == "Pending"),
                "approved" => allReports.Where(r => r.Status == "Approved"),
                "rejected" => allReports.Where(r => r.Status == "Rejected"),
                _ => allReports
            };

            return View(filtered.ToList());
        }

        // Registrar oppdaterer status og legger igjen tilbakemelding på en innsendt rapport.
        [Authorize(Roles = "Registrar")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult UpdateStatus(int id, string status, string message)
        {
            var report = _context.ReportItems.FirstOrDefault(r => r.ReportID == id && !r.IsDraft);
            if (report == null)
            {
                TempData["ErrorMessage"] = "Report not found.";
                return RedirectToAction("Index");
            }

            report.Status = status;
            report.ReviewMessage = message;
            report.ReviewedAt = DateTime.Now;

            _context.SaveChanges();

            TempData["SuccessMessage"] = "Report status updated.";

            return RedirectToAction("Index");
        }

        // Registrar kan slette en ikke-utkast rapport dersom den ikke skal beholdes.
        [Authorize(Roles = "Registrar")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(int id)
        {
            var report = _context.ReportItems
                .FirstOrDefault(r => r.ReportID == id && !r.IsDraft);

            if (report == null)
            {
                return NotFound();
            }

            _context.ReportItems.Remove(report);
            _context.SaveChanges();

            TempData["SuccessMessage"] = "Report deleted.";

            return RedirectToAction("Index");
        }

        // Viser detaljer for en spesifikk rapport og sikrer at piloter kun ser egne innsendelser.
        [HttpGet]
        public IActionResult Details(int id)
        {
            var isRegistrar = User.IsInRole("Registrar");
            var isPilot = User.IsInRole("Pilot");

            var report = _context.ReportItems
                                  .Include(r => r.ReportObstacle)
                                  .Include(r => r.OrganizationRef)
                                  .FirstOrDefault(r => r.ReportID == id);
            if (report == null)
                return NotFound();

            if (!isRegistrar && isPilot)
            {
                var currentEmail = User?.Identity?.Name;
                if (string.IsNullOrWhiteSpace(currentEmail) && TempData.ContainsKey("CurrentUserEmail"))
                {
                    currentEmail = TempData.Peek("CurrentUserEmail") as string;
                }

                var normalizedEmail = (currentEmail ?? string.Empty).Trim().ToLowerInvariant();
                var submittedEmail = (report.SubmittedByEmail ?? string.Empty).Trim().ToLowerInvariant();

                if (normalizedEmail != submittedEmail)
                {
                    return Forbid();
                }
            }

            return View(report);
        }
    }
}