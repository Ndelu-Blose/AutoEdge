using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AutoEdge.Models.Entities
{
    public class Reservation
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int VehicleId { get; set; }

        [Required]
        public int CustomerId { get; set; }

        [Required]
        [StringLength(50)]
        public string ReservationNumber { get; set; } = string.Empty;

        public DateTime ReservationDate { get; set; } = DateTime.UtcNow;

        public DateTime ExpiryDate { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal DepositAmount { get; set; }

        [StringLength(20)]
        public string Status { get; set; } = "Active"; // Active, Expired, Converted, Cancelled

        [StringLength(1000)]
        public string Notes { get; set; } = string.Empty;

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        public DateTime ModifiedDate { get; set; } = DateTime.UtcNow;

        public bool IsActive { get; set; } = true;

        [StringLength(50)]
        public string PaymentMethod { get; set; } = string.Empty;

        [StringLength(100)]
        public string PaymentTransactionId { get; set; } = string.Empty;

        public DateTime? PaymentDate { get; set; }

        [StringLength(20)]
        public string PaymentStatus { get; set; } = "Pending"; // Pending, Completed, Failed, Refunded

        [Column(TypeName = "decimal(18,2)")]
        public decimal? RefundAmount { get; set; }

        public DateTime? RefundDate { get; set; }

        [StringLength(500)]
        public string RefundReason { get; set; } = string.Empty;

        public DateTime? ConvertedToContractDate { get; set; }

        public int? ContractId { get; set; }

        [StringLength(500)]
        public string CancellationReason { get; set; } = string.Empty;

        public DateTime? CancellationDate { get; set; }

        public string? CancelledByUserId { get; set; }

        // Navigation properties
        [ForeignKey("VehicleId")]
        public virtual Vehicle Vehicle { get; set; } = null!;

        [ForeignKey("CustomerId")]
        public virtual Customer Customer { get; set; } = null!;

        [ForeignKey("ContractId")]
        public virtual Contract? Contract { get; set; }

        [ForeignKey("CancelledByUserId")]
        public virtual ApplicationUser? CancelledBy { get; set; }
    }
}