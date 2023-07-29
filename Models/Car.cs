
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GearRent.Models
{
    public class Car
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Make is required.")]
        public string Make { get; set; }

        [Required(ErrorMessage = "Model is required.")]
        public string Model { get; set; }

        [Range(1900, 2100, ErrorMessage = "Year must be between 1900 and 2100.")]
        public int Year { get; set; }

        public string Color { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "Price must be a non-negative value.")]
        public decimal Price { get; set; }

        [Range(0, 15, ErrorMessage = "Number of seats must be between 0 and 15.")]
        public int NumberOfSeats { get; set; }

        [Range(0.1, double.MaxValue, ErrorMessage = "Engine size must be a positive value.")]
        public float EngineSize { get; set; }

        public bool Available { get; set; }

        [EnumDataType(typeof(FuelType), ErrorMessage = "Invalid fuel type.")]
        public string FuelType { get; set; }

        [Range(0.1, 30, ErrorMessage = "Acceleration must be a positive value.")]
        public float Acceleration { get; set; }

        [Range(0.1, 100, ErrorMessage = "Fuel consumption must be a positive value.")]
        public float FuelConsumption { get; set; }

        public string PhotoLink { get; set; }

        public ICollection<Reservation>? Reservations { get; set; }

        public CarTag Tag { get; set; }

        [NotMapped]
        [Display(Name = "Photo")]
        public IFormFile PhotoFile { get; set; }

    }


}
