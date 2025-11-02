using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AutoEdge.Models.Entities
{
    public enum QuestionType
    {
        MultipleChoice,
        TrueFalse,
        ShortAnswer,
        Essay
    }

    public class Question
    {
        [Key]
        public int QuestionId { get; set; }

        [Required]
        [StringLength(50)]
        public string QuestionCode { get; set; } = string.Empty;

        [Required]
        [StringLength(200)]
        public string QuestionText { get; set; } = string.Empty;

        [Required]
        public QuestionType Type { get; set; }

        [StringLength(1000)]
        public string? Options { get; set; } // JSON array for multiple choice options

        [StringLength(500)]
        public string? CorrectAnswer { get; set; }

        [Required]
        public int Points { get; set; } = 10;

        [StringLength(50)]
        public string Category { get; set; } = string.Empty; // Technical, Behavioral, etc.

        [StringLength(50)]
        public string Department { get; set; } = string.Empty; // Engineering, Sales, etc.

        public bool IsActive { get; set; } = true;

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        public DateTime ModifiedDate { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public virtual ICollection<AssessmentQuestion> AssessmentQuestions { get; set; } = new List<AssessmentQuestion>();
    }
}
