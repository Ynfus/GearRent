using GearRent.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Stripe;
using static GearRent.Controllers.PaymentIntentApiController;

namespace GearRent.Controllers
{
    [Route("create-payment-intent")]
    [ApiController]
    public class PaymentIntentApiController : Controller
    {

        [HttpPost]
        public ActionResult Create(PaymentIntentCreateRequest request)
        {
            string result = string.Join("", request.Items.Select(item => item.Id));
            var paymentIntentService = new PaymentIntentService();
            var paymentIntent = paymentIntentService.Create(new PaymentIntentCreateOptions
            {
                Description="Płatność za rezerwacje nr "+result,
                Amount = CalculateOrderAmount(request.Items),
                Currency = "pln",
                AutomaticPaymentMethods = new PaymentIntentAutomaticPaymentMethodsOptions
                {
                    Enabled = true,
                },
            });

            return Json(new { clientSecret = paymentIntent.ClientSecret });
        }

        private int CalculateOrderAmount(Item[] items)
        {
            // Replace this constant with a calculation of the order's amount
            // Calculate the order total on the server to prevent
            // people from directly manipulating the amount on the client
            return 1400;
        }

        public class Item
        {
            [JsonProperty("id")]
            public string Id { get; set; }
        }

        public class PaymentIntentCreateRequest
        {
            [JsonProperty("items")]
            public Item[] Items { get; set; }
        }
    }
}

