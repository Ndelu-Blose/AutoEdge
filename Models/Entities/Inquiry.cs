using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AutoEdge.Models.Entities
{
    public class Inquiry
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int VehicleId { get; set; }

        [Required]
        public int CustomerId { get; set; }

        [Required]
        [StringLength(50)]
        public string InquiryType { get; set; } = string.Empty; // Information, Reservation, Purchase, TestDrive

        [Required]
        [StringLength(1000)]
        public string Message { get; set; } = string.Empty;

        [StringLength(20)]
        public string Status { get; set; } = "Open"; // Open, InProgress, Resolved, Closed

        [StringLength(20)]
        public string Priority { get; set; } = "Medium"; // Low, Medium, High, Urgent

        public string? AssignedToUserId { get; set; }

        [StringLength(1000)]
        public string Response { get; set; } = string.Empty;

        public DateTime? ResponseDate { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        public DateTime ModifiedDate { get; set; } = DateTime.UtcNow;

        public bool IsActive { get; set; } = true;

        [StringLength(100)]
        public string CustomerName { get; set; } = string.Empty;

        [StringLength(100)]
        public string CustomerEmail { get; set; } = string.Empty;

        [StringLength(20)]
        public string CustomerPhone { get; set; } = string.Empty;

        [StringLength(20)]
        public string PreferredContactMethod { get; set; } = "Email";

        public DateTime? PreferredContactTime { get; set; }

        [StringLength(500)]
        public string SpecialRequests { get; set; } = string.Empty;

        public bool IsTestDriveRequested { get; set; } = false;

        public DateTime? PreferredTestDriveDate { get; set; }

        public bool IsFinancingInquiry { get; set; } = false;

        [Column(TypeName = "decimal(18,2)")]
        public decimal? OfferedPrice { get; set; }

        // Navigation properties
        [ForeignKey("VehicleId")]
        public virtual Vehicle Vehicle { get; set; } = null!;

        [ForeignKey("CustomerId")]
        public virtual Customer Customer { get; set; } = null!;

        [ForeignKey("AssignedToUserId")]
        public virtual ApplicationUser? AssignedTo { get; set; }
    }
}