using Microsoft.AspNetCore.Mvc;
using System.Net.Mail;

namespace GearRent.Controllers
{
    public class ContactController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
        [HttpPost]
        public ActionResult Index(string name, string email, string phone, string message)
        {
            //var mail = new MailMessage();
            //mail.From = new MailAddress("");
            //mail.To.Add("info@GearRent.pl");
            //mail.Subject = "Wiadomość z formularza kontaktowego";
            //mail.Body = $"Imię i nazwisko: {name}\nAdres e-mail: {email}\nNumer telefonu: {phone}\n\n{message}";

            //var smtp = new SmtpClient();
            //smtp.Host = "";
            //smtp.Port = 587;
            //smtp.Credentials = new System.Net.NetworkCredential("username", "password");
            //smtp.EnableSsl = true;

            //smtp.Send(mail);
            TempData["messageSent"] = true;

            return View();
        }

    }
}
