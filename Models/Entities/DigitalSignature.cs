using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AutoEdge.Models.Entities
{
    public class DigitalSignature
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int ContractId { get; set; }

        [ForeignKey("ContractId")]
        public virtual Contract Contract { get; set; }

        [Required]
        [StringLength(50)]
        public string SignerName { get; set; }

        [Required]
        [StringLength(100)]
        public string SignerEmail { get; set; }

        [Required]
        public string SignatureData { get; set; } // Base64 encoded signature image data

        [Required]
        public DateTime SignedDate { get; set; }

        [StringLength(45)]
        public string IpAddress { get; set; }

        [StringLength(500)]
        public string UserAgent { get; set; }

        [Required]
        [StringLength(50)]
        public string SignatureType { get; set; } = "Canvas"; // Canvas, Touch, etc.

        [StringLength(100)]
        public string DocumentHash { get; set; } // For integrity verification

        public bool IsActive { get; set; } = true;

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        public DateTime? ModifiedDate { get; set; }
    }
}