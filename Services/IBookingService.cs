using AutoEdge.Models.Entities;

namespace AutoEdge.Services
{
    public class CreateBookingRequest
    {
        public ServiceType ServiceType { get; set; }
        public string? CustomerId { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public string CustomerEmail { get; set; } = string.Empty;
        public string? CustomerPhone { get; set; }
        public string Make { get; set; } = string.Empty;
        public string Model { get; set; } = string.Empty;
        public int Year { get; set; }
        public string? VIN { get; set; }
        public int? Mileage { get; set; }
        public DateOnly PreferredDate { get; set; }
        public TimeOnly PreferredStart { get; set; }
        public ServiceDeliveryMethod DeliveryMethod { get; set; }
        public string? PickupAddress { get; set; } // Required when DeliveryMethod = Pickup
        public string? Notes { get; set; }
    }

    public interface IBookingService
    {
        Task<ServiceBooking> CreateBookingAsync(CreateBookingRequest request, CancellationToken ct = default);
        Task<ServiceBooking?> ConfirmBookingAsync(int bookingId, CancellationToken ct = default);
        Task<ServiceBooking?> GetBookingAsync(int bookingId, CancellationToken ct = default);
        Task<ServiceBooking?> GetBookingWithJobAsync(int bookingId, CancellationToken ct = default);
        Task<List<ServiceBooking>> GetAllBookingsAsync(CancellationToken ct = default);
        Task<List<ServiceBooking>> GetBookingsByMechanicAsync(int mechanicId, CancellationToken ct = default);
        Task<bool> ReassignBookingToMechanicAsync(int bookingId, int mechanicId, CancellationToken ct = default);
    }
}