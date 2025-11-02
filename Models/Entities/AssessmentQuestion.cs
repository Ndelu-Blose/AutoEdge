using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AutoEdge.Models.Entities
{
    public class AssessmentQuestion
    {
        [Key]
        public int AssessmentQuestionId { get; set; }

        [Required]
        public int AssessmentId { get; set; }

        [Required]
        public int QuestionId { get; set; }

        [Required]
        public int Order { get; set; } // Order of questions in the assessment

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        // Navigation properties
        [ForeignKey("AssessmentId")]
        public virtual Assessment Assessment { get; set; } = null!;

        [ForeignKey("QuestionId")]
        public virtual Question Question { get; set; } = null!;

        public virtual ICollection<AssessmentAnswer> AssessmentAnswers { get; set; } = new List<AssessmentAnswer>();
    }
}
