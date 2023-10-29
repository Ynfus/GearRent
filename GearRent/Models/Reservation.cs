
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;

namespace GearRent.Models
{
    public class Reservation
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public IdentityUser User { get; set; }
        public int CarId { get; set; }
        public Car Car { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public ReservationStatus Status { get; set; } = ReservationStatus.Unpaid;

        public decimal ReservationValue { get; set; }
        public int? BillingInfoId { get; set; }
        public BillingInfo BillingInfo { get; set; }
    }
}
