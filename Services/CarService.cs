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



    }
}
