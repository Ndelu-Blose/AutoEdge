namespace AutoEdge.Services
{
    public interface IVideoMeetingService
    {
        Task<MeetingDetails> CreateMeetingAsync(string title, DateTime startTime, int durationMinutes, string recruiterEmail);
        Task<bool> DeleteMeetingAsync(string meetingId);
        Task<MeetingDetails> GetMeetingAsync(string meetingId);
    }

    public class MeetingDetails
    {
        public string MeetingId { get; set; } = string.Empty;
        public string MeetingUrl { get; set; } = string.Empty;
        public string MeetingPassword { get; set; } = string.Empty;
        public string JoinUrl { get; set; } = string.Empty;
        public DateTime StartTime { get; set; }
        public int DurationMinutes { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Platform { get; set; } = string.Empty; // Zoom, Teams, Daily.co
    }
}
