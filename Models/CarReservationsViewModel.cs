namespace GearRent.Models
{
    public class CarReservationsViewModel
    {

        public string CarMake { get; set; }
        public string CarModel { get; set; }
        public int CarYear { get; set; }
        public string UserId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }
}
