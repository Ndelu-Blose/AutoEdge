using AutoEdge.Services;
using System.Text.Json;

namespace AutoEdge.Services
{
    public class VideoMeetingService : IVideoMeetingService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<VideoMeetingService> _logger;
        private readonly HttpClient _httpClient;

        public VideoMeetingService(IConfiguration configuration, ILogger<VideoMeetingService> logger, HttpClient httpClient)
        {
            _configuration = configuration;
            _logger = logger;
            _httpClient = httpClient;
        }

        public async Task<MeetingDetails> CreateMeetingAsync(string title, DateTime startTime, int durationMinutes, string recruiterEmail)
        {
            try
            {
                // Using Jitsi Meet - no API key required, simple and reliable
                return await CreateJitsiMeetingAsync(title, startTime, durationMinutes, recruiterEmail);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating video meeting for title: {Title}", title);
                throw;
            }
        }

        public async Task<bool> DeleteMeetingAsync(string meetingId)
        {
            try
            {
                // Jitsi Meet doesn't require explicit meeting deletion
                // Meetings are automatically cleaned up when no participants are present
                _logger.LogInformation("Jitsi meeting {MeetingId} will be automatically cleaned up when empty", meetingId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error handling meeting deletion: {MeetingId}", meetingId);
                return false;
            }
        }

        public async Task<MeetingDetails> GetMeetingAsync(string meetingId)
        {
            try
            {
                // For Jitsi Meet, we can reconstruct the meeting details from the meeting ID
                var jitsiDomain = _configuration["Jitsi:Domain"] ?? "meet.jit.si";
                var meetingUrl = $"https://{jitsiDomain}/{meetingId}";
                
                return new MeetingDetails
                {
                    MeetingId = meetingId,
                    MeetingUrl = meetingUrl,
                    JoinUrl = meetingUrl,
                    Platform = "Jitsi Meet",
                    Title = $"Interview Meeting - {meetingId}"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting meeting: {MeetingId}", meetingId);
                return new MeetingDetails();
            }
        }

        private async Task<MeetingDetails> CreateJitsiMeetingAsync(string title, DateTime startTime, int durationMinutes, string recruiterEmail)
        {
            try
            {
                // Generate a unique meeting ID for Jitsi
                var meetingId = GenerateJitsiMeetingId(title, recruiterEmail);
                
                // Get Jitsi domain from configuration or use default
                var jitsiDomain = _configuration["Jitsi:Domain"] ?? "meet.jit.si";
                
                // Create the Jitsi Meet URL
                var meetingUrl = $"https://{jitsiDomain}/{meetingId}";
                
                _logger.LogInformation("Created Jitsi meeting: {MeetingId} for {Title}", meetingId, title);
                
                return new MeetingDetails
                {
                    MeetingId = meetingId,
                    MeetingUrl = meetingUrl,
                    JoinUrl = meetingUrl,
                    Platform = "Jitsi Meet",
                    Title = title,
                    StartTime = startTime,
                    DurationMinutes = durationMinutes
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating Jitsi meeting");
                return CreateFallbackMeeting(title, startTime, durationMinutes);
            }
        }

        private string GenerateJitsiMeetingId(string title, string recruiterEmail)
        {
            // Create a clean, URL-safe meeting ID
            var cleanTitle = title.ToLowerInvariant()
                .Replace(" ", "-")
                .Replace("interview", "")
                .Replace("at-autoedge", "")
                .Trim('-');
            
            // Get recruiter name from email (before @)
            var recruiterName = recruiterEmail.Split('@')[0].ToLowerInvariant();
            
            // Generate unique suffix
            var uniqueSuffix = Guid.NewGuid().ToString("N")[..8];
            
            // Combine to create meeting ID
            var meetingId = $"autoedge-{cleanTitle}-{recruiterName}-{uniqueSuffix}";
            
            // Ensure it's not too long (Jitsi has URL length limits)
            if (meetingId.Length > 50)
            {
                meetingId = $"autoedge-{uniqueSuffix}";
            }
            
            return meetingId;
        }

        private MeetingDetails CreateFallbackMeeting(string title, DateTime startTime, int durationMinutes)
        {
            var meetingId = $"fallback-meeting-{Guid.NewGuid().ToString("N")[..8]}";
            return new MeetingDetails
            {
                MeetingId = meetingId,
                MeetingUrl = $"https://meet.jit.si/{meetingId}",
                JoinUrl = $"https://meet.jit.si/{meetingId}",
                Platform = "Jitsi Meet (Fallback)",
                Title = title,
                StartTime = startTime,
                DurationMinutes = durationMinutes
            };
        }
    }
}
