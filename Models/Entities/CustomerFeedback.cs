using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AutoEdge.Models.Entities
{
    public class CustomerFeedback
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        public int ContractId { get; set; }
        
        [ForeignKey("ContractId")]
        public virtual Contract Contract { get; set; } = null!;
        
        [Range(1, 5)]
        public int Rating { get; set; }
        
        [Range(1, 5)]
        public int ServiceRating { get; set; }
        
        [Range(1, 5)]
        public int VehicleRating { get; set; }
        
        [StringLength(2000)]
        public string? Comments { get; set; }
        
        public bool RecommendToOthers { get; set; }
        
        [StringLength(500)]
        public string? ImprovementSuggestions { get; set; }
        
        public bool AllowFollowUp { get; set; }
        
        public DateTime SubmittedDate { get; set; } = DateTime.Now;
        
        public DateTime? LastUpdatedDate { get; set; }
        
        public bool IsActive { get; set; } = true;
    }
}