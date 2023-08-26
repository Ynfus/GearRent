using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using GearRent.Data;
using GearRent.Models;
using System.Security.Claims;
using Microsoft.AspNetCore.Identity.UI.Services;
using GearRent.Services;

namespace GearRent.Controllers
{
    public class ReservationsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IEmailSender _emailSender;
        private readonly IUserService _userService;
        private readonly ICarService _carService;
        private readonly IReservationService _reservationService;

        public ReservationsController(ApplicationDbContext context, IEmailSender emailSender,
            IUserService userService, ICarService carService, IReservationService reservationService)
        {
            _context = context;
            _emailSender = emailSender;
            _userService = userService;
            _carService = carService;
            _reservationService = reservationService;
        }

        public async Task<IActionResult> Index()
        {
            var applicationDbContext = await _reservationService.GetAllReservationsAsync();
            return View();
        }
        public async Task<ActionResult> Checkout(string payment_intent, string payment_intent_client_secret, string redirect_status, Reservation reservation)
        {
            ViewBag.MyValue = reservation.Id;
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

        public IActionResult Create(int carId, string startDate, string endDate)
        {

            string userId = HttpContext.User.Identity.IsAuthenticated ? HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value : null;
            if (userId == null)
            {
                return Redirect("/Identity/Account/Login");
            }
            ViewData["CarId"] = carId;
            ViewData["StartDate"] = startDate;
            ViewData["EndDate"] = endDate;
            ViewData["UserId"] = userId;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,UserId,CarId,StartDate,EndDate,Status")] Reservation reservation)
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
            return RedirectToAction(nameof(Checkout), reservation);
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
            Car car = await _carService.GetCarAsync(reservation.CarId);
            var email = await _context.Users
                .Where(u => u.Id == reservation.UserId)
                .Select(u => u.Email)
                .FirstOrDefaultAsync();
            var subject = $"Potwierdzenie rezerwacji {reservation.Id}";
            var body = $"Twoja rezerwacja na {car.Make} {car.Model} została pomyślnie złożona.\nOpłacona kwota zamówienia to: {reservation.ReservationValue}";
            await _emailSender.SendEmailAsync(email, subject, body);
            await _context.SaveChangesAsync();

            return RedirectToAction("Thanks", "Reservations", new { id = id });
        }
        public async Task<IActionResult> Thanks(int id)
        {
            Reservation reservation = await _reservationService.GetReservationByIdAsync(id);
            Car car = await _carService.GetCarAsync(reservation.CarId);
            ViewBag.Car = car;
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
            if (_context.Reservations == null)
            {
                return Problem("Entity set 'ApplicationDbContext.Reservations'  is null.");
            }
            var reservation = await _reservationService.GetReservationByIdAsync(id);
            if (reservation != null)
            {
                reservation.Status = ReservationStatus.Canceled;
            }

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
