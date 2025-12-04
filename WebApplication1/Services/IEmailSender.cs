// Kontrakt for e-postutsendelser slik at vi kan bytte implementasjon ved behov
using System.Threading.Tasks;

namespace WebApplication1.Services
{
    // Interface holder bare det som trengs for passordreset, ikke en full e-postklient
    public interface IEmailSender
    {
        // Sender en enkel tilbakestillings-epost med navn og link til mottakeren
        Task SendPasswordResetAsync(string recipientEmail, string recipientName, string resetLink);
    }
}