using AutoEdge.Models.Entities;
using AutoEdge.Services;
using AutoEdge.Tests.TestSupport;
using Microsoft.Extensions.Logging;
using Moq;

namespace AutoEdge.Tests;

public class BookingServiceTests
{
    [Fact]
    public async Task CreateBookingAsync_ShouldThrow_WhenOutsideBusinessHours()
    {
        using var context = TestDbFactory.CreateContext();
        var service = CreateService(context);

        var request = BuildRequest();
        request.PreferredStart = new TimeOnly(7, 30);

        await Assert.ThrowsAsync<InvalidOperationException>(() => service.CreateBookingAsync(request));
    }

    [Fact]
    public async Task CreateBookingAsync_ShouldThrow_WhenDateIsNotFuture()
    {
        using var context = TestDbFactory.CreateContext();
        var service = CreateService(context);

        var request = BuildRequest();
        request.PreferredDate = DateOnly.FromDateTime(DateTime.Today);

        await Assert.ThrowsAsync<InvalidOperationException>(() => service.CreateBookingAsync(request));
    }

    [Fact]
    public async Task CreateBookingAsync_ShouldCreateConfirmedBookingAndServiceJob_WhenRequestIsValid()
    {
        using var context = TestDbFactory.CreateContext();
        context.Mechanics.Add(new Mechanic
        {
            Name = "Primary Mechanic",
            Rating = 4.9,
            IsAvailable = true
        });
        await context.SaveChangesAsync();

        var service = CreateService(context);
        var request = BuildRequest();

        var booking = await service.CreateBookingAsync(request);

        Assert.Equal(ServiceBookingStatus.Confirmed, booking.Status);
        Assert.True(booking.Id > 0);

        var storedJob = context.ServiceJobs.Single(j => j.ServiceBookingId == booking.Id);
        Assert.NotNull(storedJob.MechanicId);
        Assert.Equal(request.PreferredDate, storedJob.ScheduledDate);
        Assert.Equal(request.PreferredStart, storedJob.ScheduledStart);
    }

    private static CreateBookingRequest BuildRequest()
    {
        return new CreateBookingRequest
        {
            ServiceType = ServiceType.Maintenance,
            CustomerId = "cust-1",
            CustomerName = "Test Customer",
            CustomerEmail = "customer@example.com",
            CustomerPhone = "0123456789",
            Make = "Toyota",
            Model = "Corolla",
            Year = 2022,
            VIN = "WDB12345678901234",
            Mileage = 25000,
            PreferredDate = DateOnly.FromDateTime(DateTime.Today.AddDays(2)),
            PreferredStart = new TimeOnly(9, 0),
            DeliveryMethod = ServiceDeliveryMethod.Dropoff,
            Notes = "Routine service"
        };
    }

    private static BookingService CreateService(AutoEdge.Data.ApplicationDbContext context)
    {
        var qr = new Mock<IQRCodeService>();
        qr.Setup(x => x.GenerateQRCodeForServiceBookingAsync(It.IsAny<int>(), It.IsAny<int>()))
            .ReturnsAsync("qr-token");
        qr.Setup(x => x.GenerateQRCodeImageAsync(It.IsAny<string>()))
            .ReturnsAsync([1, 2, 3]);

        var email = new Mock<IEmailService>();
        email.Setup(x => x.SendServiceBookingQRCodeEmailWithPdfAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<DateTime>(),
                It.IsAny<string>(),
                It.IsAny<byte[]>()))
            .Returns(Task.CompletedTask);

        var pdf = new Mock<IQRCodePdfService>();
        pdf.Setup(x => x.GenerateServiceBookingQRCodePdfAsync(It.IsAny<ServiceBooking>(), It.IsAny<byte[]>()))
            .ReturnsAsync([1, 2, 3, 4]);

        var pickup = new Mock<IPickupDropoffService>();
        var logger = new Mock<ILogger<BookingService>>();

        return new BookingService(context, qr.Object, email.Object, pdf.Object, logger.Object, pickup.Object);
    }
}
