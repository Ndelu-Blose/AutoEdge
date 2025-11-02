using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AutoEdge.Models.Entities
{
    public class InterviewSlot
    {
        [Key]
        public int SlotId { get; set; }

        [Required]
        public DateTime StartTime { get; set; }

        [Required]
        public DateTime EndTime { get; set; }

        public bool IsBooked { get; set; } = false;

        public int? InterviewId { get; set; }

        [Required]
        [StringLength(100)]
        public string RecruiterName { get; set; } = string.Empty;

        [StringLength(100)]
        public string RecruiterEmail { get; set; } = string.Empty;

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        public DateTime ModifiedDate { get; set; } = DateTime.UtcNow;

        public bool IsActive { get; set; } = true;

        [StringLength(200)]
        public string Notes { get; set; } = string.Empty;

        // Navigation properties
        [ForeignKey("InterviewId")]
        public virtual Interview? Interview { get; set; }
    }
}
