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
                    string fullMessage = "Witaj!\n\n";
                    fullMessage += message;
                    fullMessage += "\n\nDziękujemy za korzystanie z naszych usług!\n";
                    fullMessage += "Pozdrawiamy,\nZespół GearRent";
                    fullMessage += "\n\n\nDane kontaktowe:\n";
                    fullMessage += "ul. Półwiejska 42, 61-888 Poznań\n";
                    fullMessage += "+48 123 456 789\n";
                    fullMessage += "kontakt@gearrent.pl\n\n";
                    fullMessage += "Godziny otwarcia:\n";
                    fullMessage += "Poniedziałek - piątek: 8:00 - 20:00\n";
                    fullMessage += "Sobota: 9:00 - 18:00\n";
                    fullMessage += "Niedziela: nieczynne";
                    MailMessage mailMessage = new MailMessage();
                    mailMessage.From = new MailAddress("sender@example.com");
                    mailMessage.To.Add(new MailAddress(toEmail));
                    mailMessage.Subject = subject;
                    mailMessage.Body = fullMessage;

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
