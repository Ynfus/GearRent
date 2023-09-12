using GearRent.Data;
using GearRent.Models;
using GearRent.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace GearRentxUnit
{
    public class UnitTest1
    {
        private readonly DbContextOptions<ApplicationDbContext> _options;
        private readonly ApplicationDbContext _context;

        public UnitTest1()
        {
            _options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new ApplicationDbContext(_options);
            _context.Database.EnsureCreated();
        }

        public void Dispose()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }
        [Fact]
        public async Task GetAllReservationsAsync_ReturnsAllReservations()
        {

            _context.Users.Add(new ApplicationUser
            {
                Id = "cc85e8d0-b878-49f3-9bcd-a1350284ad01",
                UserName = "user@user.com",
                Email = "user@user.com",
            });
            _context.Cars.Add(new Car
            {
                Id = 1,
                Make = "Ford",
                Model = "Fiesta",
                Year = 2023,
                Color = "Black",
                Price = 10000.00M,
                NumberOfSeats = 5,
                EngineSize = 2.0F,
                Available = true,
                FuelType = "Gasoline",
                Acceleration = 8.0F,
                FuelConsumption = 7.5F,
                PhotoLink = "car.jpg",
            });
            _context.Reservations.AddRange(new List<Reservation>
            {
                new Reservation
                {
                    Id = 1,
                    UserId = "cc85e8d0-b878-49f3-9bcd-a1350284ad01",
                    CarId = 1,
                    StartDate = DateTime.Parse("2023-09-01"),
                    EndDate = DateTime.Parse("2023-09-10"),
                    Status = ReservationStatus.Unpaid,
                    ReservationValue = 500.00M
                },
                new Reservation
                {
                    Id = 2,
                    UserId = "cc85e8d0-b878-49f3-9bcd-a1350284ad01",
                    CarId = 1,
                    StartDate = DateTime.Parse("2023-09-01"),
                    EndDate = DateTime.Parse("2023-09-10"),
                    Status = ReservationStatus.Unpaid,
                    ReservationValue = 500.00M
                },
            });
            _context.SaveChanges();

            var reservationService = new ReservationService(_context, null, null);

            var reservations = await reservationService.GetAllReservationsAsync();

            Assert.Equal(2, reservations.Count);

        }

        [Fact]
        public async Task GetReservationByIdAsyncInclude_ReturnsReservationById()
        {
            _context.Users.Add(new ApplicationUser
            {
                Id = "cc85e8d0-b878-49f3-9bcd-a1350284ad01",
                UserName = "user@user.com",
                Email = "user@user.com",
            });
            _context.Cars.Add(new Car
            {
                Id = 1,
                Make = "Ford",
                Model = "Fiesta",
                Year = 2023,
                Color = "Black",
                Price = 10000.00M,
                NumberOfSeats = 5,
                EngineSize = 2.0F,
                Available = true,
                FuelType = "Gasoline",
                Acceleration = 8.0F,
                FuelConsumption = 7.5F,
                PhotoLink = "car.jpg",
            });
            _context.Reservations.AddRange(new List<Reservation>
            {
                new Reservation
                {
                    Id = 1,
                    UserId = "cc85e8d0-b878-49f3-9bcd-a1350284ad01",
                    CarId = 1,
                    StartDate = DateTime.Parse("2023-09-01"),
                    EndDate = DateTime.Parse("2023-09-10"),
                    Status = ReservationStatus.Unpaid,
                    ReservationValue = 500.00M
                },
                new Reservation
                {
                    Id = 2,
                    UserId = "cc85e8d0-b878-49f3-9bcd-a1350284ad01",
                    CarId = 1,
                    StartDate = DateTime.Parse("2023-09-01"),
                    EndDate = DateTime.Parse("2023-09-10"),
                    Status = ReservationStatus.Unpaid,
                    ReservationValue = 500.00M
                },
            });
            _context.SaveChanges();
            var reservationService = new ReservationService(_context, null, null);

            var reservation = await reservationService.GetReservationByIdAsyncInclude(1);

            Assert.NotNull(reservation);
            Assert.Equal(1, reservation.Id);

        }

        [Fact]
        public async Task GetReservationByIdAsync_ReturnsReservationById()
        {
            _context.Reservations.AddRange(new List<Reservation>
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
            _context.SaveChanges();
            var reservationService = new ReservationService(_context, null, null);

            var reservation = await reservationService.GetReservationByIdAsync(1);

            Assert.NotNull(reservation);
            Assert.Equal(1, reservation.Id);

        }
        [Fact]
        public async Task CancelUnpaidReservationsAndSendEmailsAsync_CancelsUnpaidReservation()
        {
            _context.Users.Add(new ApplicationUser
            {
                Id = "cc85e8d0-b878-49f3-9bcd-a1350284ad01",
                UserName = "user@user.com",
                Email = "user@user.com",
            });
            _context.Cars.Add(new Car
            {
                Id = 1,
                Make = "Ford",
                Model = "Fiesta",
                Year = 2023,
                Color = "Black",
                Price = 10000.00M,
                NumberOfSeats = 5,
                EngineSize = 2.0F,
                Available = true,
                FuelType = "Gasoline",
                Acceleration = 8.0F,
                FuelConsumption = 7.5F,
                PhotoLink = "car.jpg",
            });
            _context.Reservations.Add(new Reservation
            {
                Id = 1,
                UserId = "cc85e8d0-b878-49f3-9bcd-a1350284ad01",
                CarId = 1,
                StartDate = DateTime.Parse("2023-09-01"),
                EndDate = DateTime.Parse("2023-09-10"),
                Status = ReservationStatus.Unpaid,
                ReservationValue = 500.00M
            });
            _context.SaveChanges();

            var emailSender = new Mock<IEmailSender>();
            var carService = new Mock<ICarService>();
            carService.Setup(c => c.GetCarAsync(It.IsAny<int>())).ReturnsAsync(new Car
            {
                Id = 1,
                Make = "Ford",
                Model = "Mondeo"
            });

            var reservationService = new ReservationService(_context, emailSender.Object, carService.Object);

            await reservationService.CancelUnpaidReservationsAndSendEmailsAsync(1);

            var reservation = await _context.Reservations.FindAsync(1);
            Assert.NotNull(reservation);
            Assert.Equal(ReservationStatus.Canceled, reservation.Status);
            emailSender.Verify(
                es => es.SendEmailAsync(
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>()
                ),
                Times.Once
            );

        }

        [Fact]
        public async Task CancelUnpaidReservationsAndSendEmailsAsync_DoesNotCancelPaidReservation()
        {
            _context.Users.Add(new ApplicationUser
            {
                Id = "cc85e8d0-b878-49f3-9bcd-a1350284ad01",
                UserName = "user@user.com",
                Email = "user@user.com",
            });
            _context.Cars.Add(new Car
            {
                Id = 1,
                Make = "Ford",
                Model = "Fiesta",
                Year = 2023,
                Color = "Black",
                Price = 10000.00M,
                NumberOfSeats = 5,
                EngineSize = 2.0F,
                Available = true,
                FuelType = "Gasoline",
                Acceleration = 8.0F,
                FuelConsumption = 7.5F,
                PhotoLink = "car.jpg",
            });
            _context.Reservations.Add(new Reservation
            {
                Id = 1,
                UserId = "cc85e8d0-b878-49f3-9bcd-a1350284ad01",
                CarId = 1,
                StartDate = DateTime.Parse("2023-09-01"),
                EndDate = DateTime.Parse("2023-09-10"),
                Status = ReservationStatus.Approved,
                ReservationValue = 500.00M
            });
            _context.SaveChanges();



            var emailSender = new Mock<IEmailSender>();
            var carService = new Mock<ICarService>();
            carService.Setup(c => c.GetCarAsync(It.IsAny<int>())).ReturnsAsync(new Car
            {
                Id = 1,
                Make = "Ford",
                Model = "Mondeo"
            });

            var reservationService = new ReservationService(_context, emailSender.Object, carService.Object);

            await reservationService.CancelUnpaidReservationsAndSendEmailsAsync(1);

            var reservation = await _context.Reservations.FindAsync(1);
            Assert.NotNull(reservation);
            Assert.Equal(ReservationStatus.Approved, reservation.Status);
            emailSender.Verify(
                es => es.SendEmailAsync(
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>()
                ),
                Times.Never
            );

        }

    }
}