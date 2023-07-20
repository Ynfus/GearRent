using GearRent.Models;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using System.Data;
using System.Net.Mail;

namespace GearRent.Controllers
{
    public class ContactController : Controller
    {
        private readonly IEmailSender _emailSender;
        public ContactController(IEmailSender emailSender)
        {
            _emailSender = emailSender;
        }

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
            _emailSender.SendEmailAsync(contactViewModel.Email,"Wiadomość z formularza kontaktowego "+ DateTime.Now.ToString() ,contactViewModel.Message+"\n"+contactViewModel.Name);
            TempData["messageSent"] = true;

            return RedirectToAction("Index");
        }

    }
}
