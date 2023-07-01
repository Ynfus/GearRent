using GearRent.Data;
using GearRent.Models;
using GearRent.PaginatedList;
using GearRent.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Stripe.Radar;
using System;
using System.Data;
using System.Diagnostics;
using System.Drawing.Printing;
using System.Linq;
using System.Net.Mail;

namespace GearRent.Controllers
{
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IEmailSender _emailSender;
        private readonly IUserService _userService;
        private readonly ICarService _carService;
        private readonly IReservationService _reservationService;
        private readonly IEmployeeService _employeeService;


        public AdminController(ApplicationDbContext context, UserManager<IdentityUser> userManager,
            RoleManager<IdentityRole> roleManager, IEmailSender emailSender, ICarService carService,
            IReservationService reservationService, IUserService userService, IEmployeeService employeeService)
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
            _carService = carService;
            _reservationService = reservationService;
            _userService = userService;
            _emailSender = emailSender;
            _employeeService = employeeService;
        }
        [HttpGet]
        public async Task<IActionResult> CarList(int? pageNumber)
        {
            //var carList = from r in _context.Cars
            //              select r;

            var carList = _carService.GetAllCarsAsyncQueryable();
            int pageSize = 3;
            var paginatedCars = await PaginatedList<Car>.CreateAsync(carList.AsNoTracking(), pageNumber ?? 1, pageSize);

            return View(paginatedCars);
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
                .Include(r => r.Car);
            int pageSize = 3;

            return View(await PaginatedList<Reservation>.CreateAsync(upcomingReservations.AsNoTracking(), pageNumber ?? 1, pageSize));
        }

        public async Task<IActionResult> EditCar(int? id)
        {
            if (id == null || _context.Cars == null)
            {
                return NotFound();
            }

            var car = await _context.Cars.FindAsync(id);
            if (car == null)
            {
                return NotFound();
            }
            return View(car);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditCar(int id, [Bind("Id,Make,Model,Year,Color,NumberOfSeats,EngineSize,Available,PhotoLink")] Car car)
        {
            if (id != car.Id)
            {
                return NotFound();
            }
            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(car);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CarExists(car.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(CarList));
            }
            return View(car);
        }
        [HttpGet]
        public async Task<IActionResult> CarReservations(int id, int? pageNumber)
        {
            var carReservations = _context.Reservations
                .Where(r => r.CarId == id)
                .AsQueryable();

            var car = _context.Cars.Find(id);

            if (carReservations == null || car == null)
            {
                return NotFound();
            }

            var carReservationViewModels = carReservations.Select(r => new CarReservationsViewModel
            {
                CarMake = car.Make,
                CarModel = car.Model,
                CarYear = car.Year,
                UserId = r.UserId,
                StartDate = r.StartDate.Date,
                EndDate = r.EndDate
            }).AsQueryable();

            int pageSize = 3;
            var paginatedReservations = await PaginatedList<CarReservationsViewModel>.CreateAsync(carReservationViewModels, pageNumber ?? 1, pageSize);

            return View(paginatedReservations);
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteCar(int id)
        {
            var car = await _context.Cars.FindAsync(id);
            if (car == null)
            {
                return NotFound();
            }

            _context.Cars.Remove(car);
            await _context.SaveChangesAsync();

            return NoContent();
        }
        [HttpDelete]
        public async Task<IActionResult> DeleteReservation(int id)
        {
            var reservation = await _reservationService.GetReservationByIdAsync(id);
            if (reservation == null)
            {
                return NotFound();
            }

            _context.Reservations.Remove(reservation);
            var email = await _userService.GetUserEmailByIdAsync(reservation.UserId);
            var car = await _carService.GetCarEmailByIdAsync(reservation.CarId);
            var subject = $"Anulacja rezerwacji {reservation.Id}";
            var body = $"Twoja rezerwacja na {car.Make} {car.Model} na okres {reservation.StartDate.Date.ToShortDateString()} - {reservation.EndDate.Date.ToShortDateString()} została anulowana.\nW razie problemów prosimy o kontakt \nPozdrawiam";
            _emailSender.SendEmailAsync(email,subject,body);
            await _context.SaveChangesAsync();

            return NoContent();
        }
        private bool CarExists(int id)
        {
            return (_context.Cars?.Any(e => e.Id == id)).GetValueOrDefault();
        }

        public IActionResult Index1()
        {

            return View();
        }
        public async Task<IActionResult> EmployeesList(int? pageNumber)
        {
            var employeesList = await _employeeService.GetEmployeesAsync();
            int pageSize = 3;
            var paginatedUsers = await PaginatedList<IdentityUser>.CreateAsync(employeesList, pageNumber ?? 1, pageSize);

            return View(paginatedUsers);
        }
        [HttpGet]
        public IActionResult AddEmployee()
        {
            var roles = _roleManager.Roles.Select(r => new SelectListItem { Value = r.Name, Text = r.NormalizedName }).ToList();
            ViewBag.Roles = roles;

            var users = _userManager.Users.ToList();
            return View(users);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddEmployee(string email, string role)
        {
            var user = await _userManager.FindByEmailAsync(email);
            var isInRole = await _userManager.IsInRoleAsync(user, role);
            if (isInRole)
            {
                return View();
            }
            await _userManager.AddToRoleAsync(user, role);

            return RedirectToAction(nameof(Index));
        }
        [HttpGet]
        public IActionResult SearchUsers(string email)
        {
            var users = _userManager.Users
                .Where(u => u.Email.Contains(email))
                .Select(u => new { email = u.Email })
                .ToList();

            return Json(users);
        }
        public async Task<IActionResult> MonthlyReport(int year, int month)
        {
            var reservations = await _context.Reservations
                .Where(r => r.StartDate.Year == year && r.StartDate.Month == month)
                .ToListAsync();

            decimal totalRevenue = reservations.Sum(r => r.ReservationValue);

            int totalReservations = reservations.Count;

            var reportData = new ReportViewModel
            {
                Year = year,
                Month = month,
                TotalRevenue = totalRevenue,
                TotalReservations = totalReservations
            };

            return View(reportData);
        }
        public IActionResult MonthlyReportGet(int year, int month)
        {
            var reservations = _context.Reservations
                .Where(r => r.StartDate.Year == year && r.StartDate.Month == month)
                .ToList();

            decimal totalRevenue = reservations.Sum(r => r.ReservationValue);

            int totalReservations = reservations.Count;

            var reportData = new ReportViewModel
            {
                Year = year,
                Month = month,
                TotalRevenue = totalRevenue,
                TotalReservations = totalReservations
            };

            return Json(reportData);
        }
    }
}
