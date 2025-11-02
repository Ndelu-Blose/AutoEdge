using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AutoEdge.Models.Entities
{
    public class ServiceReminder
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int ContractId { get; set; }

        [Required]
        [StringLength(100)]
        public string ServiceType { get; set; } = string.Empty; // Oil Change, Tire Rotation, Brake Inspection, etc.

        [StringLength(500)]
        public string Description { get; set; } = string.Empty;

        [Required]
        public DateTime ScheduledDate { get; set; }

        public DateTime? CompletedDate { get; set; }

        [StringLength(20)]
        public string Status { get; set; } = "Scheduled"; // Scheduled, Completed, Cancelled, Overdue

        public int? Mileage { get; set; }

        [StringLength(1000)]
        public string Notes { get; set; } = string.Empty;

        [StringLength(100)]
        public string ServiceProvider { get; set; } = string.Empty;

        [StringLength(200)]
        public string ServiceLocation { get; set; } = string.Empty;

        [Column(TypeName = "decimal(18,2)")]
        public decimal? EstimatedCost { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal? ActualCost { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        public DateTime? LastReminderSent { get; set; }

        public int RemindersSent { get; set; } = 0;

        // Navigation properties
        [ForeignKey("ContractId")]
        public virtual Contract Contract { get; set; } = null!;
    }
}