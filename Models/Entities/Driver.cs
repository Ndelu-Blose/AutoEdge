using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AutoEdge.Models.Entities
{
    public class Driver
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(450)]
        public string UserId { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [StringLength(20)]
        public string Phone { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string LicenseNumber { get; set; } = string.Empty;

        [Required]
        [StringLength(20)]
        public string VehicleRegistration { get; set; } = string.Empty;

        [StringLength(100)]
        public string VehicleMake { get; set; } = string.Empty;

        [StringLength(100)]
        public string VehicleModel { get; set; } = string.Empty;

        [StringLength(20)]
        public string VehicleColor { get; set; } = string.Empty;

        public bool IsAvailable { get; set; } = true;

        [Column(TypeName = "decimal(3,2)")]
        public decimal Rating { get; set; } = 5.0m;

        public int TotalPickups { get; set; } = 0;

        public int SuccessfulPickups { get; set; } = 0;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        [ForeignKey("UserId")]
        public virtual ApplicationUser User { get; set; } = null!;

        public virtual ICollection<VehiclePickup> VehiclePickups { get; set; } = new List<VehiclePickup>();
    }
}
