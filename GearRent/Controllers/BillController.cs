using DinkToPdf;
using DinkToPdf.Contracts;
using GearRent.Data;
using GearRent.Models;
using GearRent.Services;
using Hangfire;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using System.Drawing.Printing;

namespace GearRent.Controllers
{
    public class BillController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ICustomEmailSender _emailSender;
        private readonly IUserService _userService;
        private readonly ICarService _carService;
        private readonly IReservationService _reservationService;
        private readonly IBackgroundJobClient _backgroundJobClient;
        private readonly IConverter _pdfConverter;


        public BillController(ApplicationDbContext context, ICustomEmailSender emailSender,
            IUserService userService, ICarService carService, IReservationService reservationService, IBackgroundJobClient backgroundJobClient, IConverter pdfConverter)
        {
            _context = context;
            _emailSender = emailSender;
            _userService = userService;
            _carService = carService;
            _reservationService = reservationService;
            _backgroundJobClient = backgroundJobClient;
            _pdfConverter = pdfConverter;
        }
        public async Task<IActionResult> BillCreate(int reservationId)
        {

            var reservation = await _reservationService.GetReservationByIdAsyncInclude(2091);
            if (reservation == null)
            {
                return NotFound();
            }
            var billModel = new BillModel
            {
                BillId = 1,
                Name = /*"nnnnl",*/reservation.User.UserName,
                CarModel = /*"nnnnl",*/$"{reservation.Car.Make} {reservation.Car.Model}",
                Date = DateTime.Now.Date.ToString("d"),
                TotalValue = /*1,*/reservation.ReservationValue,
                Days = /*8*/(int)(reservation.EndDate - reservation.StartDate).TotalDays
            };
            // return View("GenerateBillTemplate",billModel);
            var htmlContent = ViewToString("GenerateBillTemplate", billModel);
            var doc = new HtmlToPdfDocument
            {
                GlobalSettings =
            {
                PaperSize = DinkToPdf.PaperKind.A4,
                Orientation = Orientation.Portrait,
                Margins = new MarginSettings
                {
                    Top = 0,
                    Right = 0,
                    Bottom = 0,
                    Left = 0
                },

            },
                Objects =
            {
                new ObjectSettings
                {
                    HtmlContent = htmlContent,

                }
            }
            };

            var pdfBytes = _pdfConverter.Convert(doc);
            return File(pdfBytes, "application/pdf", "invoice.pdf");
        }

        private string ViewToString(string viewName, object model)
        {
            ViewData.Model = model;
            using (var writer = new StringWriter())
            {
                var viewEngine = HttpContext.RequestServices.GetRequiredService<ICompositeViewEngine>();
                var view = viewEngine.FindView(ControllerContext, viewName, false).View;
                var viewContext = new ViewContext(ControllerContext, view, ViewData, TempData, writer, new HtmlHelperOptions());
                view.RenderAsync(viewContext).GetAwaiter().GetResult();
                return writer.GetStringBuilder().ToString();
            }
        }

    }
}
