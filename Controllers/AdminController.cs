using GearRent.Data;
using GearRent.Models;
using GearRent.PaginatedList;
using GearRent.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Data;

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
        public async Task<IActionResult> Index(int? pageNumber, int? daysAhead, bool? showInProgress, bool? showApproved, bool? showUnpaid, bool? showFinished, bool? showCanceled, int? pageSize, bool isAjaxRequest)
        {
            var addDaysDate = DateTime.Now.AddDays(daysAhead ?? 10);
            var currentDate = DateTime.Now.AddDays(-100);

            var upcomingReservations = _context.Reservations
                .Where(r => r.StartDate <= addDaysDate && r.StartDate >= currentDate);

            upcomingReservations = upcomingReservations.Include(r => r.Car);

            if (showInProgress == true || showApproved == true || showUnpaid == true || showFinished == true || showCanceled == true)
            {
                upcomingReservations = upcomingReservations.Where(r =>
                    (showInProgress == true && r.Status == ReservationStatus.InProgress) ||
                    (showApproved == true && r.Status == ReservationStatus.Approved) ||
                    (showUnpaid == true && r.Status == ReservationStatus.Unpaid) ||
                    (showFinished == true && r.Status == ReservationStatus.Finished) ||
                    (showCanceled == true && (r.Status == ReservationStatus.Canceled))
                );
            }

            if (isAjaxRequest)
            {
                var paginatedList = await PaginatedList<Reservation>.CreateAsync(upcomingReservations.AsNoTracking(), pageNumber ?? 1, pageSize ?? 3);
                HttpContext.Response.Headers.Add("X-Total-Pages", paginatedList.TotalPages.ToString());
                HttpContext.Response.Headers.Add("X-Current-Page", (pageNumber ?? 1).ToString());
                return PartialView("_FilteredResultsReservationsPartialView", paginatedList);
            }
            else
            {
                return View(await PaginatedList<Reservation>.CreateAsync(upcomingReservations.AsNoTracking(), pageNumber ?? 1, pageSize ?? 3));

            }
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
        public ActionResult YearlyReportGet(int year)
        {
            var monthlyReportData = _context.Reservations
                .Where(r => r.EndDate.Year == year)
                .GroupBy(r => r.EndDate.Month)
                .Select(group => new
                {
                    Month = new DateTime(year, group.Key, 1).ToString("MMMM"),
                    Reservations = group.Count(),
                    Revenue = group.Sum(r => r.ReservationValue)
                })
                .ToList();

            return Json(monthlyReportData);
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
