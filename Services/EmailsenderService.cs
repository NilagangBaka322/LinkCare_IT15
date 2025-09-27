using Microsoft.AspNetCore.Identity.UI.Services;
using System.Threading.Tasks;

namespace LinkCare_IT15.Services
{
    public class EmailSender : IEmailSender
    {
        public Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            // For now, do nothing
            return Task.CompletedTask;
        }
    }
}
