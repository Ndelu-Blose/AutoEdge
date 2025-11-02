using System.ComponentModel.DataAnnotations;

namespace AutoEdge.Models.ViewModels
{
    public class ServiceReminderViewModel
    {
        public int ContractId { get; set; }
        public string VehicleInfo { get; set; } = string.Empty;
        public string VIN { get; set; } = string.Empty;
        public int CurrentMileage { get; set; }
        public DateTime PurchaseDate { get; set; }
        
        // Last Service Information
        public DateTime? LastServiceDate { get; set; }
        public int LastServiceMileage { get; set; }
        public string LastServiceType { get; set; } = string.Empty;
        
        // Next Service Due
        public DateTime? NextServiceDue { get; set; }
        public int NextServiceMileage { get; set; }
        public string NextServiceType { get; set; } = string.Empty;
        
        // New Reminder Details
        [Required(ErrorMessage = "Service type is required")]
        [Display(Name = "Service Type")]
        public string ServiceType { get; set; } = string.Empty;
        
        [Display(Name = "Custom Service Description")]
        public string? CustomServiceDescription { get; set; }
        
        [Display(Name = "Reminder Date")]
        [DataType(DataType.Date)]
        public DateTime? ReminderDate { get; set; }
        
        [Display(Name = "Reminder Mileage")]
        [Range(0, int.MaxValue, ErrorMessage = "Mileage must be a positive number")]
        public int? ReminderMileage { get; set; }
        
        [Display(Name = "Days Before Service")]
        [Range(1, 365, ErrorMessage = "Days before reminder must be between 1 and 365")]
        public int DaysBeforeReminder { get; set; } = 30;
        
        // Notification Preferences
        [Display(Name = "Email Reminder")]
        public bool EmailReminder { get; set; } = true;
        
        [Display(Name = "SMS Reminder")]
        public bool SmsReminder { get; set; }
        
        [Display(Name = "App Notification")]
        public bool AppNotification { get; set; } = true;
        
        // Additional Information
        [Display(Name = "Notes")]
        [StringLength(500, ErrorMessage = "Notes cannot exceed 500 characters")]
        public string? Notes { get; set; }
        
        // Recurring Options
        [Display(Name = "Recurring Reminder")]
        public bool IsRecurring { get; set; }
        
        [Display(Name = "Recurring Interval (months)")]
        [Range(1, 24, ErrorMessage = "Recurring interval must be between 1 and 24 months")]
        public int RecurringInterval { get; set; } = 6;
        
        // Active Reminders List
        public List<ServiceReminderItem> ActiveReminders { get; set; } = new List<ServiceReminderItem>();
    }
    
    public class ServiceReminderItem
    {
        public int Id { get; set; }
        public string ServiceType { get; set; } = string.Empty;
        public DateTime DueDate { get; set; }
        public int DueMileage { get; set; }
        public bool EmailReminder { get; set; }
        public bool SmsReminder { get; set; }
        public bool AppNotification { get; set; }
        public bool IsRecurring { get; set; }
        public int RecurringInterval { get; set; }
        public string? Notes { get; set; }
        public DateTime CreatedDate { get; set; }
        public bool IsActive { get; set; } = true;
    }
}