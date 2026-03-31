using AutoEdge.Models.Entities;
using AutoEdge.Repositories;
using AutoEdge.Services;
using AutoEdge.Tests.TestSupport;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using System.Linq.Expressions;

namespace AutoEdge.Tests;

public class PaymentServiceTests
{
    [Fact]
    public async Task ProcessPaymentAsync_ShouldReturnUnsupportedMethod_WhenMethodIsUnknown()
    {
        using var context = TestDbFactory.CreateContext();
        var service = CreateService(context, []);

        var result = await service.ProcessPaymentAsync(new PaymentRequest
        {
            PurchaseId = 1,
            Amount = 100m,
            PaymentMethod = "Crypto"
        });

        Assert.False(result.IsSuccess);
        Assert.Equal("UNSUPPORTED_PAYMENT_METHOD", result.ErrorCode);
    }

    [Fact]
    public async Task GetPaymentsByPurchaseIdAsync_ShouldReturnPaymentsMatchingContractId()
    {
        using var context = TestDbFactory.CreateContext();
        var payments = new List<Payment>
        {
            new() { Id = 1, ContractId = 5, Status = "Completed", PaymentMethod = "Cash" },
            new() { Id = 2, ContractId = 7, Status = "Pending", PaymentMethod = "Cash" },
            new() { Id = 3, ContractId = 5, Status = "Processing", PaymentMethod = "BankTransfer" }
        };
        var service = CreateService(context, payments);

        var result = await service.GetPaymentsByPurchaseIdAsync(5);

        Assert.Equal(2, result.Count);
        Assert.All(result, p => Assert.Equal(5, p.ContractId));
    }

    [Fact]
    public async Task ValidatePaymentAsync_ShouldReturnTrue_WhenPaymentIsCompletedWithoutStripeIntent()
    {
        using var context = TestDbFactory.CreateContext();
        var payments = new List<Payment>
        {
            new() { Id = 9, ContractId = 3, Status = "Completed", StripePaymentIntentId = string.Empty, PaymentMethod = "Cash" }
        };
        var service = CreateService(context, payments);

        var isValid = await service.ValidatePaymentAsync(9);

        Assert.True(isValid);
    }

    private static PaymentService CreateService(AutoEdge.Data.ApplicationDbContext context, List<Payment> payments)
    {
        var paymentsRepository = new Mock<IRepository<Payment>>();
        paymentsRepository.Setup(x => x.GetAllAsync()).ReturnsAsync(payments);
        paymentsRepository.Setup(x => x.GetByIdAsync(It.IsAny<int>()))
            .ReturnsAsync((int id) => payments.FirstOrDefault(p => p.Id == id));

        var purchasesRepository = new Mock<IRepository<Purchase>>();
        var genericRepository = new Mock<IRepository<Vehicle>>();

        var unitOfWork = new Mock<IUnitOfWork>();
        unitOfWork.SetupGet(x => x.Payments).Returns(paymentsRepository.Object);
        unitOfWork.SetupGet(x => x.Purchases).Returns(purchasesRepository.Object);
        unitOfWork.SetupGet(x => x.Vehicles).Returns(genericRepository.Object);
        unitOfWork.SetupGet(x => x.VehicleImages).Returns(new Mock<IRepository<VehicleImage>>().Object);
        unitOfWork.SetupGet(x => x.Customers).Returns(new Mock<IRepository<Customer>>().Object);
        unitOfWork.SetupGet(x => x.Inquiries).Returns(new Mock<IRepository<Inquiry>>().Object);
        unitOfWork.SetupGet(x => x.Reservations).Returns(new Mock<IRepository<Reservation>>().Object);
        unitOfWork.SetupGet(x => x.Documents).Returns(new Mock<IRepository<Document>>().Object);
        unitOfWork.SetupGet(x => x.Contracts).Returns(new Mock<IRepository<Contract>>().Object);
        unitOfWork.SetupGet(x => x.Deliveries).Returns(new Mock<IRepository<Delivery>>().Object);
        unitOfWork.SetupGet(x => x.ServiceBookings).Returns(new Mock<IRepository<ServiceBooking>>().Object);
        unitOfWork.Setup(x => x.SaveChangesAsync()).ReturnsAsync(1);

        var configuration = new ConfigurationBuilder().AddInMemoryCollection(
        [
            new KeyValuePair<string, string?>("Stripe:SecretKey", "sk_test_123")
        ]).Build();

        var logger = new Mock<ILogger<PaymentService>>();
        var email = new Mock<IEmailService>();

        return new PaymentService(unitOfWork.Object, context, configuration, logger.Object, email.Object);
    }
}
