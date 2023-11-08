using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using GearRent.Data;
using GearRent.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.Security.Claims;

namespace GearRent.Controllers
{
    public class BillingInfoController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public BillingInfoController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager= userManager;
        }

        // GET: BillingInfo
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.BillingInfos.Include(b => b.User);
            return View(await applicationDbContext.ToListAsync());
        }

        // GET: BillingInfo/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.BillingInfos == null)
            {
                return NotFound();
            }

            var billingInfo = await _context.BillingInfos
                .Include(b => b.User)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (billingInfo == null)
            {
                return NotFound();
            }

            return View(billingInfo);
        }

        // GET: BillingInfo/Create
        public IActionResult Create()
        {
            ViewData["UserId"] = new SelectList(_context.Users, "Id", "Id");
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> AddBillingInfo([FromBody] BillingInfo newBillingInfo)
        {
            string userId = HttpContext.User.Identity.IsAuthenticated ? HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value : null;
            var user = _userManager.Users.FirstOrDefault(u => u.Id == userId);

            if (user != null)
            {
                newBillingInfo.UserId = userId;
                newBillingInfo.User = user;
                _context.Add(newBillingInfo);
                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "New billing info added successfully." });
            }


            

            return Json(new { success = false, message = "Validation failed." });
        }

        // POST: BillingInfo/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,UserId,FirstName,LastName,PhoneNumber,Street,City,PostalCode,User")] BillingInfo billingInfo)
        {
            var user =  _userManager.Users.FirstOrDefault(u => u.Id == billingInfo.UserId);

            if (user != null)
            {
                billingInfo.User = user;
            }
            if (ModelState.IsValid)
            {
                _context.Add(billingInfo);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));

            }

            ViewData["UserId"] = new SelectList(_context.Users, "Id", "Id", billingInfo.UserId);
            return View(billingInfo);
        }

        // GET: BillingInfo/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.BillingInfos == null)
            {
                return NotFound();
            }

            var billingInfo = await _context.BillingInfos.FindAsync(id);
            if (billingInfo == null)
            {
                return NotFound();
            }
            ViewData["UserId"] = new SelectList(_context.Users, "Id", "Id", billingInfo.UserId);
            return View(billingInfo);
        }

        // POST: BillingInfo/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,UserId,FirstName,LastName,PhoneNumber,Street,City,PostalCode")] BillingInfo billingInfo)
        {
            if (id != billingInfo.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(billingInfo);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!BillingInfoExists(billingInfo.Id))
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
            ViewData["UserId"] = new SelectList(_context.Users, "Id", "Id", billingInfo.UserId);
            return View(billingInfo);
        }

        // GET: BillingInfo/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.BillingInfos == null)
            {
                return NotFound();
            }

            var billingInfo = await _context.BillingInfos
                .Include(b => b.User)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (billingInfo == null)
            {
                return NotFound();
            }

            return View(billingInfo);
        }

        // POST: BillingInfo/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.BillingInfos == null)
            {
                return Problem("Entity set 'ApplicationDbContext.BillingInfos'  is null.");
            }
            var billingInfo = await _context.BillingInfos.FindAsync(id);
            if (billingInfo != null)
            {
                _context.BillingInfos.Remove(billingInfo);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool BillingInfoExists(int id)
        {
          return (_context.BillingInfos?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
