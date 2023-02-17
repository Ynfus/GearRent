namespace GearRent.Models
{
    public class Car
    {
        public int Id { get; set; }
        public string Make { get; set; }
        public string Model { get; set; }
        public int Year { get; set; }
        public string Color { get; set; }
        public int NumberOfSeats { get; set; }
        public float EngineSize { get; set; }
        public bool Available { get; set; }
        public ICollection<Reservation> Reservations { get; set; }
    }
}
