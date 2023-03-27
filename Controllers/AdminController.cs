using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Data;

namespace GearRent.Controllers
{
    [Authorize(Roles = "administrator")]
    public class AdminController:Controller
    {
        public IActionResult Index()
        {

            return View();
        }
    }
}
