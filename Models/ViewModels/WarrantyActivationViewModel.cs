using System.ComponentModel.DataAnnotations;
using AutoEdge.Models.Entities;

namespace AutoEdge.Models.ViewModels
{
    public class WarrantyActivationViewModel
    {
        public int ContractId { get; set; }
        
        [Display(Name = "Vehicle Information")]
        public string VehicleInfo { get; set; } = string.Empty;
        
        [Display(Name = "VIN")]
        public string VIN { get; set; } = string.Empty;
        
        [Display(Name = "Purchase Date")]
        public DateTime PurchaseDate { get; set; }
        
        [Required]
        [Display(Name = "Warranty Type")]
        public string WarrantyType { get; set; } = string.Empty;
        
        [Required]
        [Range(1, 10)]
        [Display(Name = "Duration (Years)")]
        public int DurationYears { get; set; } = 3;
        
        [Required]
        [Display(Name = "Coverage Details")]
        [StringLength(1000, ErrorMessage = "Coverage details cannot exceed 1000 characters")]
        public string CoverageDetails { get; set; } = string.Empty;
        
        [Display(Name = "Terms and Conditions")]
        [StringLength(2000, ErrorMessage = "Terms cannot exceed 2000 characters")]
        public string Terms { get; set; } = string.Empty;
        
        [Display(Name = "I agree to the warranty terms and conditions")]
        [Range(typeof(bool), "true", "true", ErrorMessage = "You must agree to the terms and conditions")]
        public bool AgreeToTerms { get; set; }
        
        public List<Warranty> ExistingWarranties { get; set; } = new List<Warranty>();
        
        public List<string> AvailableWarrantyTypes { get; set; } = new List<string>
        {
            "Basic Warranty",
            "Extended Warranty",
            "Powertrain Warranty",
            "Bumper-to-Bumper Warranty",
            "Corrosion Warranty",
            "Emission Warranty"
        };
    }
}