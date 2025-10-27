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

        // Update status, add message(Registrar)
        public static void UpdateStatus(Guid id, string status, string? message = null)
        {
            lock (_lock)
            {
                var report = _items.FirstOrDefault(i => i.Id == id);
                if (report != null)
                {
                    report.Status = status;
                    report.ReviewMessage = message ?? "";
                }
            }
        }
    }
}
