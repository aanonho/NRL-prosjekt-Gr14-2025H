// Konfigurasjonsmodell for SMTP-innstillinger hentet fra appsettings
namespace WebApplication1.Models
{
    // Bundles alle feltene som trengs for å kontakte SMTP-serveren
    public class EmailOptions
    {
        // Serveradresse og port som SmtpClient skal bruke
        public string SmtpServer { get; set; }
        public int SmtpPort { get; set; }
        // Avsenderinfo og legitimasjon for å autentisere mot serveren
        public string SenderEmail { get; set; }
        public string SenderName { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        // Toggle for å sikre transport med SSL når serveren krever det
        public bool UseSSL { get; set; }
    }
}
