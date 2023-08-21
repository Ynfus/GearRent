using System.ComponentModel.DataAnnotations;

namespace GearRent.Models
{
    public class ComplaintViewModel
    {
        [Required(ErrorMessage = "To pole jest wymagane.")]
        public string FullName { get; set; }

        [Required(ErrorMessage = "To pole jest wymagane.")]
        [EmailAddress(ErrorMessage = "Niepoprawny format adresu e-mail.")]
        public string Email { get; set; }

        public string Phone { get; set; }

        [Required(ErrorMessage = "To pole jest wymagane.")]
        public string ReservationNumber { get; set; }

        [Required(ErrorMessage = "To pole jest wymagane.")]
        public string ComplaintDescription { get; set; }
    }
}





