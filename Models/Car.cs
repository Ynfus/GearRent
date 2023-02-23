using Sieve.Attributes;

namespace GearRent.Models
{
    public class Car
    {
        public int Id { get; set; }
        public string Make { get; set; }
        public string Model { get; set; }
        public int Year { get; set; }
        public string Color { get; set; }

        [Sieve(CanSort = true)]
        public decimal Price { get; set; }
        public int NumberOfSeats { get; set; }
        public float EngineSize { get; set; }
        [Sieve(CanFilter = true)]
        public bool Available { get; set; }
        public string PhotoLink { get; set; }
        public ICollection<Reservation> Reservations { get; set; }
        public CarTag Tag { get; set; }
    }

}
