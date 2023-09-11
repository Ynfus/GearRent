using GearRent.Data;
using GearRent.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace GearRent.Services
{
    public interface IEmployeeService
    {
        Task<IQueryable<IdentityUser>> GetEmployeesAsync();
    }
    public class EmployeeService: IEmployeeService
    {
        private readonly ApplicationDbContext _context;

        public EmployeeService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IQueryable<IdentityUser>> GetEmployeesAsync()
        {
            var employeesIdList = await _context.UserRoles
                .Select(userRole => userRole.UserId)
                .ToListAsync();

            var employeesList = _context.Users
                .Where(user => employeesIdList.Contains(user.Id));

            return employeesList.AsQueryable();
        }

    }
}
