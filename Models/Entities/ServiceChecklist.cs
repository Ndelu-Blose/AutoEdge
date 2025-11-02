using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AutoEdge.Models.Entities
{
    public class ServiceChecklist
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int ServiceJobId { get; set; }

        [Required]
        public int MechanicId { get; set; }

        [Required]
        [StringLength(200)]
        public string ServiceType { get; set; } = string.Empty;

        [Required]
        public DateTime StartedAt { get; set; } = DateTime.UtcNow;

        public DateTime? CompletedAt { get; set; }

        [StringLength(1000)]
        public string? Notes { get; set; }

        public bool IsCompleted { get; set; } = false;

        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalEstimatedCost { get; set; } = 0m;

        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalActualCost { get; set; } = 0m;

        public int TotalEstimatedDurationMinutes { get; set; } = 0;

        public int TotalActualDurationMinutes { get; set; } = 0;

        public int CompletedItemsCount { get; set; } = 0;

        public int TotalItemsCount { get; set; } = 0;

        public double ProgressPercentage => TotalItemsCount > 0 ? (double)CompletedItemsCount / TotalItemsCount * 100 : 0;

        [Required]
        public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

        [StringLength(50)]
        public string CreatedBy { get; set; } = string.Empty;

        // Navigation properties
        public virtual ServiceJob ServiceJob { get; set; } = null!;
        public virtual Mechanic Mechanic { get; set; } = null!;
        public virtual ICollection<ServiceChecklistItem> Items { get; set; } = new List<ServiceChecklistItem>();
        public virtual ICollection<ServicePhoto> Photos { get; set; } = new List<ServicePhoto>();
    }

    public class ServiceChecklistItem
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int ServiceChecklistId { get; set; }

        [Required]
        [StringLength(200)]
        public string TaskName { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Description { get; set; }

        public bool IsCompleted { get; set; } = false;

        public DateTime? CompletedAt { get; set; }

        [StringLength(100)]
        public string? CompletedBy { get; set; }

        [StringLength(500)]
        public string? Notes { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal? EstimatedCost { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal? ActualCost { get; set; }

        public int? EstimatedDurationMinutes { get; set; }

        public int? ActualDurationMinutes { get; set; }

        [Required]
        public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public virtual ServiceChecklist ServiceChecklist { get; set; } = null!;
    }

    public class ServicePhoto
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int ServiceChecklistId { get; set; }

        [Required]
        [StringLength(200)]
        public string PhotoType { get; set; } = string.Empty; // "Before", "After", "During"

        [Required]
        [StringLength(500)]
        public string FilePath { get; set; } = string.Empty;

        [StringLength(200)]
        public string? FileName { get; set; }

        [StringLength(1000)]
        public string? Description { get; set; }

        [Required]
        public DateTime TakenAt { get; set; } = DateTime.UtcNow;

        [StringLength(50)]
        public string TakenBy { get; set; } = string.Empty;

        [Required]
        public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public virtual ServiceChecklist ServiceChecklist { get; set; } = null!;
    }

    public enum ServicePhotoType
    {
        Before = 1,
        During = 2,
        After = 3
    }
}
