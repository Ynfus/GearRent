using GearRent.Data;
using GearRent.Models;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;

namespace GearRent.Services
{
    public interface IReservationService
    {
        Task<List<Reservation>> GetAllReservationsAsync();
        Task<Reservation> GetReservationByIdAsync(int id);
        Task<Reservation> GetReservationByIdAsyncInclude(int id);
        Task CancelUnpaidReservationsAndSendEmailsAsync(int id);
    }

    public class ReservationService : IReservationService
    {
        private readonly ApplicationDbContext _context;
        private readonly ICustomEmailSender _emailSender;
        private readonly ICarService _carService;

        public ReservationService(ApplicationDbContext context, ICustomEmailSender emailSender, ICarService carService)
        {
            _context = context;
            _emailSender = emailSender;
            _carService = carService;
        }

        public async Task<List<Reservation>> GetAllReservationsAsync()
        {
            return await _context.Reservations
                .Include(r => r.Car)
                .Include(r => r.User)
                .ToListAsync();
        }

        public async Task<Reservation> GetReservationByIdAsyncInclude(int id)
        {
            var reservation = await _context.Reservations
                .Include(r => r.Car)
                .Include(r => r.User)
                .Include(r=>r.BillingInfo)
                .FirstAsync(r => r.Id == id);

            return reservation;
        }
        public async Task<Reservation> GetReservationByIdAsync(int id)
        {
            return await _context.Reservations
                .FirstOrDefaultAsync(r => r.Id == id);
        }

        public async Task CancelUnpaidReservationsAndSendEmailsAsync(int reservationId)
        {
            var reservation = await _context.Reservations.FindAsync(reservationId);

            if (reservation == null)
            {
                return;
            }

            if (reservation.Status != ReservationStatus.Unpaid)
            {
                return;
            }

            reservation.Status = ReservationStatus.Canceled;
            _context.Update(reservation);
            await _context.SaveChangesAsync();

            try
            {
                Car car = await _carService.GetCarAsync(reservation.CarId);
                var email = await _context.Users
                    .Where(u => u.Id == reservation.UserId)
                    .Select(u => u.Email)
                    .FirstOrDefaultAsync();
                var subject = $"Anulacja rezerwacji {reservation.Id}";
                var body = $"Twoja rezerwacja na {car.Make} {car.Model} została anulowana ze względu na nieopłacenie jej 3 dni przed startem wypożyczenia.\nSkontaktuj się z nami mailowo w sprawie zwrotu środków";
                await _emailSender.SendEmailAsync(email, subject, body);
            }
            catch (Exception ex)
            {

            }
        }
    }
}

