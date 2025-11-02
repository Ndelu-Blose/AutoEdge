using AutoEdge.Models.Entities;

namespace AutoEdge.Services
{
    public interface IResumeParserService
    {
        Task<ResumeParseResult> ParseResumeAsync(string filePath);
        Task<decimal> CalculateMatchScoreAsync(Application application, JobPosting jobPosting);
        Task<List<Application>> GetShortlistedApplicationsAsync(int jobId, decimal threshold = 70, bool autoUpdateStatus = true);
        Task<bool> ManuallyShortlistApplicationsAsync(List<int> applicationIds);
        Task<bool> ResetApplicationsToSubmittedAsync(int jobId);
        
        // AI-Enhanced Methods
        Task<decimal> CalculateAIMatchScoreAsync(Application application, JobPosting jobPosting);
        Task<List<Application>> GetAIShortlistedApplicationsAsync(int jobId);
    }

    public class ResumeParseResult
    {
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public int YearsOfExperience { get; set; }
        public List<string> Skills { get; set; } = new List<string>();
        public List<string> Education { get; set; } = new List<string>();
        public List<string> Certifications { get; set; } = new List<string>();
        public List<string> PreviousJobTitles { get; set; } = new List<string>();
        public string WorkHistory { get; set; } = string.Empty;
        public string ExtractedText { get; set; } = string.Empty;
    }
}
