using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using GearRent.Data;
using GearRent.Models;
using System.Security.Claims;
using Microsoft.AspNetCore.Identity.UI.Services;
using GearRent.Services;
using Hangfire;

namespace GearRent.Controllers
{
    public class ReservationsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IEmailSender _emailSender;
        private readonly IUserService _userService;
        private readonly ICarService _carService;
        private readonly IReservationService _reservationService;
        private readonly IBackgroundJobClient _backgroundJobClient;

        public ReservationsController(ApplicationDbContext context, IEmailSender emailSender,
            IUserService userService, ICarService carService, IReservationService reservationService, IBackgroundJobClient backgroundJobClient)
        {
            _context = context;
            _emailSender = emailSender;
            _userService = userService;
            _carService = carService;
            _reservationService = reservationService;
            _backgroundJobClient = backgroundJobClient;
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
            string userId = HttpContext.User.Identity.IsAuthenticated ? HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value : null;
            if (userId == null)
            {
                return Redirect("/Identity/Account/Login");
            }
            int carId = HttpContext.Session.GetInt32("carId") ?? 0;
            string startDate = HttpContext.Session.GetString("startDate");
            string endDate = HttpContext.Session.GetString("endDate");
            ViewData["CarId"] = carId;
            ViewData["StartDate"] = startDate;
            ViewData["EndDate"] = endDate;
            ViewData["UserId"] = userId;
            HttpContext.Session.Remove("carId");
            HttpContext.Session.Remove("startDate");
            HttpContext.Session.Remove("endDate");

            return View();
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
        public async Task<IActionResult> Create([Bind("Id,UserId,CarId,StartDate,EndDate,Status")] Reservation reservation, bool unpaid)
        {
            Car car = await _carService.GetCarAsync(reservation.CarId);
            if (car == null)
            {
                return NotFound();
            }
            reservation.Car = car;
            var value = car.Price * (reservation.EndDate - reservation.StartDate).Days;
            reservation.ReservationValue = value;
            _context.Add(reservation);
            await _context.SaveChangesAsync();
            if (unpaid)
            {
                Console.WriteLine(reservation.Id+"000000");
                return RedirectToAction("ThanksEmailUnpaid", "Reservations",new { id = reservation.Id } );

            }
            else
            {
                return RedirectToAction(nameof(Checkout), new { id=reservation.Id });

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
            try
            {
                Car car = await _carService.GetCarAsync(reservation.CarId);
                var email = await _context.Users
                    .Where(u => u.Id == reservation.UserId)
                    .Select(u => u.Email)
                    .FirstOrDefaultAsync();
                var subject = $"Potwierdzenie rezerwacji {reservation.Id}";
                var body = $"Twoja rezerwacja na {car.Make} {car.Model} została pomyślnie złożona.\nNieopłacona kwota zamówienia to: {reservation.ReservationValue}";
                await _emailSender.SendEmailAsync(email, subject, body);
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
    }
}
