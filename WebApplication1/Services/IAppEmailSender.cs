namespace WebApplication1.Services
{
    public interface IAppEmailSender
    {
        Task SendEmailAsync(string to, string subject, string htmlBody);
    }
}