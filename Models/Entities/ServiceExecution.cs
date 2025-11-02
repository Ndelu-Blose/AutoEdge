using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AutoEdge.Models.Entities
{
    public class ServiceExecution
    {
        [Key]
        public int ServiceExecutionId { get; set; }

        [Required]
        public int ServiceBookingId { get; set; }

        [Required]
        [StringLength(450)]
        public string TechnicianId { get; set; } = string.Empty;

        public DateTime? StartTime { get; set; }

        public DateTime? EndTime { get; set; }

        [StringLength(2000)]
        public string? TasksCompleted { get; set; } // JSON string for completed tasks

        [StringLength(2000)]
        public string? PartsUsed { get; set; } // JSON array of parts with quantities and costs

        [Column(TypeName = "decimal(5,2)")]
        public decimal LaborHours { get; set; } = 0;

        [Column(TypeName = "decimal(10,2)")]
        public decimal LaborRate { get; set; } = 0;

        [StringLength(1000)]
        public string? AdditionalIssues { get; set; }

        public bool AdditionalWorkApproved { get; set; } = false;

        public bool QualityCheckPassed { get; set; } = false;

        [StringLength(1000)]
        public string? TestDriveNotes { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal TotalCost { get; set; } = 0;

        public bool AdvisorApproval { get; set; } = false;

        [StringLength(450)]
        public string? ApprovedBy { get; set; }

        public DateTime? ApprovedAt { get; set; }

        [Required]
        [StringLength(50)]
        public string ExecutionStatus { get; set; } = "Scheduled"; // Scheduled, InProgress, Completed, AwaitingReview, ApprovedForBilling

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        [ForeignKey("ServiceBookingId")]
        public virtual ServiceBooking? ServiceBooking { get; set; }

        [ForeignKey("TechnicianId")]
        public virtual ApplicationUser? Technician { get; set; }

        [ForeignKey("ApprovedBy")]
        public virtual ApplicationUser? ApprovedByUser { get; set; }
    }
}
