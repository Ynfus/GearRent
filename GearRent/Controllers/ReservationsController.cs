using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using GearRent.Data;
using GearRent.Models;
using System.Security.Claims;
using Microsoft.AspNetCore.Identity.UI.Services;
using GearRent.Services;
using Hangfire;
using DinkToPdf;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using DinkToPdf.Contracts;

namespace GearRent.Controllers
{
    public class ReservationsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ICustomEmailSender _emailSender;
        private readonly IUserService _userService;
        private readonly ICarService _carService;
        private readonly IReservationService _reservationService;
        private readonly IBackgroundJobClient _backgroundJobClient;
        private readonly IConverter _pdfConverter;

        public ReservationsController(ApplicationDbContext context, ICustomEmailSender emailSender,
            IUserService userService, ICarService carService, IReservationService reservationService, IConverter pdfConverter, IBackgroundJobClient backgroundJobClient)
        {
            _context = context;
            _emailSender = emailSender;
            _userService = userService;
            _carService = carService;
            _reservationService = reservationService;
            _backgroundJobClient = backgroundJobClient;
            _pdfConverter = pdfConverter;
        }

        public async Task<IActionResult> Index()
        {
            var applicationDbContext = await _reservationService.GetAllReservationsAsync();
            return View();
        }
        public async Task<ActionResult> Checkout(string payment_intent, string payment_intent_client_secret, string redirect_status, int id)
        {
            ViewBag.MyValue = id;
            return View();
        }

        public async Task<IActionResult> Details(int id)
        {

            var reservation = await _reservationService.GetReservationByIdAsyncInclude(id);
            if (reservation == null)
            {
                return NotFound();
            }

            return View(reservation);
        }
        [HttpGet]
        public IActionResult Create()
        {
            if (!User.Identity.IsAuthenticated)
            {
                return Redirect("/Identity/Account/Login");
            }
            if (string.IsNullOrEmpty(HttpContext.Session.GetString("startDate")) ||
        string.IsNullOrEmpty(HttpContext.Session.GetString("endDate")) || (HttpContext.Session.GetInt32("carId"))==null)
            {
                return RedirectToAction("Index", "Home");
            }
            string userId = User.FindFirst(ClaimTypes.NameIdentifier).Value;
            int carId = HttpContext.Session.GetInt32("carId") ?? 0;
            string startDate = HttpContext.Session.GetString("startDate");
            string endDate = HttpContext.Session.GetString("endDate");

            //to zrobię za pomocą ajaxa
            //var billingInfoOptions = _context.BillingInfos
            //    .Where(b => b.UserId == userId)
            //    .ToList();

            var viewModel = new CreateReservationViewModel
            {
                CarId = carId,
                StartDate = startDate,
                EndDate = endDate,
                UserId = userId,
                //BillingInfoOptions = billingInfoOptions
            };

            HttpContext.Session.Remove("carId");
            HttpContext.Session.Remove("startDate");
            HttpContext.Session.Remove("endDate");

            return View(viewModel);
        }
        [HttpPost]
        public IActionResult SetSessionData(int carId, string startDate, string endDate)
        {
            HttpContext.Session.SetInt32("carId", carId);
            HttpContext.Session.SetString("startDate", startDate);
            HttpContext.Session.SetString("endDate", endDate);

            return Ok();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,UserId,CarId,StartDate,EndDate,Status")] Reservation reservation, int SelectedBillingInfoId, bool unpaid)
        {
            Car car = await _carService.GetCarAsync(reservation.CarId);
            if (car == null)
            {
                return NotFound();
            }
            reservation.Car = car;
            reservation.BillingInfoId = SelectedBillingInfoId;
            var value = car.Price * (reservation.EndDate - reservation.StartDate).Days;
            reservation.ReservationValue = value;
            _context.Add(reservation);
            await _context.SaveChangesAsync();
            if (unpaid)
            {
                return RedirectToAction("ThanksEmailUnpaid", "Reservations", new { id = reservation.Id });

            }
            else
            {
                return RedirectToAction(nameof(Checkout), new { id = reservation.Id });

            }
        }
        public async Task<IActionResult> ThanksEmailUnpaid(int id)
        {
            Console.WriteLine(id + "1111");
            string userId = HttpContext.User.Identity.IsAuthenticated ? HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value : null;
            var reservation = await _context.Reservations.FindAsync(id);
            bool emailSent = true;
            var cancellationDate = reservation.StartDate.AddDays(-3);
            _backgroundJobClient.Schedule<ReservationService>(
                x => x.CancelUnpaidReservationsAndSendEmailsAsync(reservation.Id),
                cancellationDate
            );
            try
            {
                Car car = await _carService.GetCarAsync(reservation.CarId);
                var email = await _context.Users
                    .Where(u => u.Id == reservation.UserId)
                    .Select(u => u.Email)
                    .FirstOrDefaultAsync();
                var subject = $"Potwierdzenie rezerwacji {reservation.Id}";
                var body = $"Twoja rezerwacja na {car.Make} {car.Model} została pomyślnie złożona.\nKwota zamówienia do opłacenia to: {reservation.ReservationValue}, pmaiętaj, aby opłacić zamówienie do 3 dni przed startem rezerwacji.";
                await _emailSender.SendEmailAsync(email, subject, body);
            }
            catch (Exception)
            {
                emailSent = false;
            }
            return RedirectToAction("Thanks", "Reservations", new { id = id, emailSent });
        }

        public async Task<IActionResult> Edit(int id)
        {
            if (id == null || _context.Reservations == null)
            {
                return NotFound();
            }

            var reservation = await _reservationService.GetReservationByIdAsync(id);
            if (reservation == null)
            {
                return NotFound();
            }
            ViewData["CarId"] = new SelectList(_context.Cars, "Id", "Id", reservation.CarId);
            ViewData["UserId"] = new SelectList(_context.Set<ApplicationUser>(), "Id", "Id", reservation.UserId);
            return View(reservation);
        }



        public async Task<IActionResult> ThanksEmail(int id)
        {
            string userId = HttpContext.User.Identity.IsAuthenticated ? HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value : null;
            var reservation = await _context.Reservations.FindAsync(id);
            if (reservation.UserId != userId)
            {
                return NotFound();
            }
            reservation.Status = ReservationStatus.Approved;
            await _context.SaveChangesAsync();
            bool emailSent = true;
            var pdf = await BillCreate(reservation.Id);

            try
            {
                Car car = await _carService.GetCarAsync(reservation.CarId);
                var email = await _context.Users
                    .Where(u => u.Id == reservation.UserId)
                    .Select(u => u.Email)
                    .FirstOrDefaultAsync();
                var subject = $"Potwierdzenie rezerwacji {reservation.Id}";
                var body = $"Twoja rezerwacja na {car.Make} {car.Model} została pomyślnie złożona.\nOpłacona kwota zamówienia to: {reservation.ReservationValue}";
                await _emailSender.SendEmailWithAttachmentAsync(email, subject, body, pdf, $"faktura{reservation.Id}.pdf");
            }
            catch (Exception)
            {
                emailSent = false;
            }
            return RedirectToAction("Thanks", "Reservations", new { id = id, emailSent });
        }

        public async Task<IActionResult> Thanks(int id, bool emailSent)
        {
            Reservation reservation = await _reservationService.GetReservationByIdAsync(id);
            Car car = await _carService.GetCarAsync(reservation.CarId);
            ViewBag.Car = car;
            ViewBag.EmailSent = emailSent;
            return View(reservation);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,UserId,CarId,StartDate,EndDate,Status")] Reservation reservation)
        {
            if (id != reservation.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(reservation);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ReservationExists(reservation.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["CarId"] = new SelectList(_context.Cars, "Id", "Id", reservation.CarId);
            ViewData["UserId"] = new SelectList(_context.Set<ApplicationUser>(), "Id", "Id", reservation.UserId);
            return View(reservation);
        }

        public async Task<IActionResult> Delete(int id)
        {
            if (_context.Reservations == null)
            {
                return Problem("Entity set 'ApplicationDbContext.Reservations'  is null.");
            }
            var reservation = await _reservationService.GetReservationByIdAsync(id);
            if (reservation != null)
            {
                _context.Reservations.Remove(reservation);
            }

            await _context.SaveChangesAsync();
            return StatusCode(200);
        }
        public async Task<IActionResult> CanceledStatus(int id)
        {
            var reservation = await _reservationService.GetReservationByIdAsync(id);
            if (reservation != null)
            {
                reservation.Status = ReservationStatus.Canceled;
            }
            try
            {
                Car car = await _carService.GetCarAsync(reservation.CarId);
                var email = await _context.Users
                    .Where(u => u.Id == reservation.UserId)
                    .Select(u => u.Email)
                    .FirstOrDefaultAsync();
                var subject = $"Anulacja rezerwacji {reservation.Id}";
                var body = $"Twoja rezerwacja na {car.Make} {car.Model} została przez Ciebie anulowana.\nSkontaktuj się z nami mailowo w sprawie zwrotu środków";
                await _emailSender.SendEmailAsync(email, subject, body);
            }
            catch (Exception ex) { }
            await _context.SaveChangesAsync();
            return StatusCode(200);
        }
        public async Task<IActionResult> ManageReservation(int id, string action)
        {
            var reservation = await _reservationService.GetReservationByIdAsync(id);
            string state;

            if (reservation != null)
            {
                switch (action)
                {
                    case "cancel":
                        reservation.Status = ReservationStatus.Canceled;
                        state = "anulowana";
                        break;
                    case "start":
                        reservation.Status = ReservationStatus.InProgress;
                        state = "w trakcie";
                        break;
                    case "finish":
                        reservation.Status = ReservationStatus.Finished;
                        state = "zakończona";
                        break;
                    default:
                        return StatusCode(400);
                }

                try
                {
                    Car car = await _carService.GetCarAsync(reservation.CarId);
                    var email = await _context.Users
                        .Where(u => u.Id == reservation.UserId)
                        .Select(u => u.Email)
                        .FirstOrDefaultAsync();
                    var subject = $"Zmiana statusu rezerwacji {reservation.Id}";
                    var body = $"Status Twojej rezerwacji na {car.Make} {car.Model} został zmieniony na {state}.";
                    await _emailSender.SendEmailAsync(email, subject, body);
                }
                catch (Exception ex)
                {
                }

                await _context.SaveChangesAsync();
                return StatusCode(200);
            }
            else
            {
                return StatusCode(404);
            }
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var reservation = await _reservationService.GetReservationByIdAsync(id);
            if (reservation != null)
            {
                _context.Reservations.Remove(reservation);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ReservationExists(int id)
        {
            return (_context.Reservations?.Any(e => e.Id == id)).GetValueOrDefault();
        }
        public async Task<byte[]> BillCreate(int reservationId)
        {

            var reservation = await _reservationService.GetReservationByIdAsyncInclude(reservationId);
            if (reservation == null)
            {
                return null;
            }
            var billModel = new BillModel
            {
                BillId = reservation.Id,
                Name = reservation.BillingInfo.FirstName + reservation.BillingInfo.LastName,
                CarModel = $"{reservation.Car.Make} {reservation.Car.Model}",
                Date = DateTime.Now.Date.ToString("d"),
                TotalValue = reservation.ReservationValue,
                Days = (int)(reservation.EndDate - reservation.StartDate).TotalDays,
                Address = reservation.BillingInfo.Street + reservation.BillingInfo.City,
                PostalCode = reservation.BillingInfo.PostalCode,
                PhoneNumber = reservation.BillingInfo.PhoneNumber,
            };
            var htmlContent = ViewToString("GenerateBillTemplate", billModel);
            var doc = new HtmlToPdfDocument
            {
                GlobalSettings =
            {
                PaperSize = DinkToPdf.PaperKind.A4,
                Orientation = Orientation.Portrait,
                Margins = new MarginSettings
                {
                    Top = 0,
                    Right = 0,
                    Bottom = 0,
                    Left = 0
                },

            },
                Objects =
            {
                new ObjectSettings
                {
                    HtmlContent = htmlContent,

                }
            }
            };

            var pdfBytes = _pdfConverter.Convert(doc);
            return pdfBytes;
        }

        private string ViewToString(string viewName, object model)
        {
            ViewData.Model = model;
            using (var writer = new StringWriter())
            {
                var viewEngine = HttpContext.RequestServices.GetRequiredService<ICompositeViewEngine>();
                var view = viewEngine.FindView(ControllerContext, viewName, false).View;
                var viewContext = new ViewContext(ControllerContext, view, ViewData, TempData, writer, new HtmlHelperOptions());
                view.RenderAsync(viewContext).GetAwaiter().GetResult();
                return writer.GetStringBuilder().ToString();
            }
        }
    }
}
