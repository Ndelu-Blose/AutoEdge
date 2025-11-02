using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AutoEdge.Models.Entities
{
    public class ServiceInvoice
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int ServiceBookingId { get; set; }

        [Required]
        [StringLength(50)]
        public string InvoiceNumber { get; set; } = string.Empty;

        [Required]
        public DateTime IssueDate { get; set; } = DateTime.UtcNow;

        [Required]
        public DateTime DueDate { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal SubTotal { get; set; } = 0m;

        [Column(TypeName = "decimal(10,2)")]
        public decimal TaxAmount { get; set; } = 0m;

        [Column(TypeName = "decimal(10,2)")]
        public decimal TotalAmount { get; set; } = 0m;

        [Required]
        [StringLength(20)]
        public string Status { get; set; } = "Draft"; // Draft, Sent, Paid, Overdue, Cancelled

        [StringLength(500)]
        public string? PDFPath { get; set; }

        [StringLength(1000)]
        public string? Notes { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        [ForeignKey("ServiceBookingId")]
        public virtual ServiceBooking ServiceBooking { get; set; } = null!;

        public virtual ICollection<ServicePayment> ServicePayments { get; set; } = new List<ServicePayment>();
    }
}