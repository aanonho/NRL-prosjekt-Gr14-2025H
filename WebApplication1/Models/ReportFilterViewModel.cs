using System;
using System.Collections.Generic;

namespace WebApplication1.Models
{
    public class ReportFilterViewModel
    {
        public string? Type { get; set; }
        public string? Organization { get; set; }
        public string? ReporterName { get; set; }
        public string? Status { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public string? Sort { get; set; } = "date_desc"; // Default sort

        public List<ReportItem> Reports { get; set; } = new();
    }
}
