using GearRent.Data;
using GearRent.Models;
using GearRent.PaginatedList;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Data;
using System.Diagnostics;

namespace GearRent.Controllers
{
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AdminController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult AddCar()
        {
            var tagItems = Enum.GetValues(typeof(CarTag))
    .Cast<CarTag>()
    .Select(t => new SelectListItem
    {
        Value = ((int)t).ToString(),
        Text = t.ToString()
    })
    .ToList();
            ViewBag.CarTags = tagItems;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddCar([Bind("Id,Make,Model,Year,Color,NumberOfSeats,EngineSize,Available,PhotoLink,Tag")] Car car)
        {
            if (ModelState.IsValid)
            {
                _context.Add(car);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(car);
        }
        public async Task<IActionResult> Index(int? pageNumber)
        {
            var upcomingReservations = from r in _context.Reservations
                                       select r;
            var currentDate = DateTime.Now.AddDays(-10);
            upcomingReservations = upcomingReservations
                .Where(r => r.StartDate > currentDate /*&& r.Status == ReservationStatus.Approved*/)
                .Include(r => r.Car)
                //.Include(r => r.User)
                ;



            int pageSize = 3;
            return View(await PaginatedList<Reservation>.CreateAsync(upcomingReservations.AsNoTracking(), pageNumber ?? 1, pageSize));
        }

        public IActionResult Index1()
        {

            return View();
        }
    }
}
