using GearRent.Data;
using GearRent.Models;
using GearRent.Services;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;

namespace GearRentxUnit
{
    public class UnitTest1
    {

        [Fact]
        public async Task GetReservationByIdAsync_ReturnsCorrectReservation()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: "InMemoryReservationDatabase")
                .Options;

            using (var context = new ApplicationDbContext(options))
            {
                _ = context.Reservations.Add(new Reservation
                {
                    Id = 1,
                    UserId = "cc85e8d0-b878-49f3-9bcd-a1350284ad01",
                    CarId = 42,
                    StartDate = DateTime.Parse("2023-09-01"),
                    EndDate = DateTime.Parse("2023-09-10"),
                    Status = ReservationStatus.Unpaid,
                    ReservationValue = 500.00M
                });
                context.SaveChanges();
            }

            using (var context = new ApplicationDbContext(options))
            {
                var reservationService = new ReservationService(context, null, null);

                var reservation = await reservationService.GetReservationByIdAsync(1);

                Assert.NotNull(reservation);
                Assert.Equal(1, reservation.Id);
            }
        }
        [Fact]
        public async Task GetAllReservationsAsync_ReturnsAllReservations()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: "InMemoryReservationDb")
                .Options;

            using (var context = new ApplicationDbContext(options))
            {
                context.Reservations.AddRange(new List<Reservation>
            {
                new Reservation
                {
                    Id = 1,
                    UserId = "cc85e8d0-b878-49f3-9bcd-a1350284ad01",
                    CarId = 42,
                    StartDate = DateTime.Parse("2023-09-01"),
                    EndDate = DateTime.Parse("2023-09-10"),
                    Status = ReservationStatus.Unpaid,
                    ReservationValue = 500.00M
                },
                new Reservation
                {
                    Id = 2,
                    UserId = "cc85e8d0-b878-49f3-9bcd-a1350284ad01",
                    CarId = 42,
                    StartDate = DateTime.Parse("2023-09-01"),
                    EndDate = DateTime.Parse("2023-09-10"),
                    Status = ReservationStatus.Unpaid,
                    ReservationValue = 500.00M
                },
            });
                context.SaveChanges();
            }

            using (var context = new ApplicationDbContext(options))
            {
                var reservationService = new ReservationService(context, null, null);

                var reservations = await reservationService.GetAllReservationsAsync();

                Assert.Equal(2, reservations.Count);
            }
        }

        [Fact]
        public async Task GetReservationByIdAsyncInclude_ReturnsReservationById()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: "InMemoryReservationDb")
                .Options;

            using (var context = new ApplicationDbContext(options))
            {
                context.Reservations.AddRange(new List<Reservation>
            {
                new Reservation
                {
                    Id = 1,
                    UserId = "cc85e8d0-b878-49f3-9bcd-a1350284ad01",
                    CarId = 42,
                    StartDate = DateTime.Parse("2023-09-01"),
                    EndDate = DateTime.Parse("2023-09-10"),
                    Status = ReservationStatus.Unpaid,
                    ReservationValue = 500.00M
                },
                new Reservation
                {
                    Id = 2,
                    UserId = "cc85e8d0-b878-49f3-9bcd-a1350284ad01",
                    CarId = 42,
                    StartDate = DateTime.Parse("2023-09-01"),
                    EndDate = DateTime.Parse("2023-09-10"),
                    Status = ReservationStatus.Unpaid,
                    ReservationValue = 500.00M
                },
            });
                context.SaveChanges();
            }

            using (var context = new ApplicationDbContext(options))
            {
                var reservationService = new ReservationService(context, null, null);

                var reservation = await reservationService.GetReservationByIdAsyncInclude(1);

                Assert.NotNull(reservation);
                Assert.Equal(1, reservation.Id);
            }
        }

        [Fact]
        public async Task GetReservationByIdAsync_ReturnsReservationById()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: "InMemoryReservationDb")
                .Options;

            using (var context = new ApplicationDbContext(options))
            {
                context.Reservations.AddRange(new List<Reservation>
            {
                new Reservation
                {
                    Id = 1,
                    UserId = "cc85e8d0-b878-49f3-9bcd-a1350284ad01",
                    CarId = 42,
                    StartDate = DateTime.Parse("2023-09-01"),
                    EndDate = DateTime.Parse("2023-09-10"),
                    Status = ReservationStatus.Unpaid,
                    ReservationValue = 500.00M
                },
                new Reservation
                {
                    Id = 2,
                    UserId = "cc85e8d0-b878-49f3-9bcd-a1350284ad01",
                    CarId = 42,
                    StartDate = DateTime.Parse("2023-09-01"),
                    EndDate = DateTime.Parse("2023-09-10"),
                    Status = ReservationStatus.Unpaid,
                    ReservationValue = 500.00M
                },
            });
                context.SaveChanges();
            }

            using (var context = new ApplicationDbContext(options))
            {
                var reservationService = new ReservationService(context, null, null);

                var reservation = await reservationService.GetReservationByIdAsync(1);

                Assert.NotNull(reservation);
                Assert.Equal(1, reservation.Id);
            }
        }

    }
}