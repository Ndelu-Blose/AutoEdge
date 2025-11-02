using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AutoEdge.Models.Entities
{
    public class AssessmentAnswer
    {
        [Key]
        public int AssessmentAnswerId { get; set; }

        [Required]
        public int AssessmentQuestionId { get; set; }

        [Required]
        public int ApplicationId { get; set; }

        [Required]
        [StringLength(2000)]
        public string AnswerText { get; set; } = string.Empty;

        public decimal? Score { get; set; } // Score for this specific answer

        [StringLength(500)]
        public string? GradingNotes { get; set; }

        public DateTime SubmittedDate { get; set; } = DateTime.UtcNow;

        public DateTime ModifiedDate { get; set; } = DateTime.UtcNow;

        // Navigation properties
        [ForeignKey("AssessmentQuestionId")]
        public virtual AssessmentQuestion AssessmentQuestion { get; set; } = null!;

        [ForeignKey("ApplicationId")]
        public virtual Application Application { get; set; } = null!;
    }
}
