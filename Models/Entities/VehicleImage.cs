using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AutoEdge.Models.Entities
{
    public class VehicleImage
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int VehicleId { get; set; }

        [Required]
        [StringLength(500)]
        public string ImagePath { get; set; } = string.Empty;

        [StringLength(50)]
        public string ImageType { get; set; } = "Gallery"; // Primary, Gallery, Interior, Exterior

        public DateTime UploadDate { get; set; } = DateTime.UtcNow;

        public int DisplayOrder { get; set; } = 0;

        public bool IsActive { get; set; } = true;

        [StringLength(100)]
        public string FileName { get; set; } = string.Empty;

        public long FileSize { get; set; }

        [StringLength(100)]
        public string ContentType { get; set; } = string.Empty;

        [StringLength(500)]
        public string AltText { get; set; } = string.Empty;

        // Navigation properties
        [ForeignKey("VehicleId")]
        public virtual Vehicle Vehicle { get; set; } = null!;
    }
}