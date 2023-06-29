using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Options;
using System.Net.Mail;
using System.Net;

namespace GearRent.Services
{

    public class EmailSender : IEmailSender
    {
        private readonly ILogger<EmailSender> _logger;

        public EmailSender(ILogger<EmailSender> logger)
        {
            _logger = logger;
        }

        public async Task SendEmailAsync(string toEmail, string subject, string message)
        {
            try
            {
                using (SmtpClient smtpClient = new SmtpClient("localhost", 2525))
                {
                    //smtpClient.EnableSsl = true;
                    smtpClient.DeliveryMethod = SmtpDeliveryMethod.Network;
                    smtpClient.UseDefaultCredentials = true;

                    MailMessage mailMessage = new MailMessage();
                    mailMessage.From = new MailAddress("sender@example.com");
                    mailMessage.To.Add(new MailAddress(toEmail));
                    mailMessage.Subject = subject;
                    mailMessage.Body = message;

                    await smtpClient.SendMailAsync(mailMessage);

                    _logger.LogInformation($"Email sent successfully to {toEmail}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to send email to {toEmail}");
                throw;
            }
        }
    }
}
