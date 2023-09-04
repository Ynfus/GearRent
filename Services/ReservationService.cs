using GearRent.Data;
using GearRent.Models;
using Microsoft.EntityFrameworkCore;

namespace GearRent.Services
{
    public interface IReservationService
    {
        Task<List<Reservation>> GetAllReservationsAsync();
        Task<Reservation> GetReservationByIdAsync(int id);
        Task<Reservation> GetReservationByIdAsyncInclude(int id);
        Task<List<string>> CancelUnpaidReservationsAndSendEmailsAsync();
    }

    public class ReservationService : IReservationService
    {
        private readonly ApplicationDbContext _context;

        public ReservationService(ApplicationDbContext context)
        {
            _context = context;
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
            return await _context.Reservations
                .Include(r => r.Car)
                .Include(r => r.User)
                .FirstOrDefaultAsync(r => r.Id == id);
        }
        public async Task<Reservation> GetReservationByIdAsync(int id)
        {
            return await _context.Reservations
                .FirstOrDefaultAsync(r => r.Id == id);
        }
        public async Task<List<string>> CancelUnpaidReservationsAndSendEmailsAsync()
        {
            var userEmails = new List<string>();
            var reservationsToCancel = await _context.Reservations
                .Include(r => r.Car)
                .Include(r => r.User)
                .Where(r => r.Status == ReservationStatus.Unpaid && r.StartDate < DateTime.Now.AddDays(3))
                .ToListAsync();

            foreach (var reservation in reservationsToCancel)
            {
                reservation.Status = ReservationStatus.Canceled;

                _context.Entry(reservation).State = EntityState.Modified;

                userEmails.Add(reservation.User.Email);
            }

            await _context.SaveChangesAsync();


            return userEmails;
        }
    }
}
