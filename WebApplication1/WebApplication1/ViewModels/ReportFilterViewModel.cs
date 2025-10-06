using System;
using System.Collections.Generic;
using WebApplication1.Models;   

namespace WebApplication1.ViewModels
{
    public class ReportFilterViewModel
    {
        public string? Type { get; set; }
        public string? Organization { get; set; }
        public string? ReporterName { get; set; }
        public string? Status { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }

        public List<Report>? Reports { get; set; }
    }
}
