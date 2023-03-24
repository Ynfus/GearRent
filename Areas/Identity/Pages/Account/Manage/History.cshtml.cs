using GearRent.Data;
using GearRent.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;

namespace GearRent.Areas.Identity.Pages.Account.Manage
{
    public class HistoryModel : PageModel
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly ApplicationDbContext _context;
        public HistoryModel(ApplicationDbContext context,UserManager<IdentityUser> userManager,
        SignInManager<IdentityUser> signInManager)
        {
            _context = context;
            _userManager = userManager;
            _signInManager = signInManager;
        }
        public List<Reservation> reservations { get; set; }
        private async Task LoadAsync(IdentityUser user)
        {
            var userName = await _context.Reservations.FindAsync(user);
            var reservation = await _context.Reservations.Where(r => r.CarId == 1).ToListAsync(); 

        }

        public async Task<IActionResult> OnGetAsync()
        {

            var user = await _userManager.GetUserAsync(User);
            Debug.Write(user);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            await LoadAsync(user);
            return Page();
        }

     
    }
}
