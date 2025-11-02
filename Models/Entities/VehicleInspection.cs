using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AutoEdge.Models.Entities
{
    public class VehicleInspection
    {
        [Key]
        public int VehicleInspectionId { get; set; }

        [Required]
        public int ServiceBookingId { get; set; }

        [Required]
        [StringLength(450)]
        public string InspectorId { get; set; } = string.Empty;

        public DateTime CheckInTime { get; set; } = DateTime.UtcNow;

        public int? OdometerReading { get; set; }

        [StringLength(2000)]
        public string? ExteriorCondition { get; set; } // JSON string for exterior details

        [StringLength(2000)]
        public string? InteriorCondition { get; set; } // JSON string for interior details

        [StringLength(2000)]
        public string? DamageDocumentation { get; set; } // JSON string for damages with photos

        [StringLength(2000)]
        public string? FluidLevels { get; set; } // JSON string for fluid measurements

        public int? FuelLevel { get; set; } // Percentage

        [StringLength(1000)]
        public string? RequiredMaintenance { get; set; }

        [StringLength(2000)]
        public string? InspectionPhotos { get; set; } // JSON array of photo URLs

        [StringLength(500)]
        public string? InspectionReportUrl { get; set; }

        [Required]
        [StringLength(50)]
        public string InspectionStatus { get; set; } = "InProgress"; // InProgress, Completed, RequiresAttention

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        [ForeignKey("ServiceBookingId")]
        public virtual ServiceBooking? ServiceBooking { get; set; }

        [ForeignKey("InspectorId")]
        public virtual ApplicationUser? Inspector { get; set; }
    }
}
