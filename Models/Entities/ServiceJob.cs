using System.ComponentModel.DataAnnotations;

namespace AutoEdge.Models.Entities
{
    public class ServiceJob
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int ServiceBookingId { get; set; }
        public virtual ServiceBooking ServiceBooking { get; set; } = null!;

        public int? MechanicId { get; set; }
        public virtual Mechanic? Mechanic { get; set; }

        public DateOnly ScheduledDate { get; set; }
        public TimeOnly ScheduledStart { get; set; }
        public int DurationMin { get; set; }
        public bool IsCompleted { get; set; } = false;

        // Navigation property for ServiceChecklist
        public virtual ServiceChecklist? ServiceChecklist { get; set; }
    }
} 