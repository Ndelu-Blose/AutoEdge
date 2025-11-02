using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AutoEdge.Models.Entities
{
    public class JobPosting
    {
        [Key]
        public int JobId { get; set; }

        [Required]
        [StringLength(200)]
        public string JobTitle { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string Department { get; set; } = string.Empty; // Mechanical Engineer, Sales Representative, Driver, Desktop Technician

        [Required]
        public string JobDescription { get; set; } = string.Empty;

        [Required]
        public string Requirements { get; set; } = string.Empty;

        [Required]
        public string Responsibilities { get; set; } = string.Empty;

        [Required]
        public int MinYearsExperience { get; set; }

        [Required]
        public string RequiredQualifications { get; set; } = string.Empty;

        [Required]
        public int PositionsAvailable { get; set; }

        public DateTime PostedDate { get; set; } = DateTime.UtcNow;

        public DateTime ClosingDate { get; set; }

        [Required]
        [StringLength(20)]
        public string Status { get; set; } = "Draft"; // Active, Closed, Draft

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        public DateTime ModifiedDate { get; set; } = DateTime.UtcNow;

        [StringLength(450)]
        public string CreatedByUserId { get; set; } = string.Empty;

        public bool IsActive { get; set; } = true;

        [StringLength(1000)]
        public string Notes { get; set; } = string.Empty;

        // Navigation properties
        [ForeignKey("CreatedByUserId")]
        public virtual ApplicationUser? CreatedBy { get; set; }

        public virtual ICollection<Application> Applications { get; set; } = new List<Application>();
    }
}
