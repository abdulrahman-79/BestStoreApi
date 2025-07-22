using SendGrid;
using SendGrid.Helpers.Mail;
using System.Net.Mail;

namespace BestStoreApi.Services
{
    public class EmailSender
    {
        private readonly string apiKey;
        private readonly string fromEmail;
        private readonly string senderName;
        public EmailSender(IConfiguration configuration)
        {
            this.apiKey = configuration["EmailSender:ApiKey"]!;
            this.fromEmail = configuration["EmailSender:FromEmail"]!;
            this.senderName = configuration["EmailSender:SenderName"]!;
        }
        public  async Task SendEmail(string subject, string toEmail, string userName, string message)
        {
            /* var options = new SendGridClientOptions
            {
                ApiKey = apiKey
            };
            options.SetDataResidency("eu"); 
            var client = new SendGridClient(options); */
            // uncomment the above 6 lines if you are sending mail using a regional EU subuser
            // and remove the client declaration just below
            var client = new SendGridClient(apiKey);
            var from = new EmailAddress(fromEmail, senderName);
            var to = new EmailAddress(toEmail, userName);
            var plainTextContent = message;
            var htmlContent = "";
            var msg = MailHelper.CreateSingleEmail(from, to, subject, plainTextContent, htmlContent);
            var response = await client.SendEmailAsync(msg);
        }
    }
}
