using System.ComponentModel.DataAnnotations;
using AutoEdge.Models.Entities;

namespace AutoEdge.Models.ViewModels
{
    public class CustomerFeedbackViewModel
    {
        public int ContractId { get; set; }
        
        [Display(Name = "Vehicle Information")]
        public string VehicleInfo { get; set; } = string.Empty;
        
        [Display(Name = "Purchase Date")]
        public DateTime PurchaseDate { get; set; }
        
        [Required]
        [Range(1, 5, ErrorMessage = "Please provide an overall rating from 1 to 5 stars")]
        [Display(Name = "Overall Rating")]
        public int Rating { get; set; }
        
        [Required]
        [Range(1, 5, ErrorMessage = "Please provide a service rating from 1 to 5 stars")]
        [Display(Name = "Service Quality Rating")]
        public int ServiceRating { get; set; }
        
        [Required]
        [Range(1, 5, ErrorMessage = "Please provide a vehicle rating from 1 to 5 stars")]
        [Display(Name = "Vehicle Quality Rating")]
        public int VehicleRating { get; set; }
        
        [Required]
        [Display(Name = "Comments")]
        [StringLength(2000, MinimumLength = 10, ErrorMessage = "Comments must be between 10 and 2000 characters")]
        public string Comments { get; set; } = string.Empty;
        
        [Display(Name = "Would you recommend AutoEdge to others?")]
        public bool RecommendToOthers { get; set; }
        
        [Display(Name = "Areas for Improvement (Optional)")]
        [StringLength(1000, ErrorMessage = "Improvement suggestions cannot exceed 1000 characters")]
        public string? ImprovementSuggestions { get; set; }
        
        [Display(Name = "Contact for Follow-up")]
        public bool AllowFollowUp { get; set; }
        
        public CustomerFeedback? ExistingFeedback { get; set; }
        
        public bool IsUpdate => ExistingFeedback != null;
    }
}