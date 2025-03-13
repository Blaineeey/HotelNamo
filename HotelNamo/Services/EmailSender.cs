using Microsoft.AspNetCore.Identity.UI.Services;
using SendGrid;
using SendGrid.Helpers.Mail;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using System.Net.Mail;

namespace HotelNamo.Services
{
    public class EmailSender : IEmailSender
    {
        private readonly AuthMessageSenderOptions _options;

        public EmailSender(IOptions<AuthMessageSenderOptions> optionsAccessor)
        {
            _options = optionsAccessor.Value;
        }

        public async Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            var client = new SendGridClient(_options.SendGridKey);
            var msg = new SendGridMessage()
            {
                From = new EmailAddress("your-email@example.com", "HotelNamo"),
                Subject = subject,
                HtmlContent = htmlMessage
            };

            msg.AddTo(new EmailAddress(email));
            await client.SendEmailAsync(msg);
        }
    }
} 