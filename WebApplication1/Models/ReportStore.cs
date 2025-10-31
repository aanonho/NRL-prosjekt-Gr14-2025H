using System;
using System.Collections.Generic;
using System.Linq;

namespace WebApplication1.Models
{
    // Extremely simple in-memory store so the list can show items
    // that were submitted during the time the app is running.
    // No database — just a static List and a lock for safety.
    public static class ReportStore
    {
        private static readonly List<ReportItem> _items = new List<ReportItem>();
        private static readonly object _lock = new object();

        public static void Add(ReportItem item)
        {
            lock (_lock)
            {
                _items.Add(item);
            }
        }

        public static List<ReportItem> GetAll()
        {
            lock (_lock)
            {
                // Return a copy so callers cannot modify internal list by mistake
                return _items.ToList();
            }
        }


        // Gathering reports to a specific user
        public static List<ReportItem> GetReportsByUser(string email)
        {
            lock (_lock)
            {
                return _items.Where(item => item.SubmittedByEmail == email).ToList();
            }
        }

        // Update status, add message (Registrar)
        public static void UpdateStatus(int id, string newStatus, string? message = null)
        {
            lock (_lock)
            {
                var report = _items.FirstOrDefault(r => r.ReportID == id);
                if (report != null)
                {
                    // Update only what registrar is allowed to change
                    report.Status = newStatus;
                    report.IsDraft = false;
                    report.ReviewedAt = DateTime.Now;
                    report.ReviewMessage = message ?? "";

                    // Force-update stored record safely
                    var index = _items.FindIndex(r => r.ReportID == id);
                    if (index >= 0)
                    {
                        _items[index] = report;
                    }
                }
            }
        }


        public static void Update(int id, ReportItem updatedReport)
        {
            lock (_lock)
            {
                var existing = _items.FirstOrDefault(r => r.ReportID == id);
                if (existing != null)
                {
                    var index = _items.IndexOf(existing);
                    _items[index] = updatedReport;
                }
            }
        }



    }
}
