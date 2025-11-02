using AutoEdge.Models.Entities;

namespace AutoEdge.Services
{
    public interface IInterviewSchedulingService
    {
        // Validate business hours and generate available time slots
        Task<List<TimeSlot>> GenerateAvailableTimeSlotsAsync(DateTime date, int durationMinutes, string recruiterName);
        
        // Validate if a specific time is within business hours
        bool IsWithinBusinessHours(DateTime dateTime);
        
        // Check for time conflicts
        Task<bool> HasTimeConflictAsync(DateTime startTime, int durationMinutes, string recruiterName);
        
        // Auto-schedule interviews with proper intervals
        Task<List<Interview>> AutoScheduleInterviewsAsync(List<Application> applications, DateTime startDate, int durationMinutes, string recruiterName, string recruiterEmail);
        
        // Generate time slots for a specific date
        Task<List<TimeSlot>> GetAvailableSlotsForDateAsync(DateTime date, int durationMinutes, string recruiterName);
        
        // Validate and create interview slots
        Task<InterviewSlotResult> CreateInterviewSlotsAsync(List<Application> applications, DateTime startDate, int durationMinutes, string recruiterName, string recruiterEmail);
    }

    public class TimeSlot
    {
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public bool IsAvailable { get; set; }
        public string Status { get; set; } = string.Empty; // Available, Booked, Conflict
        public string? ConflictReason { get; set; }
        public int DurationMinutes { get; set; }
    }

    public class InterviewSlotResult
    {
        public bool Success { get; set; }
        public List<Interview> CreatedInterviews { get; set; } = new List<Interview>();
        public List<string> Errors { get; set; } = new List<string>();
        public List<string> Warnings { get; set; } = new List<string>();
        public string Summary { get; set; } = string.Empty;
    }
}
