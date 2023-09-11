using GearRent.Data;
using GearRent.Models;
using GearRent.PaginatedList;
using GearRent.Services;
using Hangfire;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Data;

namespace GearRent.Controllers
{
    public class HangfireControler : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IEmailSender _emailSender;
        private readonly IReservationService _reservationService;



        public HangfireControler(ApplicationDbContext context, UserManager<IdentityUser> userManager,
            RoleManager<IdentityRole> roleManager, IEmailSender emailSender)
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
            _emailSender = emailSender;
        }
        //public IActionResult CanceledStatus()
        //{
        //    BackgroundJob.Schedule(() => SendCancellationEmail(), TimeSpan.FromMinutes(1));
        //    return StatusCode(200);
        //}

        //public static void SendCancellationEmail()
        //{
        //     EmailSender.SendEmailAsync1("a@a.pl", "a", "a");
        //    //Console.WriteLine("a");
        //    //Console.WriteLine(DateTime.Now.ToString()+"\n");
        //}
        //public IActionResult CanceledStatus(int id)
        //{
        //    RecurringJob.AddOrUpdate(() => DailyReservationCancellationAsync(), Cron.Minutely());
        //    return StatusCode(200);
        //}


        //public async Task DailyReservationCancellationAsync()
        //{
        //    var emailList = await _reservationService.CancelUnpaidReservationsAndSendEmailsAsync();
        //    if (emailList != null)
        //    {
        //        foreach (var email in emailList)
        //        {
        //            await _emailSender.SendEmailAsync(email, "a", "a1");
        //        }
        //    }
        //}
    }
}
