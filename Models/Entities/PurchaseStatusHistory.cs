using System.ComponentModel.DataAnnotations;

namespace AutoEdge.Models.Entities
{
    public class PurchaseStatusHistory
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int PurchaseId { get; set; }

        [Required]
        [StringLength(50)]
        public string FromStatus { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string ToStatus { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string Status { get; set; } = string.Empty;

        [StringLength(500)]
        public string Notes { get; set; } = string.Empty;

        [StringLength(100)]
        public string ChangedBy { get; set; } = string.Empty;

        [StringLength(50)]
        public string ChangeReason { get; set; } = string.Empty;

        public DateTime ChangedDate { get; set; } = DateTime.UtcNow;

        public bool IsSystemGenerated { get; set; } = false;

        // Navigation properties
        public virtual Purchase Purchase { get; set; } = null!;
    }
}