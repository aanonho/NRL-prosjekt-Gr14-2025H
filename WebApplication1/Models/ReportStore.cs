using System;
using MySqlConnector;
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
    }
}
