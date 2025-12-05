// The Wviewmodel collects report data and filter selections for the registrar/pilot interface overview.
using System.Collections.Generic;
using System.Linq;

namespace WebApplication1.Models
{
    // File purpose: ViewModel that binds together the report list with filtering and sorting selections for the overview page.
    public class ReportsIndexViewModel
    {
        // Aggregated list of reports displayed in the table after filters are applied.
        public List<ReportItem> Reports { get; set; } = new List<ReportItem>();

        /*
         * Curren filter and sorting selections that are populated from the request and sent back to the view
         * so that the fields retain the values the user selected (status, sorting, and free text on organization).
         */
        public string Status { get; set; } = "all";          // all | Pending | Approved | Rejected
        public string Sort { get; set; } = "date_desc";      // date_desc | date_asc
        public string Organization { get; set; } = "";       // free text contains filter

        // Total counts and filtered count allow displaying counts to the user.
        public int TotalCount { get; set; }
        public int FilteredCount { get; set; }

        // Devrived counts for drafts and submitted reports for small status indicators in the UI.
        public int DraftCount => Reports.Count(r => r.IsDraft);
        public int SubmittedCount => Reports.Count(r => !r.IsDraft);

        // Flag indicating whuch role-based perspective the view is showing, along with the email of the logged-in user.
        public bool IsPilotView { get; set; }
        public bool IsRegistrarView { get; set; }
        public string? CurrentUserEmail { get; set; }
    }
}
