using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations.Schema;

namespace GearRent.Models
{
    public class BillingInfo
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; } 
        public string PhoneNumber { get; set; } 
        public string Street { get; set; }
        public string City { get; set; }
        public string PostalCode { get; set; }
    }
}
