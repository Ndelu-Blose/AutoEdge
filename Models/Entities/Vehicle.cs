using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AutoEdge.Models.Entities
{
    public class Vehicle
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(17)]
        public string VIN { get; set; } = string.Empty;

        [StringLength(20)]
        public string LicensePlate { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string Make { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string Model { get; set; } = string.Empty;

        [Required]
        public int Year { get; set; }

        public int Mileage { get; set; }

        [StringLength(50)]
        public string EngineType { get; set; } = string.Empty;

        [StringLength(50)]
        public string Transmission { get; set; } = string.Empty;

        [StringLength(30)]
        public string InteriorColor { get; set; } = string.Empty;

        [StringLength(30)]
        public string ExteriorColor { get; set; } = string.Empty;

        [Required]
        [StringLength(20)]
        public string Condition { get; set; } = string.Empty; // New, Used, Certified Pre-owned

        [Column(TypeName = "nvarchar(max)")]
        public string Features { get; set; } = string.Empty; // JSON string for features

        [Column(TypeName = "decimal(18,2)")]
        public decimal CostPrice { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal SellingPrice { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal NegotiableRange { get; set; }

        [Required]
        [StringLength(20)]
        public string Status { get; set; } = "Available"; // Available, Reserved, Sold, Under Maintenance

        [StringLength(100)]
        public string Location { get; set; } = string.Empty;

        public DateTime DateListed { get; set; } = DateTime.UtcNow;

        [StringLength(100)]
        public string Source { get; set; } = string.Empty; // trade-in, auction, etc.

        public DateTime PurchaseDate { get; set; }

        public string CreatedBy { get; set; } = string.Empty;

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        public DateTime ModifiedDate { get; set; } = DateTime.UtcNow;

        public bool IsActive { get; set; } = true;

        public bool IsDeleted { get; set; } = false;

        // Navigation properties
        public virtual ICollection<VehicleImage> VehicleImages { get; set; } = new List<VehicleImage>();
        public virtual ICollection<Inquiry> Inquiries { get; set; } = new List<Inquiry>();
        public virtual ICollection<Reservation> Reservations { get; set; } = new List<Reservation>();
        public virtual ICollection<Contract> Contracts { get; set; } = new List<Contract>();
        public virtual ICollection<Purchase> Purchases { get; set; } = new List<Purchase>();
    }
}