// Contract for email sending so we can swap implementations as needed.
using System.Threading.Tasks;

namespace WebApplication1.Services
{
    // The interface only includes what is needed for password reset, not a full email client.
    public interface IEmailSender
    {
        // Sends a simple password reset email with name and link to the recipient.
        Task SendPasswordResetAsync(string recipientEmail, string recipientName, string resetLink);
    }
}
