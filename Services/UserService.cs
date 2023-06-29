using GearRent.Data;
using GearRent.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace GearRent.Services
{
    public interface IUserService
    {
        Task<List<IdentityUser>> GetAllUsersAsync();
    }

    public class UserService : IUserService
    {
        private readonly ApplicationDbContext _context;



        public async Task<List<IdentityUser>> GetAllUsersAsync()
        {
            return await _context.Users.ToListAsync();
        }
    }
}
