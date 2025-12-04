// Viser feildata til feilsiden slik at brukeren kan referere til RequestId.
namespace WebApplication1.Models
{
    public class ErrorViewModel
    {
        // RequestId brukes til å slå opp i logger eller diagnostikk.
        public string? RequestId { get; set; }

        // Viser feltet i viewet kun når RequestId faktisk finnes.
        public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
    }
}