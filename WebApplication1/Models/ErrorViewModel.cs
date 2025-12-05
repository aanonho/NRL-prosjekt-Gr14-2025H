// Displays error data on the error page so the user can refer to the RequestId.
namespace WebApplication1.Models
{
    public class ErrorViewModel
    {
        // RequestId used for looking up in logs or diagnostics.
        public string? RequestId { get; set; }

        // Display the fields in the view only when RequestId actually exists.
        public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
    }
}
