using GearRent.Data;
using GearRent.Models;
using GearRent.PaginatedList;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
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

        public AdminController(ApplicationDbContext context, UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;

        }
        [HttpGet]
        public async Task<IActionResult> CarList(int? pageNumber)
        {
            var carList = from r in _context.Cars
                          select r;

            int pageSize = 3;
            var paginatedCars = await PaginatedList<Car>.CreateAsync(carList.AsQueryable(), pageNumber ?? 1, pageSize);

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
                .Include(r => r.Car)
                //.Include(r => r.User)
                ;



            int pageSize = 3;

            return View(await PaginatedList<Reservation>.CreateAsync(upcomingReservations.AsNoTracking(), pageNumber ?? 1, pageSize));
        }

        // GET: Cars/Edit/5
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

        // POST: Cars/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
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
            var reservation = await _context.Reservations.FindAsync(id);
            if (reservation == null)
            {
                return NotFound();
            }

            _context.Reservations.Remove(reservation);
            var email = await _context.Users
                .Where(u => u.Id == reservation.UserId)
                .Select(u => u.Email)
                .FirstOrDefaultAsync();
            var car = await _context.Cars
                .Where(u => u.Id == reservation.CarId)
                .Select(u => new { u.Make, u.Model })
                .FirstOrDefaultAsync();
            SmtpClient smtpClient = new SmtpClient("localhost", 2525);
            smtpClient.DeliveryMethod = SmtpDeliveryMethod.Network;
            MailMessage mailMessage = new MailMessage();
            mailMessage.From = new MailAddress("sender@example.com");
            mailMessage.To.Add(new MailAddress(email));
            mailMessage.Subject = $"Anulacja rezerwacji {reservation.Id}";
            mailMessage.Body = $"Twoja rezerwacja na {car.Make} {car.Model} na okres {reservation.StartDate.Date.ToShortDateString()} - {reservation.EndDate.Date.ToShortDateString()} została anulowana.\nW razie problemów prosimy o kontakt \nPozdrawiam";

            smtpClient.Send(mailMessage);
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
            var employeesIdList = await _context.UserRoles
                .Select(userRole => userRole.UserId)
                .ToListAsync();

            var employeesList = _context.Users
                .Where(user => employeesIdList.Contains(user.Id))
                .AsQueryable();

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
        //[HttpPost]
        ////[ValidateAntiForgeryToken]
        //[ActionName("AddEmployee")]
        //public async Task<IActionResult> AddEmployeePost(IdentityUser model)
        //{
        //    if (ModelState.IsValid)
        //    {
        //        //var passwordValidator = new PasswordValidator<IdentityUser>();
        //        //var passwordValidationResult = await passwordValidator.ValidateAsync(_userManager, null, model.PasswordHash);
        //        //if (!passwordValidationResult.Succeeded)
        //        //{
        //        //    foreach (var error in passwordValidationResult.Errors)
        //        //    {
        //        //        ModelState.AddModelError(string.Empty, error.Description);
        //        //    }
        //        //    return View(model);
        //        //}

        //        //var existingUser = await _userManager.FindByEmailAsync(model.Email);
        //        //if (existingUser != null)
        //        //{
        //        //    ModelState.AddModelError(string.Empty, "Email is already taken.");
        //        //    return View(model);
        //        //}

        //        var user = new IdentityUser
        //        {
        //            UserName = model.Email,
        //            Email = model.Email
        //        };

        //        var result = await _userManager.CreateAsync(user, model.PasswordHash);

        //        if (result.Succeeded)
        //        {
        //            await _userManager.AddToRoleAsync(user, "Employee");

        //            return RedirectToAction(nameof(Index));
        //        }

        //        foreach (var error in result.Errors)
        //        {
        //            ModelState.AddModelError(string.Empty, error.Description);
        //        }
        //    }

        //    return View(model);
        //}



    }
}
