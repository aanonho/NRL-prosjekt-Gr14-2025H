using System.Collections.Generic;

namespace WebApplication1.Models
{
    // ViewModel for the Raports page so the view has both filters and the list.
    public class ReportsIndexViewModel
    {
        public List<ReportItem> Reports { get; set; } = new List<ReportItem>();

        // Current filter/sort values (echoed back into the form)
        public string Status { get; set; } = "all";          // all | Pending | Approved | Rejected
        public string Sort { get; set; } = "date_desc";      // date_desc | date_asc
        public string Organization { get; set; } = "";       // free text contains filter

        public int TotalCount { get; set; }
        public int FilteredCount { get; set; }
    }
}
