using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AutoEdge.Models.Entities
{
    public class Warranty
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        public int ContractId { get; set; }
        
        [ForeignKey("ContractId")]
        public virtual Contract Contract { get; set; } = null!;
        
        [Required]
        [StringLength(100)]
        public string WarrantyType { get; set; } = string.Empty;
        
        [Required]
        public int DurationMonths { get; set; }
        
        [Required]
        public int DurationKilometers { get; set; }
        
        [Required]
        [StringLength(1000)]
        public string CoverageDetails { get; set; } = string.Empty;
        
        [Required]
        [StringLength(2000)]
        public string Terms { get; set; } = string.Empty;
        
        [Required]
        public DateTime StartDate { get; set; }
        
        [Required]
        public DateTime EndDate { get; set; }
        
        public bool IsActive { get; set; } = true;
        
        public DateTime CreatedDate { get; set; } = DateTime.Now;
        
        public DateTime? LastUpdatedDate { get; set; }
        
        [StringLength(50)]
        public string Status { get; set; } = "Active";
        
        [StringLength(500)]
        public string? Notes { get; set; }
    }
}