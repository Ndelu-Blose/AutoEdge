using AutoEdge.Models.Entities;
using AutoEdge.Services;
using AutoEdge.Tests.TestSupport;
using Microsoft.Extensions.Logging;
using Moq;

namespace AutoEdge.Tests;

public class ServiceNotificationServiceTests
{
    [Fact]
    public async Task GenerateServiceInvoiceAsync_ShouldUseChecklistActualCostAndPickupFee()
    {
        using var context = TestDbFactory.CreateContext();
        var booking = SeedBooking(context, ServiceDeliveryMethod.Pickup);
        var job = new ServiceJob
        {
            ServiceBookingId = booking.Id,
            MechanicId = 1,
            ScheduledDate = booking.PreferredDate,
            ScheduledStart = booking.PreferredStart,
            DurationMin = 90
        };
        context.ServiceJobs.Add(job);
        context.ServiceChecklists.Add(new ServiceChecklist
        {
            ServiceJob = job,
            MechanicId = 1,
            ServiceType = "Maintenance",
            TotalActualCost = 1000m,
            TotalEstimatedCost = 900m,
            CreatedBy = "test"
        });
        await context.SaveChangesAsync();

        var service = CreateService(context);
        var invoice = await service.GenerateServiceInvoiceAsync(booking.Id);

        Assert.Equal(1100m, invoice.SubTotal);
        Assert.Equal(165m, invoice.TaxAmount);
        Assert.Equal(1265m, invoice.TotalAmount);
    }

    [Fact]
    public async Task GenerateServiceInvoiceAsync_ShouldFallbackToEstimatedCost_WhenNoChecklistOrExecution()
    {
        using var context = TestDbFactory.CreateContext();
        var booking = SeedBooking(context, ServiceDeliveryMethod.Dropoff);
        booking.EstimatedCost = 1500m;
        await context.SaveChangesAsync();

        var service = CreateService(context);
        var invoice = await service.GenerateServiceInvoiceAsync(booking.Id);

        Assert.Equal(1500m, invoice.SubTotal);
        Assert.Equal(225m, invoice.TaxAmount);
        Assert.Equal(1725m, invoice.TotalAmount);
    }

    [Fact]
    public async Task GenerateServiceInvoiceAsync_ShouldReturnExistingInvoice_WhenAlreadyCreated()
    {
        using var context = TestDbFactory.CreateContext();
        var booking = SeedBooking(context, ServiceDeliveryMethod.Dropoff);
        var existing = new ServiceInvoice
        {
            ServiceBookingId = booking.Id,
            InvoiceNumber = "INV-EXISTING",
            SubTotal = 500m,
            TaxAmount = 75m,
            TotalAmount = 575m,
            Status = "Sent",
            CreatedAt = DateTime.UtcNow
        };
        context.ServiceInvoices.Add(existing);
        await context.SaveChangesAsync();

        var service = CreateService(context);
        var invoice = await service.GenerateServiceInvoiceAsync(booking.Id);

        Assert.Equal(existing.Id, invoice.Id);
        Assert.Equal("INV-EXISTING", invoice.InvoiceNumber);
        Assert.Single(context.ServiceInvoices.Where(i => i.ServiceBookingId == booking.Id));
    }

    private static ServiceNotificationService CreateService(AutoEdge.Data.ApplicationDbContext context)
    {
        var email = new Mock<IEmailService>();
        var logger = new Mock<ILogger<ServiceNotificationService>>();
        return new ServiceNotificationService(context, email.Object, logger.Object);
    }

    private static ServiceBooking SeedBooking(AutoEdge.Data.ApplicationDbContext context, ServiceDeliveryMethod deliveryMethod)
    {
        var booking = new ServiceBooking
        {
            Reference = $"SV-{Guid.NewGuid():N}"[..12],
            ServiceType = ServiceType.Maintenance,
            CustomerName = "Invoice Tester",
            CustomerEmail = "invoice@example.com",
            Make = "Ford",
            Model = "Ranger",
            Year = 2021,
            PreferredDate = DateOnly.FromDateTime(DateTime.Today.AddDays(3)),
            PreferredStart = new TimeOnly(10, 0),
            EstimatedDurationMin = 90,
            EstimatedCost = 1200m,
            DeliveryMethod = deliveryMethod,
            Status = ServiceBookingStatus.Confirmed
        };
        context.ServiceBookings.Add(booking);
        context.SaveChanges();
        return booking;
    }
}
