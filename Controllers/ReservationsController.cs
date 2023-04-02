using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using GearRent.Data;
using GearRent.Models;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using System.Diagnostics;
using Newtonsoft.Json.Linq;
using System.Globalization;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Stripe;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Cors;
using Stripe.Checkout;
using Newtonsoft.Json;
using NuGet.Versioning;

namespace GearRent.Controllers
{
    public class ReservationsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ReservationsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Reservations
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.Reservations.Include(r => r.Car).Include(r => r.User);
            return View(await applicationDbContext.ToListAsync());
        }
        public async Task<IActionResult> Checkout(string payment_intent, string payment_intent_client_secret, string redirect_status, Reservation reservation)
        {
            ViewBag.MyValue = reservation.Id;
            return View();
        }
        // GET: Reservations/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Reservations == null)
            {
                return NotFound();
            }

            var reservation = await _context.Reservations
                .Include(r => r.Car)
                .Include(r => r.User)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (reservation == null)
            {
                return NotFound();
            }

            return View(reservation);
        }

        // GET: Reservations/Create
        public IActionResult Create(int carId, string startDate, string endDate)
        {

            string userId = HttpContext.User.Identity.IsAuthenticated ? HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value : null;
            if (userId == null)
            {
                return Redirect("/Identity/Account/Login");
            }
            ViewData["CarId"] = carId;
            ViewData["StartDate"] = startDate;
            //DateTime endDateTime = DateTime.Parse(endDate);
            ViewData["EndDate"] = endDate;
            ViewData["UserId"] = userId;
            return View();
        }

        // POST: Reservations/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,UserId,CarId,StartDate,EndDate,Status")] Reservation reservation)
        {
            Car car = await _context.Cars.FindAsync(reservation.CarId);
            Debug.Write(reservation.EndDate);
            reservation.Car = car;
            _context.Add(reservation);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Checkout), reservation);



            //ViewData["CarId"] = new SelectList(_context.Cars, "Id", "Id", reservation.CarId);
            //ViewData["UserId"] = new SelectList(_context.Set<ApplicationUser>(), "Id", "Id", reservation.UserId);
            //return View(reservation);
        }

        // GET: Reservations/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.Reservations == null)
            {
                return NotFound();
            }

            var reservation = await _context.Reservations.FindAsync(id);
            if (reservation == null)
            {
                return NotFound();
            }
            ViewData["CarId"] = new SelectList(_context.Cars, "Id", "Id", reservation.CarId);
            ViewData["UserId"] = new SelectList(_context.Set<ApplicationUser>(), "Id", "Id", reservation.UserId);
            return View(reservation);
        }



        public async Task<IActionResult> Thanks1(int id)
        {
            string userId = HttpContext.User.Identity.IsAuthenticated ? HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value : null;

            var reservation = await _context.Reservations.FindAsync(id);
            if (reservation.UserId!=userId)
            {
                return NotFound();
            }
            reservation.Status = ReservationStatus.Approved;
            await _context.SaveChangesAsync();

            return RedirectToAction("Thanks", "Reservations", new { id=id });
        }
        public async Task<IActionResult> Thanks(int id)
        {
            Reservation reservation = await _context.Reservations.FindAsync(id);
           Car car = await _context.Cars.FindAsync(reservation.CarId);
            ViewBag.Car = car;
            // Przekaż model do widoku i wyświetl stronę
            return View(reservation);

        }



        /// //////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        //[DisableCors]
        //[HttpPost]
        //public ActionResult ProcessPayment(Reservation reservation)
        //{
        //    StripeConfiguration.ApiKey = "sk_test_51Mqvj3Ea7vPLiBopPCZjU5tY28KQYLyPC2K9sPLhKUsh3w1dq26Xu9qRXDrPNmvFHSXYusKaWChyCNbD8HTwsgUx00qcWXBCjz";
        //    try
        //    {


        //        var options = new ChargeCreateOptions
        //        {
        //            Amount = 2000,
        //            Currency = "pln",
        //            Source = "tok_visa",
        //            Description = "no elo elo",
        //        };
        //        var service = new ChargeService();
        //        var charge = service.Create(options);

        //        return View("Success");
        //    }
        //    catch (Exception ex)
        //    {
        //        ViewBag.Error = ex.Message;
        //        return View("Error");
        //    }
        //}
        /// ///////////////////////////////////



    /// ///////////////////////////////////////////////////////////////////////////////////////


    // POST: Reservations/Edit/5
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
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

        // GET: Reservations/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            //Debug.Write(id+"eeeeeeeeeeeeeeeeeeeeeeeeeeeeee");
            //if (id == null || _context.Reservations == null)
            //{
            //    return NotFound();
            //}

            //var reservation = await _context.Reservations
            //    //.Include(r => r.Car)
            //    //.Include(r => r.User)
            //    .FirstOrDefaultAsync(m => m.Id == id);
            //if (reservation == null)
            //{
            //    return NotFound();
            //}

            //return View(reservation);
            if (_context.Reservations == null)
            {
                return Problem("Entity set 'ApplicationDbContext.Reservations'  is null.");
            }
            var reservation = await _context.Reservations.FindAsync(id);
            if (reservation != null)
            {
                _context.Reservations.Remove(reservation);
            }

            await _context.SaveChangesAsync();
            return StatusCode(200);
        }

        // POST: Reservations/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.Reservations == null)
            {
                return Problem("Entity set 'ApplicationDbContext.Reservations'  is null.");
            }
            var reservation = await _context.Reservations.FindAsync(id);
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
