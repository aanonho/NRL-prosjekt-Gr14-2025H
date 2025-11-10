using Microsoft.Extensions.Logging;

namespace WebApplication1.Services
{
    // DEV helper: prints email content to logs and exposes it for the DEV preview box.
    public class DevEmailSender : IEmailSender
    {
        private readonly ILogger<DevEmailSender> _logger;
        public static string? LastMessageHtml { get; private set; }

        public DevEmailSender(ILogger<DevEmailSender> logger) => _logger = logger;

        public Task SendEmailAsync(string toEmail, string subject, string htmlBody)
        {
            LastMessageHtml = $"<strong>To:</strong> {toEmail}<br/><strong>Subject:</strong> {subject}<br/>{htmlBody}";
            _logger.LogInformation("DEV EMAIL to {To}: {Subject}\n{Body}", toEmail, subject, htmlBody);
            return Task.CompletedTask;
        }
    }
}