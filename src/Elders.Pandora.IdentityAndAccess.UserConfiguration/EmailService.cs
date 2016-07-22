using Microsoft.AspNet.Identity;
using System.Threading.Tasks;

namespace Elders.Pandora.IdentityAndAccess.UserConfiguration
{
    public class EmailService : IIdentityMessageService
    {
        public Task SendAsync(IdentityMessage message)
        {
            // Create the mail message
            var mailMessage = new System.Net.Mail.MailMessage(
                "web@Pandora.org",
                message.Destination,
                message.Subject,
                message.Body
                );

            // Send the message
            var client = new System.Net.Mail.SmtpClient();
            client.SendAsync(mailMessage, null);

            return Task.FromResult(true);
        }
    }
}