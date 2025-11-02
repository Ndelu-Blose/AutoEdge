using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AutoEdge.Models.Entities
{
    public class Customer
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; } = string.Empty;

        [StringLength(20)]
        public string CustomerType { get; set; } = "Individual"; // Individual, Business

        public int? CreditScore { get; set; }

        [StringLength(20)]
        public string PreferredContact { get; set; } = "Email"; // Email, Phone, SMS

        [StringLength(500)]
        public string DeliveryAddress { get; set; } = string.Empty;

        [StringLength(500)]
        public string BillingAddress { get; set; } = string.Empty;

        [StringLength(100)]
        public string EmergencyContactName { get; set; } = string.Empty;

        [StringLength(20)]
        public string EmergencyContactPhone { get; set; } = string.Empty;

        [StringLength(100)]
        public string EmergencyContactRelation { get; set; } = string.Empty;

        [StringLength(100)]
        public string Employer { get; set; } = string.Empty;

        [StringLength(100)]
        public string JobTitle { get; set; } = string.Empty;

        [Column(TypeName = "decimal(18,2)")]
        public decimal? AnnualIncome { get; set; }

        [StringLength(1000)]
        public string Notes { get; set; } = string.Empty;

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        public DateTime ModifiedDate { get; set; } = DateTime.UtcNow;

        public bool IsActive { get; set; } = true;

        [StringLength(20)]
        public string CustomerStatus { get; set; } = "Active"; // Active, Inactive, Blacklisted

        public DateTime? LastPurchaseDate { get; set; }

        public int TotalPurchases { get; set; } = 0;

        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalSpent { get; set; } = 0;

        // Navigation properties
        [ForeignKey("UserId")]
        public virtual ApplicationUser User { get; set; } = null!;
        
        public virtual ICollection<Inquiry> Inquiries { get; set; } = new List<Inquiry>();
        public virtual ICollection<Reservation> Reservations { get; set; } = new List<Reservation>();
        public virtual ICollection<Document> Documents { get; set; } = new List<Document>();
        public virtual ICollection<Contract> Contracts { get; set; } = new List<Contract>();
        public virtual ICollection<Purchase> Purchases { get; set; } = new List<Purchase>();
    }
}