using AutoEdge.Models.Entities;

namespace AutoEdge.Services
{
    public interface IAIRecruitmentService
    {
        // Auto-analyze and rate application (0-100%)
        Task<ApplicationRatingResult> AutoAnalyzeAndRateApplicationAsync(Application application, JobPosting jobPosting);
        
        // Auto-shortlist applications with AI rating
        Task<List<Application>> AutoShortlistApplicationsAsync(int jobId);
        
        // Get AI-powered match score
        Task<decimal> CalculateAIMatchScoreAsync(Application application, JobPosting jobPosting);
        
        // Enhanced resume parsing with AI
        Task<AIResumeParseResult> ParseResumeWithAIAsync(string filePath);
        
        // Generate detailed analysis report
        Task<string> GenerateDetailedAnalysisReportAsync(Application application, JobPosting jobPosting);
    }

    public class ApplicationRatingResult
    {
        public decimal OverallScore { get; set; } // 0-100%
        public decimal SkillsMatch { get; set; } // 0-100%
        public decimal ExperienceMatch { get; set; } // 0-100%
        public decimal EducationMatch { get; set; } // 0-100%
        public decimal QualificationsMatch { get; set; } // 0-100%
        public string DetailedAnalysis { get; set; } = string.Empty;
        public List<string> Strengths { get; set; } = new List<string>();
        public List<string> Weaknesses { get; set; } = new List<string>();
        public List<string> MissingSkills { get; set; } = new List<string>();
        public List<string> MatchingSkills { get; set; } = new List<string>();
        public string Recommendation { get; set; } = string.Empty; // SHORTLIST, WAITLIST, REJECT
        public string Reasoning { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty; // Shortlisted, Waitlisted, Rejected
    }

    public class AIResumeParseResult
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
        public string AISummary { get; set; } = string.Empty;
        public List<string> KeyAchievements { get; set; } = new List<string>();
        public string CareerProgression { get; set; } = string.Empty;
    }
}

