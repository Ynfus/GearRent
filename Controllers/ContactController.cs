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
            _emailSender.SendEmailAsync(contactViewModel.Email, "Wiadomość z formularza kontaktowego " + DateTime.Now.ToString(), contactViewModel.Message + "\n" + contactViewModel.Name);
            TempData["messageSent"] = true;

            return RedirectToAction("Index");
        }
        public IActionResult PrivacyPolicy()
        {
            return View();
        }
        public IActionResult DownloadFile(string fileName)
        {
            string filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot\\pdf", fileName);
            if (!System.IO.File.Exists(filePath))
            {
                return NotFound();
            }
            string fileExtension = Path.GetExtension(filePath);
            string contentType = "application/pdf";
            return File(System.IO.File.OpenRead(filePath), contentType, fileName);
        }
        public IActionResult OWNP()
        {
            return View();
        }
        public IActionResult Complaint()
        {
            return View();
        }
        [HttpPost]
        public ActionResult Complaint(ComplaintViewModel complaintViewModel)
         {
            _emailSender.SendEmailAsync(complaintViewModel.Email, "Wiadomość z formularza reklamacyjnego zamówienia nr: " + complaintViewModel.ReservationNumber + "  " + DateTime.Now.ToString(),
                complaintViewModel.ComplaintDescription + "\n\nZłozone przez: " + complaintViewModel.FullName + "\nNumer kontaktowy: " + complaintViewModel.Phone);
            TempData["messageSent"] = true;

            return RedirectToAction("Complaint");
        }
    }
}
