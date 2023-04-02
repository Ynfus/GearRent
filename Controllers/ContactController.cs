using GearRent.Models;
using Microsoft.AspNetCore.Mvc;
using System.Net.Mail;

namespace GearRent.Controllers
{
    public class ContactController : Controller
    {
        public IActionResult Index()
        {
            if (User != null && User.Identity.IsAuthenticated)
            {
                var userEmail = User.Identity.Name;
                var contactViewModel = new ContactViewModel() { Email = userEmail };
                return View(contactViewModel);
            }
            else
            {
                return View(new ContactViewModel());
            }
        }
        [HttpPost]
        public ActionResult Index(ContactViewModel contactViewModel)
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

            return RedirectToAction("Index");
        }

    }
}
