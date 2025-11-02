using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AutoEdge.Models.Entities
{
    public class EmploymentOffer
    {
        [Key]
        public int OfferId { get; set; }

        [Required]
        public int ApplicationId { get; set; }

        [Required]
        [StringLength(200)]
        public string JobTitle { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string Department { get; set; } = string.Empty;

        [Column(TypeName = "decimal(18,2)")]
        public decimal SalaryOffered { get; set; }

        [Required]
        public DateTime StartDate { get; set; }

        [Required]
        [StringLength(50)]
        public string EmploymentType { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string WorkLocation { get; set; } = string.Empty;

        [StringLength(500)]
        public string? ContractFilePath { get; set; }

        public DateTime OfferSentDate { get; set; }

        public DateTime OfferExpiryDate { get; set; }

        [Required]
        [StringLength(40)]
        public string AccessToken { get; set; } = string.Empty;

        public bool ContractAccepted { get; set; }

        public DateTime? ContractAcceptedDate { get; set; }

        [Column(TypeName = "text")]
        public string? ContractSignature { get; set; }

        [StringLength(1000)]
        public string? RejectionReason { get; set; }

        [Required]
        [StringLength(50)]
        public string Status { get; set; } = "Pending";

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedDate { get; set; }

        public bool IsActive { get; set; } = true;

        // Navigation properties
        [ForeignKey("ApplicationId")]
        public virtual Application Application { get; set; } = null!;

        public virtual PreEmploymentDocumentation? PreEmploymentDocumentation { get; set; }
    }
}
