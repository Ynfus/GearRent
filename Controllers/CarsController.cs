using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GearRent.Data;
using GearRent.Models;
using GearRent.PaginatedList;
using GearRent.Services;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;

namespace GearRent.Controllers
{

    public class CarsController : Controller
    {
        private readonly ApplicationDbContext _context;

        private readonly ICarService _carService;
        private readonly IReservationService _reservationService;
        private readonly IEmailSender _emailSender;
        private readonly IUserService _userService;
        private readonly IWebHostEnvironment _IWebHostEnvironment;

        public CarsController(ApplicationDbContext context, ICarService carService, IReservationService reservationService, IEmailSender emailSender, IUserService userService, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _carService = carService;
            _reservationService = reservationService;
            _emailSender = emailSender;
            _userService = userService;
            _IWebHostEnvironment = webHostEnvironment;
        }

        public async Task<IActionResult> List(int? pageNumber)
        {
            int pageSize = 6;

            return _context.Cars != null ?
                        View(await PaginatedList<Car>.CreateAsync(_carService.GetAllCarsAsyncQueryable(), pageNumber ?? 1, pageSize)) :
                        Problem("Entity set 'ApplicationDbContext.Cars'  is null.");

        }
        public async Task<IActionResult> Index(string sortOrder, DateTime? startDate, DateTime? endDate, int? pageNumber, CarTag? selectedCarTag, string? selectedColor)
        {
            ViewBag.PriceSortParm = String.IsNullOrEmpty(sortOrder) ? "price_desc" : "";
            ViewBag.NameSortParm = sortOrder == "name" ? "name_desc" : "name";
            var cars = _carService.GetFilteredCarsAsync(startDate, endDate, selectedCarTag, selectedColor);
            ViewBag.CarTags = Enum.GetValues(typeof(CarTag));

            var colors = await _carService.GetCarColorsAsync();
            ViewBag.Colors = colors;
            int pageSize = 3;
            return View(await PaginatedList<Car>.CreateAsync(cars, pageNumber ?? 1, pageSize));
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Cars == null)
            {
                return NotFound();
            }

            var car = await _context.Cars
                .FirstOrDefaultAsync(m => m.Id == id);
            if (car == null)
            {
                return NotFound();
            }

            return View(car);
        }

        public IActionResult Create()
        {
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Car car)
        {
            if (ModelState.IsValid)
            {
                await _carService.AddCarAsync(car);
                return RedirectToAction(nameof(Index));
            }

            return View(car);
        }

        private bool CarExists(int id)
        {
            return (_context.Cars?.Any(e => e.Id == id)).GetValueOrDefault();
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
            var fuelItems = Enum.GetValues(typeof(FuelType))
                .Cast<FuelType>()
                .Select(t => new SelectListItem
                {
                    Value = ((int)t).ToString(),
                    Text = t.ToString()
                })
                .ToList();
            ViewBag.CarTags = tagItems;
            ViewBag.FuelType = fuelItems;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddCar(Car car)
        {

            if (ModelState.IsValid)
            {
                if (car.PhotoFile != null && car.PhotoFile.Length > 0)
                {
                    var fileName = Guid.NewGuid().ToString() + Path.GetExtension(car.PhotoFile.FileName);

                    var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads", fileName);

                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await car.PhotoFile.CopyToAsync(fileStream);
                    }

                    car.PhotoLink = "/uploads/" + fileName;
                }

                await _carService.AddCarAsync(car);

                return RedirectToAction("CarList", "Cars");
            }
            return View(car);
        }
        [HttpGet]
        public async Task<IActionResult> CarList(int? pageNumber)
        {
            var carList = _carService.GetAllCarsAsyncQueryable();
            int pageSize = 3;
            var paginatedCars = await PaginatedList<Car>.CreateAsync(carList.AsNoTracking(), pageNumber ?? 1, pageSize);

            return View(paginatedCars);
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
        public async Task<IActionResult> EditCar(int id, Car car)
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
                return RedirectToAction("CarList", "Cars");
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
            _emailSender.SendEmailAsync(email, subject, body);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
