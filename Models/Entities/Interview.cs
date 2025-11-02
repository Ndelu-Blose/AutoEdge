using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AutoEdge.Models.Entities
{
    public class Interview
    {
        [Key]
        public int InterviewId { get; set; }

        [Required]
        public int ApplicationId { get; set; }

        [Required]
        public int JobId { get; set; }

        [Required]
        public DateTime ScheduledDateTime { get; set; }

        [Required]
        public int DurationMinutes { get; set; } = 60;

        [StringLength(500)]
        public string MeetingLink { get; set; } = string.Empty;

        [StringLength(100)]
        public string MeetingPassword { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string RecruiterName { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string RecruiterEmail { get; set; } = string.Empty;

        public bool IsCompleted { get; set; } = false;

        public DateTime? CompletedDate { get; set; }

        public string InterviewNotes { get; set; } = string.Empty;

        public int InterviewRating { get; set; } = 0; // 1-10 scale

        public bool EmailSent { get; set; } = false;

        public DateTime? EmailSentDate { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        public DateTime ModifiedDate { get; set; } = DateTime.UtcNow;

        public bool IsActive { get; set; } = true;

        [StringLength(1000)]
        public string AdditionalInstructions { get; set; } = string.Empty;

        [StringLength(50)]
        public string MeetingPlatform { get; set; } = string.Empty; // Zoom, Teams, Daily.co, etc.

        // Navigation properties
        [ForeignKey("ApplicationId")]
        public virtual Application? Application { get; set; }

        [ForeignKey("JobId")]
        public virtual JobPosting? JobPosting { get; set; }
    }
}
