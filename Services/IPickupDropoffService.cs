using AutoEdge.Models.Entities;

namespace AutoEdge.Services
{
    public class CreatePickupRequest
    {
        public int? ServiceBookingId { get; set; }
        public string CustomerId { get; set; } = string.Empty;
        public string CustomerName { get; set; } = string.Empty;
        public string CustomerPhone { get; set; } = string.Empty;
        public string PickupLocation { get; set; } = string.Empty;
        public DateTime PickupDate { get; set; }
        public TimeSpan PickupTime { get; set; }
        public string VehicleMake { get; set; } = string.Empty;
        public string VehicleModel { get; set; } = string.Empty;
        public int VehicleYear { get; set; }
        public string? VehicleLicensePlate { get; set; }
        public string? SpecialInstructions { get; set; }
    }

    public class DriverAssignment
    {
        public string DriverId { get; set; } = string.Empty;
        public string DriverName { get; set; } = string.Empty;
        public string DriverPhone { get; set; } = string.Empty;
        public string VehicleRegistration { get; set; } = string.Empty;
        public double Rating { get; set; }
        public bool IsAvailable { get; set; }
    }

    public interface IPickupDropoffService
    {
        Task<bool> CheckServiceableZoneAsync(string address);
        Task<List<DriverAssignment>> GetAvailableDriversAsync(DateTime date, TimeSpan time);
        Task<VehiclePickup> CreatePickupRequestAsync(CreatePickupRequest request);
        Task<bool> AssignDriverAsync(int pickupId, string driverId);
        Task<bool> UpdatePickupStatusAsync(int pickupId, string status, string? notes = null);
        Task<bool> UploadConditionPhotosAsync(int pickupId, List<IFormFile> photos, string photoType);
        Task<bool> ScheduleReturnAsync(int pickupId, DateTime returnDate, TimeSpan returnTime);
        Task<List<VehiclePickup>> GetCustomerPickupsAsync(string customerId);
        Task<VehiclePickup?> GetPickupDetailsAsync(int pickupId);
        Task<bool> CaptureCustomerSignatureAsync(int pickupId, string signatureData);
        Task<bool> SendDriverNotificationAsync(int pickupId, string message);
        Task<bool> SendCustomerNotificationAsync(int pickupId, string message);
    }
}
