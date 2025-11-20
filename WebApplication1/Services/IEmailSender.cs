using System.Threading.Tasks;

namespace WebApplication1.Services
{
    public interface IEmailSender
    {
        Task SendPasswordResetAsync(string recipientEmail, string recipientName, string resetLink);
    }
}
