using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AutoEdge.Models.Entities
{
    public class Assessment
    {
        [Key]
        public int AssessmentId { get; set; }

        [Required]
        public int ApplicationId { get; set; }

        [Required]
        [StringLength(200)]
        public string AssessmentTitle { get; set; } = string.Empty;

        [StringLength(1000)]
        public string Instructions { get; set; } = string.Empty;

        public DateTime SentDate { get; set; } = DateTime.UtcNow;

        [Required]
        public DateTime DueDate { get; set; }

        public DateTime? CompletedDate { get; set; }

        [Required]
        [StringLength(100)]
        public string AccessToken { get; set; } = string.Empty;

        public bool IsCompleted { get; set; } = false;

        public decimal Score { get; set; } = 0; // 0-100

        public bool EmailSent { get; set; } = false;

        public DateTime? EmailSentDate { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        public DateTime ModifiedDate { get; set; } = DateTime.UtcNow;

        public bool IsActive { get; set; } = true;

        [StringLength(50)]
        public string AssessmentType { get; set; } = string.Empty; // Mechanical Engineer, Sales Representative, Driver, Desktop Technician

        public int TimeLimitMinutes { get; set; } = 60;

        public bool IsPassed { get; set; } = false; // Based on 70% threshold

        [StringLength(1000)]
        public string GradingNotes { get; set; } = string.Empty;

        // Navigation properties
        [ForeignKey("ApplicationId")]
        public virtual Application? Application { get; set; }

        public virtual ICollection<AssessmentQuestion> AssessmentQuestions { get; set; } = new List<AssessmentQuestion>();
        public virtual ICollection<RecruiterAssignment> RecruiterAssignments { get; set; } = new List<RecruiterAssignment>();
    }
}
