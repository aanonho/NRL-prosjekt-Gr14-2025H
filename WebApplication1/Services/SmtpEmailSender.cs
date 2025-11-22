using System;
using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using WebApplication1.Models;

namespace WebApplication1.Services
{
    public class SmtpEmailSender : IEmailSender
    {
        private readonly EmailOptions _options;
        private readonly ILogger<SmtpEmailSender> _logger;

        public SmtpEmailSender(IOptions<EmailOptions> options, ILogger<SmtpEmailSender> logger)
        {
            _options = options.Value;
            _logger = logger;
        }

        // SMTP-based email sender for password reset notifications
        // Uses configuration from appsettings.json
        public async Task SendPasswordResetAsync(string recipientEmail, string recipientName, string resetLink)
        {
            ValidateConfiguration();

            using var client = new SmtpClient(_options.SmtpServer!, _options.SmtpPort)
            {
                EnableSsl = _options.UseSSL,
                Credentials = new NetworkCredential(_options.Username, _options.Password),
                DeliveryMethod = SmtpDeliveryMethod.Network
            };

            using var message = new MailMessage
            {
                From = new MailAddress(_options.SenderEmail!, "NRL Support"),
                Subject = "Reset your NRL account password",
                Body = $"Hello {recipientName},\n\nWe received a request to reset your password. Click the secure link below to choose a new password:\n{resetLink}\n\nIf you did not request this, you can safely ignore this email. The link will expire soon for your security.",
                IsBodyHtml = false
            };

            message.To.Add(new MailAddress(recipientEmail));

            try
            {
                await client.SendMailAsync(message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send password reset email to {RecipientEmail}", recipientEmail);
                throw;
            }
        }

        private void ValidateConfiguration()
        {
            if (string.IsNullOrWhiteSpace(_options.SmtpServer) ||
                _options.SmtpPort <= 0 ||
                string.IsNullOrWhiteSpace(_options.SenderEmail))
            {
                throw new InvalidOperationException("SMTP email settings are not fully configured.");
            }
        }

    }
}
