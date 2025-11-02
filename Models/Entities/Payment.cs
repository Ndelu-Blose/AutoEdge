using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AutoEdge.Models.Entities
{
    public class Payment
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int ContractId { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; }

        [Required]
        [StringLength(50)]
        public string PaymentMethod { get; set; } = string.Empty; // CreditCard, DebitCard, BankTransfer, Cash, Check, Financing

        [StringLength(100)]
        public string TransactionId { get; set; } = string.Empty;

        [StringLength(100)]
        public string StripePaymentIntentId { get; set; } = string.Empty;

        [StringLength(20)]
        public string Status { get; set; } = "Pending"; // Pending, Processing, Completed, Failed, Cancelled, Refunded

        public DateTime ProcessedDate { get; set; } = DateTime.UtcNow;

        [Column(TypeName = "nvarchar(max)")]
        public string GatewayResponse { get; set; } = string.Empty; // JSON response from payment gateway

        [Column(TypeName = "decimal(18,2)")]
        public decimal RefundAmount { get; set; } = 0;

        public DateTime? RefundDate { get; set; }

        [StringLength(500)]
        public string RefundReason { get; set; } = string.Empty;

        [StringLength(100)]
        public string RefundTransactionId { get; set; } = string.Empty;

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        public DateTime ModifiedDate { get; set; } = DateTime.UtcNow;

        public bool IsActive { get; set; } = true;

        [StringLength(50)]
        public string PaymentType { get; set; } = string.Empty; // DownPayment, MonthlyPayment, FullPayment, Deposit, Fee

        [StringLength(4)]
        public string CardLastFour { get; set; } = string.Empty;

        [StringLength(50)]
        public string CardBrand { get; set; } = string.Empty;

        [StringLength(100)]
        public string BankName { get; set; } = string.Empty;

        [StringLength(50)]
        public string CheckNumber { get; set; } = string.Empty;

        [Column(TypeName = "decimal(5,4)")]
        public decimal ProcessingFee { get; set; } = 0;

        [Column(TypeName = "decimal(18,2)")]
        public decimal NetAmount { get; set; }

        [StringLength(3)]
        public string Currency { get; set; } = "ZAR";

        [StringLength(1000)]
        public string Notes { get; set; } = string.Empty;

        [StringLength(500)]
        public string FailureReason { get; set; } = string.Empty;

        public int RetryCount { get; set; } = 0;

        public DateTime? NextRetryDate { get; set; }

        public bool IsRecurring { get; set; } = false;

        [StringLength(100)]
        public string RecurringScheduleId { get; set; } = string.Empty;

        public DateTime? DueDate { get; set; }

        public bool IsOverdue { get; set; } = false;

        [Column(TypeName = "decimal(18,2)")]
        public decimal LateFee { get; set; } = 0;

        [StringLength(500)]
        public string ReceiptPath { get; set; } = string.Empty;

        public bool ReceiptSent { get; set; } = false;

        public DateTime? ReceiptSentDate { get; set; }

        // Navigation properties
        [ForeignKey("ContractId")]
        public virtual Contract Contract { get; set; } = null!;
    }
}