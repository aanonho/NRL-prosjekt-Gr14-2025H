using System.Collections.Generic;
using System.Linq;

namespace WebApplication1.Models
{
    // Filformål: ViewModel som binder sammen rapportlisten med filtrerings- og sorteringsvalg til oversiktssiden.
    public class ReportsIndexViewModel
    {
        // Samlet liste over rapporter som vises i tabellen etter at filtrene er anvendt.
        public List<ReportItem> Reports { get; set; } = new List<ReportItem>();

        /*
         * Gjeldende filter- og sorteringsvalg som fylles fra forespørselen og sendes tilbake til viewet
         * slik at feltene beholder verdiene bruker valgte (status, sortering og fritekst på organisasjon).
         */
        public string Status { get; set; } = "all";          // all | Pending | Approved | Rejected
        public string Sort { get; set; } = "date_desc";      // date_desc | date_asc
        public string Organization { get; set; } = "";       // free text contains filter

        // Totalantall og antall etter filter gjør det mulig å vise tellinger for bruker.
        public int TotalCount { get; set; }
        public int FilteredCount { get; set; }

        // Avledede tellinger for utkast og innsendte rapporter for små statusindikatorer i UI.
        public int DraftCount => Reports.Count(r => r.IsDraft);
        public int SubmittedCount => Reports.Count(r => !r.IsDraft);

        // Flaggsom markerer hvilket rollebasert perspektiv siden viser, samt e-posten til innlogget bruker.
        public bool IsPilotView { get; set; }
        public bool IsRegistrarView { get; set; }
        public string? CurrentUserEmail { get; set; }
    }
}