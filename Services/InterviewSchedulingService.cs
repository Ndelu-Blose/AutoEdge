using AutoEdge.Data;
using AutoEdge.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace AutoEdge.Services
{
    public class InterviewSchedulingService : IInterviewSchedulingService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<InterviewSchedulingService> _logger;
        private readonly IVideoMeetingService _videoMeetingService;

        // Business hours configuration
        private readonly TimeSpan _businessStartTime = new TimeSpan(9, 0, 0); // 9:00 AM
        private readonly TimeSpan _businessEndTime = new TimeSpan(16, 0, 0); // 4:00 PM
        private readonly DayOfWeek[] _businessDays = { DayOfWeek.Monday, DayOfWeek.Tuesday, DayOfWeek.Wednesday, DayOfWeek.Thursday, DayOfWeek.Friday };

        public InterviewSchedulingService(ApplicationDbContext context, ILogger<InterviewSchedulingService> logger, IVideoMeetingService videoMeetingService)
        {
            _context = context;
            _logger = logger;
            _videoMeetingService = videoMeetingService;
        }

        public async Task<List<TimeSlot>> GenerateAvailableTimeSlotsAsync(DateTime date, int durationMinutes, string recruiterName)
        {
            try
            {
                var timeSlots = new List<TimeSlot>();

                // Validate if the date is a business day
                if (!IsBusinessDay(date))
                {
                    _logger.LogWarning("Date {Date} is not a business day", date.ToString("yyyy-MM-dd"));
                    return timeSlots;
                }

                // Generate time slots from 9 AM to 4 PM
                var currentTime = date.Date.Add(_businessStartTime);
                var endTime = date.Date.Add(_businessEndTime);

                while (currentTime.AddMinutes(durationMinutes) <= endTime)
                {
                    var slotEndTime = currentTime.AddMinutes(durationMinutes);
                    
                    // Check for conflicts
                    var hasConflict = await HasTimeConflictAsync(currentTime, durationMinutes, recruiterName);
                    
                    var timeSlot = new TimeSlot
                    {
                        StartTime = currentTime,
                        EndTime = slotEndTime,
                        DurationMinutes = durationMinutes,
                        IsAvailable = !hasConflict,
                        Status = hasConflict ? "Conflict" : "Available",
                        ConflictReason = hasConflict ? "Time slot conflicts with existing interview" : null
                    };

                    timeSlots.Add(timeSlot);

                    // Move to next slot (add 15-minute buffer between interviews)
                    currentTime = currentTime.AddMinutes(durationMinutes + 15);
                }

                return timeSlots;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating available time slots for date {Date}", date);
                return new List<TimeSlot>();
            }
        }

        public bool IsWithinBusinessHours(DateTime dateTime)
        {
            // Check if it's a business day
            if (!IsBusinessDay(dateTime))
                return false;

            // Check if time is within business hours
            var timeOfDay = dateTime.TimeOfDay;
            return timeOfDay >= _businessStartTime && timeOfDay <= _businessEndTime;
        }

        public async Task<bool> HasTimeConflictAsync(DateTime startTime, int durationMinutes, string recruiterName)
        {
            try
            {
                var endTime = startTime.AddMinutes(durationMinutes);

                // Check for existing interviews that overlap with this time slot
                var conflictingInterviews = await _context.Interviews
                    .Where(i => i.IsActive && 
                               i.RecruiterName == recruiterName &&
                               ((i.ScheduledDateTime <= startTime && i.ScheduledDateTime.AddMinutes(i.DurationMinutes) > startTime) ||
                                (i.ScheduledDateTime < endTime && i.ScheduledDateTime.AddMinutes(i.DurationMinutes) >= endTime) ||
                                (i.ScheduledDateTime >= startTime && i.ScheduledDateTime.AddMinutes(i.DurationMinutes) <= endTime)))
                    .ToListAsync();

                return conflictingInterviews.Any();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking time conflict for {StartTime}", startTime);
                return true; // Assume conflict if error occurs
            }
        }

        public async Task<List<Interview>> AutoScheduleInterviewsAsync(List<Application> applications, DateTime startDate, int durationMinutes, string recruiterName, string recruiterEmail)
        {
            try
            {
                var scheduledInterviews = new List<Interview>();
                var currentDate = startDate.Date;

                // Find the next available business day
                while (!IsBusinessDay(currentDate) || currentDate <= DateTime.Today)
                {
                    currentDate = currentDate.AddDays(1);
                }

                var currentTime = currentDate.Add(_businessStartTime);
                var endTime = currentDate.Add(_businessEndTime);

                foreach (var application in applications)
                {
                    // Find next available slot
                    var availableSlot = await FindNextAvailableSlotAsync(currentTime, endTime, durationMinutes, recruiterName, currentDate);
                    
                    if (availableSlot == null)
                    {
                        // Move to next business day
                        currentDate = GetNextBusinessDay(currentDate);
                        currentTime = currentDate.Add(_businessStartTime);
                        endTime = currentDate.Add(_businessEndTime);
                        availableSlot = await FindNextAvailableSlotAsync(currentTime, endTime, durationMinutes, recruiterName, currentDate);
                    }

                    if (availableSlot != null)
                    {
                        // Create meeting
                        var meetingDetails = await _videoMeetingService.CreateMeetingAsync(
                            $"Interview - {application.JobPosting?.JobTitle}",
                            availableSlot.Value,
                            durationMinutes,
                            recruiterEmail
                        );

                        var interview = new Interview
                        {
                            ApplicationId = application.ApplicationId,
                            JobId = application.JobId,
                            ScheduledDateTime = availableSlot.Value,
                            DurationMinutes = durationMinutes,
                            MeetingLink = meetingDetails.MeetingUrl,
                            MeetingPassword = meetingDetails.MeetingPassword,
                            RecruiterName = recruiterName,
                            RecruiterEmail = recruiterEmail,
                            MeetingPlatform = meetingDetails.Platform,
                            CreatedDate = DateTime.UtcNow,
                            ModifiedDate = DateTime.UtcNow,
                            IsActive = true
                        };

                        _context.Interviews.Add(interview);
                        scheduledInterviews.Add(interview);

                        // Update application status
                        application.Status = "Interview Scheduled";
                        application.ModifiedDate = DateTime.UtcNow;

                        // Move to next slot (add 15-minute buffer)
                        currentTime = availableSlot.Value.AddMinutes(durationMinutes + 15);
                    }
                }

                await _context.SaveChangesAsync();
                return scheduledInterviews;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error auto-scheduling interviews");
                return new List<Interview>();
            }
        }

        public async Task<List<TimeSlot>> GetAvailableSlotsForDateAsync(DateTime date, int durationMinutes, string recruiterName)
        {
            return await GenerateAvailableTimeSlotsAsync(date, durationMinutes, recruiterName);
        }

        public async Task<InterviewSlotResult> CreateInterviewSlotsAsync(List<Application> applications, DateTime startDate, int durationMinutes, string recruiterName, string recruiterEmail)
        {
            var result = new InterviewSlotResult();

            try
            {
                // Validate business hours
                if (!IsBusinessDay(startDate))
                {
                    result.Errors.Add($"Selected date {startDate:yyyy-MM-dd} is not a business day. Please select a weekday (Monday-Friday).");
                    return result;
                }

                if (!IsWithinBusinessHours(startDate))
                {
                    result.Errors.Add($"Selected time is outside business hours (9:00 AM - 4:00 PM).");
                    return result;
                }

                // Validate duration
                if (durationMinutes < 30 || durationMinutes > 120)
                {
                    result.Errors.Add("Interview duration must be between 30 and 120 minutes.");
                    return result;
                }

                // Check if we have enough time slots available
                var availableSlots = await GenerateAvailableTimeSlotsAsync(startDate, durationMinutes, recruiterName);
                var freeSlots = availableSlots.Count(s => s.IsAvailable);

                if (freeSlots < applications.Count)
                {
                    result.Warnings.Add($"Only {freeSlots} time slots available for {applications.Count} candidates. Some interviews may need to be scheduled on different days.");
                }

                // Auto-schedule interviews
                var scheduledInterviews = await AutoScheduleInterviewsAsync(applications, startDate, durationMinutes, recruiterName, recruiterEmail);

                result.CreatedInterviews = scheduledInterviews;
                result.Success = true;
                result.Summary = $"Successfully scheduled {scheduledInterviews.Count} interviews for {startDate:yyyy-MM-dd}";

                _logger.LogInformation("Successfully created {Count} interview slots", scheduledInterviews.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating interview slots");
                result.Errors.Add("An error occurred while creating interview slots. Please try again.");
                result.Success = false;
            }

            return result;
        }

        // Helper Methods
        private bool IsBusinessDay(DateTime date)
        {
            return _businessDays.Contains(date.DayOfWeek);
        }

        private DateTime GetNextBusinessDay(DateTime currentDate)
        {
            var nextDate = currentDate.AddDays(1);
            while (!IsBusinessDay(nextDate))
            {
                nextDate = nextDate.AddDays(1);
            }
            return nextDate;
        }

        private async Task<DateTime?> FindNextAvailableSlotAsync(DateTime startTime, DateTime endTime, int durationMinutes, string recruiterName, DateTime date)
        {
            var currentTime = startTime;

            while (currentTime.AddMinutes(durationMinutes) <= endTime)
            {
                var hasConflict = await HasTimeConflictAsync(currentTime, durationMinutes, recruiterName);
                if (!hasConflict)
                {
                    return currentTime;
                }

                // Move to next slot (add 15-minute buffer)
                currentTime = currentTime.AddMinutes(durationMinutes + 15);
            }

            return null;
        }
    }
}
