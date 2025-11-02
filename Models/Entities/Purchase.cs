using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AutoEdge.Models.Entities
{
    public class Purchase
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int CustomerId { get; set; }

        [Required]
        public int VehicleId { get; set; }

        [Required]
        [StringLength(50)]
        public string Status { get; set; } = "Initiated"; // Initiated, DocumentsUploaded, DocumentsVerified, ContractGenerated, ContractSentForSigning, ContractSigned, PaymentPending, PaymentCompleted, Completed, Cancelled

        [Column(TypeName = "decimal(18,2)")]
        public decimal PurchasePrice { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal DepositAmount { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal RemainingAmount { get; set; }

        [StringLength(50)]
        public string PaymentMethod { get; set; } = string.Empty; // Cash, Financing, CreditCard, BankTransfer

        [StringLength(100)]
        public string FinancingProvider { get; set; } = string.Empty;

        [Column(TypeName = "decimal(5,2)")]
        public decimal? InterestRate { get; set; }

        public int? LoanTermMonths { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal? MonthlyPayment { get; set; }

        public DateTime InitiatedDate { get; set; } = DateTime.UtcNow;

        public DateTime? DocumentsSubmittedDate { get; set; }

        public DateTime? DocumentsVerifiedDate { get; set; }

        public DateTime? ContractGeneratedDate { get; set; }

        public DateTime? ContractSignedDate { get; set; }

        public DateTime? PaymentCompletedDate { get; set; }

        public DateTime? CompletedDate { get; set; }

        public DateTime? CancelledDate { get; set; }

        [StringLength(500)]
        public string Notes { get; set; } = string.Empty;

        [StringLength(500)]
        public string CancellationReason { get; set; } = string.Empty;

        [StringLength(100)]
        public string AssignedSalesRep { get; set; } = string.Empty;

        public bool RequiresFinancing { get; set; } = false;

        public bool DocumentsVerified { get; set; } = false;

        public bool ContractSigned { get; set; } = false;

        public bool PaymentCompleted { get; set; } = false;

        [StringLength(100)]
        public string DeliveryMethod { get; set; } = string.Empty; // Pickup, HomeDelivery

        [StringLength(500)]
        public string DeliveryAddress { get; set; } = string.Empty;

        public DateTime? ScheduledDeliveryDate { get; set; }

        public DateTime? ActualDeliveryDate { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        public DateTime ModifiedDate { get; set; } = DateTime.UtcNow;

        public bool IsActive { get; set; } = true;

        public bool IsDeleted { get; set; } = false;

        // Navigation properties
        [ForeignKey("CustomerId")]
        public virtual Customer Customer { get; set; } = null!;
        [ForeignKey("VehicleId")]
        public virtual Vehicle Vehicle { get; set; } = null!;
        public virtual ICollection<PurchaseStatusHistory> StatusHistory { get; set; } = new List<PurchaseStatusHistory>();
        public virtual ICollection<Document> Documents { get; set; } = new List<Document>();
        public virtual ICollection<Contract> Contracts { get; set; } = new List<Contract>();
    }
}