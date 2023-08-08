using GearRent.Data;
using GearRent.Data.Migrations;
using GearRent.Models;
using Microsoft.AspNetCore.Hosting;
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
        IQueryable<Car> GetFilteredCarsAsync(DateTime? startDate, DateTime? endDate, CarTag? selectedCarTag, string? selectedColor,
                                                    float? minFuelConsumption, float? maxFuelConsumption, float? minEngineCapacity, float? maxEngineCapacity,
                                                    float? minAcceleration, float? maxAcceleration, decimal? minPrice, decimal? maxPrice);
        Task<List<string>> GetCarColorsAsync();
        Task AddCarAsync(Car car);
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
        public IQueryable<Car> GetFilteredCarsAsync(DateTime? startDate, DateTime? endDate, CarTag? selectedCarTag, string? selectedColor,
                                                    float? minFuelConsumption, float? maxFuelConsumption, float? minEngineCapacity, float? maxEngineCapacity,
                                                    float? minAcceleration, float? maxAcceleration, decimal? minPrice, decimal? maxPrice)
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

            if (minFuelConsumption != null && maxFuelConsumption != null)
            {
                cars = cars.Where(c => c.FuelConsumption >= minFuelConsumption && c.FuelConsumption <= maxFuelConsumption);
            }

            if (minEngineCapacity != null && maxEngineCapacity != null)
            {
                cars = cars.Where(c => c.EngineSize >= minEngineCapacity && c.EngineSize <= maxEngineCapacity);
            }

            if (minAcceleration != null && maxAcceleration != null)
            {
                cars = cars.Where(c => c.Acceleration >= minAcceleration && c.Acceleration <= maxAcceleration);
            }
            if (minPrice != null && maxPrice != null)
            {
                cars = cars.Where(c => c.Price >= minPrice && c.Price <= maxPrice);
            }
            return cars.AsQueryable();
        }

        public async Task<List<string>> GetCarColorsAsync()
        {
            return await _context.Cars.Select(c => c.Color).Distinct().ToListAsync();
        }
        public async Task AddCarAsync(Car car)
        {

            _context.Add(car);
            await _context.SaveChangesAsync();
        }

    }
}
