using AutoEdge.Models.Entities;

namespace AutoEdge.Services
{
    public interface IServiceNotificationService
    {
        Task<bool> SendServiceCompletionNotificationAsync(int bookingId);
        Task<bool> SendPaymentDueNotificationAsync(int bookingId);
        Task<bool> SendPaymentConfirmationNotificationAsync(int paymentId);
        Task<bool> SendPaymentReminderNotificationAsync(int bookingId);
        Task<ServiceInvoice> GenerateServiceInvoiceAsync(int bookingId);
        Task<bool> SendServiceStatusUpdateAsync(int bookingId, string status, string message);
        Task<bool> SendDriverAssignmentNotificationAsync(int bookingId, string driverName, string driverPhone);
        Task<bool> SendPickupScheduledNotificationAsync(int bookingId, DateTime pickupDate, string driverName);
        Task<bool> SendReturnScheduledNotificationAsync(int bookingId, DateTime returnDate, string driverName);
    }
}
