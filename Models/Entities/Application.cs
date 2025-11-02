using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AutoEdge.Models.Entities
{
    public class Application
    {
        [Key]
        public int ApplicationId { get; set; }

        [Required]
        public int JobId { get; set; }

        [StringLength(450)]
        public string? UserId { get; set; } // Link to ApplicationUser

        [Required]
        [StringLength(50)]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string LastName { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string Email { get; set; } = string.Empty;

        [Required]
        [StringLength(20)]
        public string PhoneNumber { get; set; } = string.Empty;

        [Required]
        [StringLength(500)]
        public string Address { get; set; } = string.Empty;

        [StringLength(500)]
        public string ResumeFilePath { get; set; } = string.Empty;

        [StringLength(500)]
        public string CoverLetterPath { get; set; } = string.Empty;

        [StringLength(500)]
        public string IdDocumentPath { get; set; } = string.Empty;

        [StringLength(500)]
        public string CertificatesPath { get; set; } = string.Empty;

        public string ParsedResumeText { get; set; } = string.Empty;

        public int YearsOfExperience { get; set; }

        public string ExtractedSkills { get; set; } = string.Empty;

        public string ExtractedEducation { get; set; } = string.Empty;

        public decimal MatchScore { get; set; } = 0;

        [Required]
        [StringLength(50)]
        public string Status { get; set; } = "Submitted"; // Submitted, Under Review, Shortlisted, Interview Scheduled, Assessment Sent, Final Decision

        public DateTime SubmittedDate { get; set; } = DateTime.UtcNow;

        public DateTime? ReviewedDate { get; set; }

        public string AdminNotes { get; set; } = string.Empty;

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        public DateTime ModifiedDate { get; set; } = DateTime.UtcNow;

        public bool IsActive { get; set; } = true;

        [StringLength(1000)]
        public string WhySuitableForRole { get; set; } = string.Empty;

        [StringLength(100)]
        public string HighestQualification { get; set; } = string.Empty;

        // Navigation properties
        [ForeignKey("JobId")]
        public virtual JobPosting? JobPosting { get; set; }

        [ForeignKey("UserId")]
        public virtual ApplicationUser? User { get; set; }

        public virtual ICollection<Interview> Interviews { get; set; } = new List<Interview>();
        public virtual ICollection<Assessment> Assessments { get; set; } = new List<Assessment>();
        public virtual ICollection<AssessmentAnswer> AssessmentAnswers { get; set; } = new List<AssessmentAnswer>();
        public virtual ICollection<RecruiterAssignment> RecruiterAssignments { get; set; } = new List<RecruiterAssignment>();
    }
}
