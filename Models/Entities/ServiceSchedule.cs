using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AutoEdge.Models.Entities
{
    public class ServiceSchedule
    {
        [Key]
        public int ServiceScheduleId { get; set; }

        [Required]
        public int ServiceBookingId { get; set; }

        [Required]
        [StringLength(450)]
        public string MechanicId { get; set; } = string.Empty;

        [Required]
        public DateTime ScheduledDate { get; set; }

        [Required]
        public TimeSpan ScheduledTime { get; set; }

        [StringLength(20)]
        public string? ServiceBay { get; set; }

        public DateTime? EstimatedCompletion { get; set; }

        [Required]
        [StringLength(50)]
        public string ScheduleStatus { get; set; } = "Scheduled"; // Scheduled, InProgress, Completed, Cancelled

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        [ForeignKey("ServiceBookingId")]
        public virtual ServiceBooking? ServiceBooking { get; set; }

        [ForeignKey("MechanicId")]
        public virtual ApplicationUser? Mechanic { get; set; }
    }
}
