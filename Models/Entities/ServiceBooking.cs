// using System.ComponentModel.DataAnnotations;
// using System.ComponentModel.DataAnnotations.Schema;

// namespace AutoEdge.Models.Entities
// {
//     public class ServiceBooking
//     {
//         [Key]
//         public int ServiceBookingId { get; set; }

//         [Required]
//         [StringLength(450)]
//         public string CustomerId { get; set; } = string.Empty;

//         [Required]
//         [StringLength(50)]
//         public string VehicleMake { get; set; } = string.Empty;

//         [Required]
//         [StringLength(50)]
//         public string VehicleModel { get; set; } = string.Empty;

//         [Required]
//         public int VehicleYear { get; set; }

//         [StringLength(17)]
//         public string? VehicleVin { get; set; }

//         public int? VehicleMileage { get; set; }

//         [Required]
//         [StringLength(100)]
//         public string ServiceType { get; set; } = string.Empty; // Maintenance, Repairs, Inspection

//         [Required]
//         public DateTime BookingDate { get; set; }

//         [Required]
//         public TimeSpan BookingTime { get; set; }

//         [StringLength(1000)]
//         public string? ServiceNotes { get; set; }

//         public int EstimatedDurationMinutes { get; set; } = 60;

//         [Column(TypeName = "decimal(10,2)")]
//         public decimal EstimatedCostMin { get; set; } = 0;

//         [Column(TypeName = "decimal(10,2)")]
//         public decimal EstimatedCostMax { get; set; } = 0;

//         [Required]
//         [StringLength(50)]
//         public string BookingStatus { get; set; } = "Requested"; // Requested, Confirmed, Scheduled, Completed, Cancelled

//         [Required]
//         [StringLength(20)]
//         public string ReferenceNumber { get; set; } = string.Empty;

//         public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

//         public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

//         // Navigation properties
//         [ForeignKey("CustomerId")]
//         public virtual ApplicationUser? Customer { get; set; }

//         public virtual ICollection<ServiceSchedule> ServiceSchedules { get; set; } = new List<ServiceSchedule>();
//         public virtual ICollection<VehiclePickup> VehiclePickups { get; set; } = new List<VehiclePickup>();
//         public virtual ICollection<VehicleInspection> VehicleInspections { get; set; } = new List<VehicleInspection>();
//         public virtual ICollection<ServiceExecution> ServiceExecutions { get; set; } = new List<ServiceExecution>();
//         public virtual ICollection<ServicePayment> ServicePayments { get; set; } = new List<ServicePayment>();
//         public virtual ICollection<ServiceInvoice> ServiceInvoices { get; set; } = new List<ServiceInvoice>();
//     }
// }

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AutoEdge.Models.Entities
{
    public enum ServiceType { Maintenance = 1, Repairs = 2, Inspection = 3 }
    public enum ServiceBookingStatus { Pending = 1, Confirmed = 2, Waitlisted = 3, Canceled = 4, Completed = 5 }
    public enum ServiceDeliveryMethod { Pickup = 1, Dropoff = 2 }

    public class ServiceBooking
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(30)]
        public string Reference { get; set; } = string.Empty; // e.g., SV-20251013-0042

        [Required]
        public ServiceType ServiceType { get; set; }

        [StringLength(450)]
        public string? CustomerId { get; set; } // nullable for guest

        [Required]
        [StringLength(80)]
        public string CustomerName { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        [StringLength(120)]
        public string CustomerEmail { get; set; } = string.Empty;

        [Phone]
        [StringLength(30)]
        public string? CustomerPhone { get; set; }

        [Required]
        [StringLength(50)]
        public string Make { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string Model { get; set; } = string.Empty;

        [Range(1900, 2100)]
        public int Year { get; set; }

        [StringLength(17)]
        public string? VIN { get; set; }

        public int? Mileage { get; set; }

        public DateOnly PreferredDate { get; set; }
        public TimeOnly PreferredStart { get; set; }

        public int EstimatedDurationMin { get; set; }
        [Column(TypeName = "decimal(18,2)")]
        public decimal EstimatedCost { get; set; }

        [StringLength(500)]
        public string? Notes { get; set; }

        [Required]
        public ServiceDeliveryMethod DeliveryMethod { get; set; }

        [StringLength(500)]
        public string? QRCode { get; set; }

        public bool IsCheckedIn { get; set; } = false;
        public DateTime? CheckedInDate { get; set; }

        public bool IsServiceStarted { get; set; } = false;

        // Additional fields for enhanced functionality
        public int? CustomerRating { get; set; }
        public string? CustomerComments { get; set; }
        public DateTime? RatedAt { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? ServiceStartedDate { get; set; }
        public DateTime? ServiceCompletedDate { get; set; }

        public ServiceBookingStatus Status { get; set; } = ServiceBookingStatus.Pending;
        public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

        // Navigation properties
        [ForeignKey("CustomerId")]
        public virtual ApplicationUser? Customer { get; set; }

        public virtual ServiceJob? ServiceJob { get; set; }
        public virtual ICollection<ServiceInvoice> ServiceInvoices { get; set; } = new List<ServiceInvoice>();
        public virtual ICollection<ServiceExecution> ServiceExecutions { get; set; } = new List<ServiceExecution>();
        public virtual ICollection<VehiclePickup> VehiclePickups { get; set; } = new List<VehiclePickup>();

        [StringLength(50)]
        public string CreatedBy { get; set; } = "web";
    }
}