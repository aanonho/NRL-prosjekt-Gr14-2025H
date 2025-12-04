// Konfigurasjonsmodell for SMTP-innstillinger hentet fra appsettings
namespace WebApplication1.Models
{
    // Bundles alle feltene som trengs for å kontakte SMTP-serveren
    public class EmailOptions
    {
        public string SmtpServer { get; set; }
        public int SmtpPort { get; set; }
        public string SenderEmail { get; set; }
        public string SenderName { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public bool UseSSL { get; set; }
    }
}