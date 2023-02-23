using Sieve.Attributes;

namespace GearRent.Models
{
    public class Reservation
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public ApplicationUser User { get; set; }
        public int CarId { get; set; }
        public Car Car { get; set; }
        [Sieve(CanFilter = true, Name = "StartDate")]
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public ReservationStatus Status { get; set; } = ReservationStatus.Pending;
    }
}
