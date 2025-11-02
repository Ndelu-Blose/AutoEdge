using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AutoEdge.Models.Entities
{
    public class Contract
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int VehicleId { get; set; }

        [Required]
        public int CustomerId { get; set; }

        [Required]
        [StringLength(50)]
        public string ContractNumber { get; set; } = string.Empty;

        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalAmount { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal DownPayment { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal FinancedAmount { get; set; }

        [StringLength(20)]
        public string PaymentType { get; set; } = string.Empty; // Cash, Financing, Lease

        [Column(TypeName = "nvarchar(max)")]
        public string LoanDetails { get; set; } = string.Empty; // JSON for loan terms

        [StringLength(20)]
        public string Status { get; set; } = "Draft"; // Draft, Pending, Signed, Completed, Cancelled

        public DateTime? SignedDate { get; set; }

        [StringLength(500)]
        public string ContractPath { get; set; } = string.Empty;

        [StringLength(500)]
        public string SignedContractPath { get; set; } = string.Empty;

        public string CreatedByUserId { get; set; } = string.Empty;

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        public DateTime ModifiedDate { get; set; } = DateTime.UtcNow;

        public bool IsActive { get; set; } = true;

        [Column(TypeName = "decimal(5,4)")]
        public decimal? InterestRate { get; set; }

        public int? LoanTermMonths { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal? MonthlyPayment { get; set; }

        [StringLength(100)]
        public string LenderName { get; set; } = string.Empty;

        [StringLength(100)]
        public string LoanNumber { get; set; } = string.Empty;

        [Column(TypeName = "decimal(18,2)")]
        public decimal? TradeInValue { get; set; }

        [StringLength(100)]
        public string TradeInVehicle { get; set; } = string.Empty;

        [Column(TypeName = "decimal(18,2)")]
        public decimal TaxAmount { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal RegistrationFee { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal DocumentationFee { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal ExtendedWarrantyFee { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal OtherFees { get; set; }

        [StringLength(1000)]
        public string SpecialTerms { get; set; } = string.Empty;

        [StringLength(1000)]
        public string Notes { get; set; } = string.Empty;

        // Foreign key to Purchase
        public int? PurchaseId { get; set; }

        public DateTime? DeliveryDate { get; set; }

        [StringLength(500)]
        public string DeliveryAddress { get; set; } = string.Empty;

        [StringLength(20)]
        public string DeliveryMethod { get; set; } = string.Empty; // Pickup, HomeDelivery, ThirdParty

        public bool IsDigitallySigned { get; set; } = false;

        public string DigitalSignatureData { get; set; } = string.Empty;

        // OpenSign Integration Fields
        [StringLength(100)]
        public string? OpenSignDocumentId { get; set; } = string.Empty;

        [StringLength(100)]
        public string? SigningRequestId { get; set; } = string.Empty;

        [StringLength(1000)]
        public string? SigningUrl { get; set; } = string.Empty;

        [StringLength(1000)]
        public string? SignedDocumentUrl { get; set; } = string.Empty;

        [StringLength(1000)]
        public string? CertificateUrl { get; set; } = string.Empty;

        public DateTime? SigningRequestSentDate { get; set; }

        public DateTime? SigningCompletedDate { get; set; }

        [StringLength(50)]
        public string? SigningStatus { get; set; } = "NotSent"; // NotSent, Sent, Viewed, Signed, Completed, Declined

        public DateTime? CompletionDate { get; set; }

        // Navigation properties
        [ForeignKey("VehicleId")]
        public virtual Vehicle Vehicle { get; set; } = null!;

        [ForeignKey("CustomerId")]
        public virtual Customer Customer { get; set; } = null!;

        [ForeignKey("CreatedByUserId")]
        public virtual ApplicationUser CreatedBy { get; set; } = null!;

        [ForeignKey("PurchaseId")]
        public virtual Purchase? Purchase { get; set; }

        public virtual ICollection<Payment> Payments { get; set; } = new List<Payment>();
        public virtual ICollection<Delivery> Deliveries { get; set; } = new List<Delivery>();
        public virtual ICollection<Warranty> Warranties { get; set; } = new List<Warranty>();
        public virtual ICollection<CustomerFeedback> CustomerFeedbacks { get; set; } = new List<CustomerFeedback>();
        public virtual ICollection<ServiceReminder> ServiceReminders { get; set; } = new List<ServiceReminder>();
        public virtual ICollection<DigitalSignature> DigitalSignatures { get; set; } = new List<DigitalSignature>();
    }
}