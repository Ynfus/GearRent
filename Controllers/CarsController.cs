using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using GearRent.Data;
using GearRent.Models;
using Azure;
using GearRent.PaginatedList;
using System.Drawing.Printing;

namespace GearRent.Controllers
{
    public class CarsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CarsController(ApplicationDbContext context )
        {
            _context = context;
        }

        //GET: Cars
        public async Task<IActionResult> List(int? pageNumber)
        {
            int pageSize = 6;

            return _context.Cars != null ?
                        View(await PaginatedList<Car>.CreateAsync(_context.Cars.AsNoTracking(), pageNumber ?? 1, pageSize)) :
                        Problem("Entity set 'ApplicationDbContext.Cars'  is null.");

        }
        public async Task<IActionResult> Index(string sortOrder, DateTime? startDate, DateTime? endDate, int? pageNumber, CarTag? selectedCarTag, string? selectedColor)
        {
            ViewBag.PriceSortParm = String.IsNullOrEmpty(sortOrder) ? "price_desc" : "";
            ViewBag.NameSortParm = sortOrder == "name" ? "name_desc" : "name";
            var cars = from c in _context.Cars
                       select c;
            ViewBag.CarTags = Enum.GetValues(typeof(CarTag));

            var colors = _context.Cars.Select(c => c.Color).Distinct().ToList();
            ViewBag.Colors = colors;

            if (startDate != null && endDate != null)
            {
                cars = cars.Where(c => c.Reservations.All(r => r.EndDate < startDate || r.StartDate > endDate));
            }
            if (!string.IsNullOrEmpty(selectedColor))
            {
                cars = cars.Where(c => c.Color == selectedColor);
            }
            if (selectedCarTag.HasValue)
            {
                var tag = selectedCarTag;
                cars = cars.Where(c => c.Tag == tag);
            }
            switch (sortOrder)
            {
                case "price_desc":
                    cars = cars.OrderByDescending(c => c.Price);
                    break;
                case "name":
                    cars = cars.OrderBy(c => c.Make);
                    break;
                case "name_desc":
                    cars = cars.OrderByDescending(c => c.Make);
                    break;
                default:
                    cars = cars.OrderBy(c => c.Price);
                    break;
            }
            int pageSize = 3;
            return View(await PaginatedList<Car>.CreateAsync(cars.AsNoTracking(), pageNumber ?? 1, pageSize));
            //return View(cars.ToList());
        }



        // GET: Cars/Details/5
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

        // GET: Cars/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Cars/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Make,Model,Year,Color,NumberOfSeats,EngineSize,Available,PhotoLink")] Car car)
        {
            if (ModelState.IsValid)
            {
                _context.Add(car);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(car);
        }

        // GET: Cars/Edit/5
        public async Task<IActionResult> Edit(int? id)
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
        public async Task<IActionResult> Edit(int id, [Bind("Id,Make,Model,Year,Color,NumberOfSeats,EngineSize,Available,PhotoLink")] Car car)
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
                return RedirectToAction(nameof(Index));
            }
            return View(car);
        }

        // GET: Cars/Delete/5
        public async Task<IActionResult> Delete(int? id)
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

        // POST: Cars/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.Cars == null)
            {
                return Problem("Entity set 'ApplicationDbContext.Cars'  is null.");
            }
            var car = await _context.Cars.FindAsync(id);
            if (car != null)
            {
                _context.Cars.Remove(car);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool CarExists(int id)
        {
          return (_context.Cars?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
