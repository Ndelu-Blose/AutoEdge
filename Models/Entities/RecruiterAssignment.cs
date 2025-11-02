using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AutoEdge.Models.Entities
{
    public class RecruiterAssignment
    {
        [Key]
        public int RecruiterAssignmentId { get; set; }

        [Required]
        public int AssessmentId { get; set; }

        [Required]
        [StringLength(450)] // ASP.NET Identity User ID length
        public string RecruiterId { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string RecruiterName { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string RecruiterEmail { get; set; } = string.Empty;

        [Required]
        public int ApplicationId { get; set; }

        [StringLength(500)]
        public string? AssignmentNotes { get; set; }

        public DateTime AssignedDate { get; set; } = DateTime.UtcNow;

        public DateTime ModifiedDate { get; set; } = DateTime.UtcNow;

        public bool IsActive { get; set; } = true;

        // Navigation properties
        [ForeignKey("AssessmentId")]
        public virtual Assessment Assessment { get; set; } = null!;

        [ForeignKey("ApplicationId")]
        public virtual Application Application { get; set; } = null!;
    }
}
