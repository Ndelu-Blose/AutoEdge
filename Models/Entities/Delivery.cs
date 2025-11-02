using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AutoEdge.Models.Entities
{
    public class Delivery
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int ContractId { get; set; }

        [Required]
        [StringLength(50)]
        public string DeliveryType { get; set; } = string.Empty; // DealerPickup, HomeDelivery, ThirdPartyDelivery

        public DateTime ScheduledDate { get; set; }

        public DateTime? CompletedDate { get; set; }

        [Required]
        [StringLength(500)]
        public string DeliveryAddress { get; set; } = string.Empty;

        [StringLength(50)]
        public string City { get; set; } = string.Empty;

        [StringLength(50)]
        public string State { get; set; } = string.Empty;

        [StringLength(10)]
        public string ZipCode { get; set; } = string.Empty;

        [StringLength(50)]
        public string Country { get; set; } = string.Empty;

        public string? DriverUserId { get; set; }

        [StringLength(100)]
        public string DriverName { get; set; } = string.Empty;

        [StringLength(20)]
        public string DriverPhone { get; set; } = string.Empty;

        [StringLength(20)]
        public string Status { get; set; } = "Scheduled"; // Scheduled, InTransit, Delivered, Failed, Cancelled

        [StringLength(100)]
        public string TrackingNumber { get; set; } = string.Empty;

        [StringLength(1000)]
        public string Notes { get; set; } = string.Empty;

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        public DateTime ModifiedDate { get; set; } = DateTime.UtcNow;

        public bool IsActive { get; set; } = true;

        [Column(TypeName = "decimal(18,2)")]
        public decimal DeliveryFee { get; set; } = 0;

        [StringLength(100)]
        public string DeliveryCompany { get; set; } = string.Empty;

        [StringLength(100)]
        public string VehicleLicensePlate { get; set; } = string.Empty;

        public DateTime? PickupTime { get; set; }

        public DateTime? EstimatedArrival { get; set; }

        public DateTime? ActualArrival { get; set; }

        [StringLength(500)]
        public string DeliveryInstructions { get; set; } = string.Empty;

        [StringLength(100)]
        public string ContactPersonName { get; set; } = string.Empty;

        [StringLength(20)]
        public string ContactPersonPhone { get; set; } = string.Empty;

        [StringLength(100)]
        public string ContactPersonEmail { get; set; } = string.Empty;

        public bool RequiresSignature { get; set; } = true;

        [StringLength(500)]
        public string SignaturePath { get; set; } = string.Empty;

        [StringLength(100)]
        public string SignedByName { get; set; } = string.Empty;

        public DateTime? SignedDate { get; set; }

        [StringLength(500)]
        public string DeliveryPhotoPath { get; set; } = string.Empty;

        [StringLength(1000)]
        public string CustomerFeedback { get; set; } = string.Empty;

        public int CustomerRating { get; set; } = 0; // 1-5 stars

        [StringLength(500)]
        public string FailureReason { get; set; } = string.Empty;

        public int RescheduleCount { get; set; } = 0;

        public DateTime? LastRescheduleDate { get; set; }

        [StringLength(500)]
        public string RescheduleReason { get; set; } = string.Empty;

        public bool IsInsured { get; set; } = false;

        [StringLength(100)]
        public string InsuranceProvider { get; set; } = string.Empty;

        [StringLength(100)]
        public string InsurancePolicyNumber { get; set; } = string.Empty;

        [Column(TypeName = "decimal(18,2)")]
        public decimal InsuranceAmount { get; set; } = 0;

        // GPS Tracking fields
        [Column(TypeName = "decimal(10,8)")]
        public decimal? CurrentLatitude { get; set; }

        [Column(TypeName = "decimal(11,8)")]
        public decimal? CurrentLongitude { get; set; }

        public DateTime? LastLocationUpdate { get; set; }

        [Column(TypeName = "decimal(5,2)")]
        public decimal? DistanceRemaining { get; set; }

        public int? EstimatedMinutesRemaining { get; set; }

        // QR Code fields for delivery verification
        [StringLength(500)]
        public string QRCode { get; set; } = string.Empty;

        public DateTime? QRCodeExpiry { get; set; }

        public bool IsDelivered { get; set; } = false;

        public DateTime? ActualDeliveryDate { get; set; }

        // Navigation properties
        [ForeignKey("ContractId")]
        public virtual Contract Contract { get; set; } = null!;

        [ForeignKey("DriverUserId")]
        public virtual ApplicationUser? Driver { get; set; }
    }
}