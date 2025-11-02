using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AutoEdge.Models.Entities
{
    public class VehiclePickup
    {
        [Key]
        public int VehiclePickupId { get; set; }

        [Required]
        public int ServiceBookingId { get; set; }

        public int? DriverId { get; set; }

        [Required]
        [StringLength(500)]
        public string PickupLocation { get; set; } = string.Empty;

        [Required]
        public DateTime PickupDate { get; set; }

        [Required]
        public TimeSpan PickupTime { get; set; }

        public DateTime? DropoffDate { get; set; }

        public TimeSpan? DropoffTime { get; set; }

        [Required]
        [StringLength(50)]
        public string PickupStatus { get; set; } = "Scheduled"; // Scheduled, EnRoute, Collected, InService, Completed, Delivered, Failed

        [StringLength(2000)]
        public string? VehicleConditionPickup { get; set; } // JSON string for condition details

        [StringLength(2000)]
        public string? VehicleConditionDropoff { get; set; } // JSON string for condition details

        public int? MileagePickup { get; set; }

        public int? MileageDropoff { get; set; }

        public int? FuelLevelPickup { get; set; } // Percentage

        public int? FuelLevelDropoff { get; set; } // Percentage

        [StringLength(2000)]
        public string? PickupPhotos { get; set; } // JSON array of photo URLs

        [StringLength(2000)]
        public string? DropoffPhotos { get; set; } // JSON array of photo URLs

        [StringLength(2000)]
        public string? CustomerSignatures { get; set; } // JSON for pickup and dropoff signatures

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        [ForeignKey("ServiceBookingId")]
        public virtual ServiceBooking? ServiceBooking { get; set; }

        [ForeignKey("DriverId")]
        public virtual Driver? Driver { get; set; }
    }
}
