using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AutoEdge.Models.Entities
{
    public class PreEmploymentDocumentation
    {
        [Key]
        public int DocumentationId { get; set; }

        [Required]
        public int OfferId { get; set; }

        // Personal Information
        [Required]
        [StringLength(13)]
        public string IdNumber { get; set; } = string.Empty;

        public DateTime DateOfBirth { get; set; }

        [Required]
        [StringLength(20)]
        public string Gender { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string Nationality { get; set; } = string.Empty;

        [Required]
        [StringLength(20)]
        public string MaritalStatus { get; set; } = string.Empty;

        [Required]
        [StringLength(500)]
        public string ResidentialAddress { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string City { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string Province { get; set; } = string.Empty;

        [Required]
        [StringLength(10)]
        public string PostalCode { get; set; } = string.Empty;

        [StringLength(20)]
        public string? AlternativePhoneNumber { get; set; }

        // Emergency Contact
        [Required]
        [StringLength(100)]
        public string EmergencyContactName { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string EmergencyContactRelationship { get; set; } = string.Empty;

        [Required]
        [StringLength(20)]
        public string EmergencyContactPhone { get; set; } = string.Empty;

        [StringLength(500)]
        public string? EmergencyContactAddress { get; set; }

        [StringLength(20)]
        public string? SecondaryEmergencyContactPhone { get; set; }

        [StringLength(100)]
        public string? SecondaryEmergencyContactName { get; set; }

        [StringLength(50)]
        public string? SecondaryEmergencyContactRelationship { get; set; }

        [StringLength(500)]
        public string? SecondaryEmergencyContactAddress { get; set; }

        // Banking Details
        [Required]
        [StringLength(50)]
        public string BankName { get; set; } = string.Empty;

        [Required]
        [StringLength(20)]
        public string AccountType { get; set; } = string.Empty;

        [Required]
        [StringLength(20)]
        public string AccountNumber { get; set; } = string.Empty;

        [Required]
        [StringLength(6)]
        public string BranchCode { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string AccountHolderName { get; set; } = string.Empty;

        [StringLength(500)]
        public string? BankStatementPath { get; set; }

        // Tax Information
        [StringLength(20)]
        public string? TaxNumber { get; set; }

        public bool RegisteredForTax { get; set; }

        [StringLength(500)]
        public string? TaxClearancePath { get; set; }

        [StringLength(50)]
        public string? TaxDirectiveNumber { get; set; }

        // Medical Information
        public bool HasMedicalAid { get; set; }

        [StringLength(100)]
        public string? MedicalAidProvider { get; set; }

        [StringLength(50)]
        public string? MedicalAidNumber { get; set; }

        [StringLength(20)]
        public string? MedicalAidMemberType { get; set; }

        public bool HasChronicConditions { get; set; }

        [StringLength(500)]
        public string? ChronicConditionsDetails { get; set; }

        public bool OnChronicMedication { get; set; }

        public bool HasDisabilities { get; set; }

        [StringLength(500)]
        public string? DisabilitiesDetails { get; set; }

        // Dependents
        public int NumberOfDependents { get; set; }

        [Column(TypeName = "text")]
        public string? DependentsDetails { get; set; } // JSON string

        // Document Paths
        [StringLength(500)]
        public string? CertifiedIdPath { get; set; }

        [StringLength(500)]
        public string? ProofOfAddressPath { get; set; }

        [Column(TypeName = "text")]
        public string? QualificationCertificatesPath { get; set; } // JSON array

        [StringLength(500)]
        public string? DriversLicensePath { get; set; }

        [Column(TypeName = "text")]
        public string? OptionalDocumentsPath { get; set; } // JSON array

        // Declarations
        public bool HasCriminalRecord { get; set; }

        [StringLength(1000)]
        public string? CriminalRecordDetails { get; set; }

        public bool DeclareAccurate { get; set; }

        public bool ConsentBackgroundCheck { get; set; }

        public bool ConsentDataProcessing { get; set; }

        public bool DocumentAuthenticity { get; set; }

        [Column(TypeName = "text")]
        public string? DigitalSignature { get; set; }

        public DateTime? SignedDate { get; set; }

        // Status and Review
        public bool IsCompleted { get; set; }

        public DateTime? CompletedDate { get; set; }

        public bool AdminReviewed { get; set; }

        public DateTime? AdminReviewedDate { get; set; }

        [StringLength(450)]
        public string? ReviewedBy { get; set; }

        public bool Approved { get; set; }

        [StringLength(1000)]
        public string? AdminNotes { get; set; }

        [StringLength(1000)]
        public string? CorrectionRequests { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedDate { get; set; }

        public bool IsActive { get; set; } = true;

        // Navigation properties
        [ForeignKey("OfferId")]
        public virtual EmploymentOffer EmploymentOffer { get; set; } = null!;

        [ForeignKey("ReviewedBy")]
        public virtual ApplicationUser? Reviewer { get; set; }
    }
}
