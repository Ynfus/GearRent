using Microsoft.AspNetCore.Identity;

namespace GearRent.Models
{
    public class ApplicationUser : IdentityUser
    {
        public ICollection<Reservation> Reservations { get; set; }
    }
}
