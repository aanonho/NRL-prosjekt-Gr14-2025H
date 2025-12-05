// Configuration model for SMTP settings retrieved from appsettings.
namespace WebApplication1.Models
{
    // Bundles all fields required to contact the SMTP server.
    public class EmailOptions
    {
        // Server address and port that the SmtpClient should use.
        public string SmtpServer { get; set; }
        public int SmtpPort { get; set; }
        // Sender information and credentials to authenticate with the server.
        public string SenderEmail { get; set; }
        public string SenderName { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        // Toggle to secure transport with SSL when the server requires it.
        public bool UseSSL { get; set; }
    }
}
