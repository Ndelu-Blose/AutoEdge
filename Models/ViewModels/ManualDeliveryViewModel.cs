using System.ComponentModel.DataAnnotations;

namespace AutoEdge.Models.ViewModels
{
    public class ManualDeliveryViewModel
    {
        public int ContractId { get; set; }
        
        [Display(Name = "Vehicle Information")]
        public string VehicleInfo { get; set; } = string.Empty;
        
        [Display(Name = "VIN")]
        public string VIN { get; set; } = string.Empty;
        
        [Display(Name = "Customer Name")]
        public string CustomerName { get; set; } = string.Empty;
        
        [Display(Name = "Customer Email")]
        public string CustomerEmail { get; set; } = string.Empty;
        
        [Display(Name = "Customer Phone")]
        public string CustomerPhone { get; set; } = string.Empty;
        
        [Required]
        [Display(Name = "Preferred Delivery Date")]
        [DataType(DataType.DateTime)]
        public DateTime DeliveryDate { get; set; } = DateTime.Now.AddDays(1);
        
        [Required]
        [Display(Name = "Delivery Address")]
        [StringLength(200, ErrorMessage = "Address cannot exceed 200 characters")]
        public string DeliveryAddress { get; set; } = string.Empty;
        
        [Required]
        [Display(Name = "City")]
        [StringLength(100, ErrorMessage = "City cannot exceed 100 characters")]
        public string DeliveryCity { get; set; } = string.Empty;
        
        [Required]
        [Display(Name = "Province")]
        [StringLength(100, ErrorMessage = "Province cannot exceed 100 characters")]
        public string DeliveryProvince { get; set; } = string.Empty;
        
        [Required]
        [Display(Name = "Postal Code")]
        [StringLength(10, ErrorMessage = "Postal code cannot exceed 10 characters")]
        public string DeliveryPostalCode { get; set; } = string.Empty;
        
        [Required]
        [Display(Name = "Contact Person Name")]
        [StringLength(100, ErrorMessage = "Contact person name cannot exceed 100 characters")]
        public string ContactPersonName { get; set; } = string.Empty;
        
        [Required]
        [Display(Name = "Contact Person Phone")]
        [Phone(ErrorMessage = "Please enter a valid phone number")]
        public string ContactPersonPhone { get; set; } = string.Empty;
        
        [Required]
        [Display(Name = "Contact Person Email")]
        [EmailAddress(ErrorMessage = "Please enter a valid email address")]
        public string ContactPersonEmail { get; set; } = string.Empty;
        
        [Display(Name = "Special Instructions")]
        [StringLength(500, ErrorMessage = "Special instructions cannot exceed 500 characters")]
        public string? SpecialInstructions { get; set; }
        
        [Display(Name = "Preferred Time Slot")]
        public string? PreferredTimeSlot { get; set; }
        
        [Display(Name = "Requires Signature")]
        public bool RequiresSignature { get; set; } = true;
        
        [Display(Name = "I confirm the delivery details are correct")]
        [Range(typeof(bool), "true", "true", ErrorMessage = "You must confirm the delivery details")]
        public bool ConfirmDetails { get; set; }
        
        public List<string> AvailableTimeSlots { get; set; } = new List<string>
        {
            "Morning (8:00 AM - 12:00 PM)",
            "Afternoon (12:00 PM - 5:00 PM)",
            "Evening (5:00 PM - 8:00 PM)",
            "Flexible (Any time)"
        };
        
        public List<string> SouthAfricanProvinces { get; set; } = new List<string>
        {
            "Eastern Cape",
            "Free State",
            "Gauteng",
            "KwaZulu-Natal",
            "Limpopo",
            "Mpumalanga",
            "Northern Cape",
            "North West",
            "Western Cape"
        };
    }
}