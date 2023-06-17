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
using System.Linq;

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
