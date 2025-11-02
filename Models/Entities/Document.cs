using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AutoEdge.Models.Entities
{
    public class Document
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int CustomerId { get; set; }

        [Required]
        public int DocumentTypeId { get; set; }



        [Required]
        [StringLength(255)]
        public string FileName { get; set; } = string.Empty;

        [Required]
        [StringLength(500)]
        public string FilePath { get; set; } = string.Empty;

        public long FileSize { get; set; }
        
        public long FileSizeBytes => FileSize;

        [StringLength(100)]
        public string? ContentType { get; set; }

        public DateTime UploadDate { get; set; } = DateTime.UtcNow;

        [Required]
        [StringLength(50)]
        public string Status { get; set; } = "Pending"; // Pending, Approved, Rejected, Under Review

        [StringLength(1000)]
        public string? ValidationNotes { get; set; }

        public string? ReviewedBy { get; set; }

        public DateTime? ReviewDate { get; set; }

        public DateTime? ExpiryDate { get; set; }



        public bool IsActive { get; set; } = true;

        // OCR and validation fields
        [StringLength(2000)]
        public string? ExtractedText { get; set; }

        public bool IsOcrProcessed { get; set; } = false;
        public DateTime? OcrProcessedDate { get; set; }

        [StringLength(1000)]
        public string? OcrValidationResults { get; set; }

        [Column(TypeName = "nvarchar(max)")]
        public string Metadata { get; set; } = string.Empty; // JSON metadata

        [StringLength(100)]
        public string DocumentNumber { get; set; } = string.Empty; // License number, ID number, etc.

        public DateTime? DocumentIssueDate { get; set; }

        public DateTime? DocumentExpiryDate { get; set; }

        [StringLength(100)]
        public string IssuingAuthority { get; set; } = string.Empty;

        public bool IsRequired { get; set; } = false;

        public int DisplayOrder { get; set; } = 0;

        [StringLength(500)]
        public string Description { get; set; } = string.Empty;

        public bool IsConfidential { get; set; } = true;

        [StringLength(100)]
        public string EncryptionKey { get; set; } = string.Empty;

        // Metadata
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        public DateTime? ModifiedDate { get; set; }

        // Foreign Keys
        public int? PurchaseId { get; set; }

        // Navigation properties
        [ForeignKey("DocumentTypeId")]
        public virtual DocumentType DocumentType { get; set; } = null!;

        [ForeignKey("CustomerId")]
        public virtual Customer Customer { get; set; } = null!;

        [ForeignKey("ReviewedBy")]
        public virtual ApplicationUser? Reviewer { get; set; }

        [ForeignKey("PurchaseId")]
        public virtual Purchase? Purchase { get; set; }
    }
}