using GearRent.Data;
using GearRent.Data.Migrations;
using GearRent.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace GearRent.Services
{
    public interface ICarService
    {
        Task<Car> GetCarAsync(int id);
        Task<List<Car>> GetAllCarsAsync();
        IQueryable<Car> GetAllCarsAsyncQueryable();
        Task<CarEmailViewModel> GetCarEmailByIdAsync(int carId);
        IQueryable<Car> GetFilteredCarsAsync(DateTime? startDate, DateTime? endDate, CarTag? selectedCarTag, string? selectedColor);
        Task<List<string>> GetCarColorsAsync();
    }

    public class CarService : ICarService
    {
        private readonly ApplicationDbContext _context;
        public CarService(ApplicationDbContext context)
        {
            _context = context;

        }
        public async Task<Car> GetCarAsync(int id)
        {
            return await _context.Cars.FindAsync(id);

        }
        public async Task<List<Car>> GetAllCarsAsync()
        {
            return await _context.Cars
                .ToListAsync();
        }
        public IQueryable<Car> GetAllCarsAsyncQueryable()
        {
            return _context.Cars.AsQueryable();
        }
        public async Task<CarEmailViewModel> GetCarEmailByIdAsync(int carId)
        {
            var car = await _context.Cars
                .Where(u => u.Id == carId)
                .Select(u => new CarEmailViewModel { Make = u.Make, Model = u.Model })
                .FirstOrDefaultAsync();

            return car;
        }
        public IQueryable<Car> GetFilteredCarsAsync(DateTime? startDate, DateTime? endDate, CarTag? selectedCarTag, string? selectedColor)
        {
            var cars = _context.Cars.AsQueryable();

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
                cars = cars.Where(c => c.Tag == selectedCarTag);
            }

            return cars.AsQueryable();
        }
        public async Task<List<string>> GetCarColorsAsync()
        {
            return await _context.Cars.Select(c => c.Color).Distinct().ToListAsync();
        }

    }
}
