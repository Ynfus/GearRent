using GearRent.Data;
using GearRent.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace GearRent.Services
{
    public interface IUserService
    {
        Task<List<IdentityUser>> GetAllUsersAsync();
        Task<string> GetUserEmailByIdAsync(string userId);
    }

    public class UserService : IUserService
    {
        private readonly ApplicationDbContext _context;

        public UserService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<IdentityUser>> GetAllUsersAsync()
        {
            return await _context.Users.ToListAsync();
        }
        public async Task<string> GetUserEmailByIdAsync(string userId)
        {
            var user = await _context.Users.FindAsync(userId);
            return user?.Email;
        }

    }
}
