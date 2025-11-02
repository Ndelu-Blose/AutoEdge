using AutoEdge.Data;
using AutoEdge.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System.Text;
using System.Text.Json;

namespace AutoEdge.Services
{
    public class AIRecruitmentService : IAIRecruitmentService
    {
        private readonly HttpClient _httpClient;
        private readonly ApplicationDbContext _context;
        private readonly ILogger<AIRecruitmentService> _logger;
        private readonly AISettings _settings;

        public AIRecruitmentService(HttpClient httpClient, ApplicationDbContext context, 
            ILogger<AIRecruitmentService> logger, IOptions<AISettings> settings)
        {
            _httpClient = httpClient;
            _context = context;
            _logger = logger;
            _settings = settings.Value;
            
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_settings.ApiKey}");
        }

        public async Task<ApplicationRatingResult> AutoAnalyzeAndRateApplicationAsync(Application application, JobPosting jobPosting)
        {
            try
            {
                var cvText = await ReadCvTextAsync(application.ResumeFilePath);
                
                var prompt = $@"You are an expert HR recruiter analyzing a job application. Provide a comprehensive evaluation with precise scoring.

JOB POSTING DETAILS:
Title: {jobPosting.JobTitle}
Department: {jobPosting.Department}
Requirements: {jobPosting.Requirements}
Experience Required: {jobPosting.MinYearsExperience} years
Qualifications: {jobPosting.RequiredQualifications}
Responsibilities: {jobPosting.Responsibilities}

CANDIDATE APPLICATION:
Name: {application.FirstName} {application.LastName}
Email: {application.Email}
Experience: {application.YearsOfExperience} years
Skills: {application.ExtractedSkills}
Education: {application.ExtractedEducation}

CANDIDATE'S CV CONTENT:
{cvText}

Analyze and provide a detailed rating in JSON format:
{{
    ""overallScore"": <number between 0-100>,
    ""skillsMatch"": <number between 0-100>,
    ""experienceMatch"": <number between 0-100>,
    ""educationMatch"": <number between 0-100>,
    ""qualificationsMatch"": <number between 0-100>,
    ""detailedAnalysis"": ""<comprehensive analysis of the candidate's fit>"",
    ""strengths"": [""<strength1>"", ""<strength2>"", ""<strength3>""],
    ""weaknesses"": [""<weakness1>"", ""<weakness2>""],
    ""missingSkills"": [""<missing skill1>"", ""<missing skill2>""],
    ""matchingSkills"": [""<matching skill1>"", ""<matching skill2>""],
    ""recommendation"": ""<SHORTLIST/WAITLIST/REJECT>"",
    ""reasoning"": ""<detailed reasoning for the recommendation>""
}}

Scoring Criteria:
- Skills Match (40%): How well do the candidate's skills align with job requirements?
- Experience Match (30%): Does the candidate have sufficient relevant experience?
- Education Match (20%): Does the candidate meet educational requirements?
- Qualifications Match (10%): Does the candidate have required certifications/qualifications?

Recommendation Rules:
- SHORTLIST: 80% or above
- WAITLIST: 75-79%
- REJECT: Below 75%

Be objective, thorough, and provide specific examples from the CV to support your scoring.";

                var response = await CallAIAsync(prompt);
                var result = ParseApplicationRating(response);
                
                // Set status based on score
                if (result.OverallScore >= 80)
                {
                    result.Status = "Shortlisted";
                    result.Recommendation = "SHORTLIST";
                }
                else if (result.OverallScore >= 75)
                {
                    result.Status = "Waitlisted";
                    result.Recommendation = "WAITLIST";
                }
                else
                {
                    result.Status = "Rejected";
                    result.Recommendation = "REJECT";
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error auto-analyzing application {ApplicationId}", application.ApplicationId);
                return new ApplicationRatingResult 
                { 
                    OverallScore = 0, 
                    Status = "Rejected", 
                    Recommendation = "REJECT",
                    Reasoning = "Error in analysis - automatic rejection"
                };
            }
        }

        public async Task<List<Application>> AutoShortlistApplicationsAsync(int jobId)
        {
            try
            {
                var applications = await _context.Applications
                    .Where(a => a.JobId == jobId && a.IsActive && a.Status == "Submitted")
                    .ToListAsync();

                var jobPosting = await _context.JobPostings.FindAsync(jobId);
                if (jobPosting == null) return new List<Application>();

                var shortlistedApplications = new List<Application>();
                var waitlistedApplications = new List<Application>();
                var rejectedApplications = new List<Application>();

                foreach (var application in applications)
                {
                    var rating = await AutoAnalyzeAndRateApplicationAsync(application, jobPosting);
                    
                    // Update application with AI rating
                    application.MatchScore = rating.OverallScore;
                    application.AdminNotes = $"AI Auto-Analysis (Score: {rating.OverallScore:F1}%)\n" +
                                           $"Status: {rating.Status}\n" +
                                           $"Detailed Analysis: {rating.DetailedAnalysis}\n" +
                                           $"Strengths: {string.Join(", ", rating.Strengths)}\n" +
                                           $"Weaknesses: {string.Join(", ", rating.Weaknesses)}\n" +
                                           $"Missing Skills: {string.Join(", ", rating.MissingSkills)}\n" +
                                           $"Matching Skills: {string.Join(", ", rating.MatchingSkills)}\n" +
                                           $"Reasoning: {rating.Reasoning}";
                    
                    // Set status based on AI rating
                    if (rating.OverallScore >= 80)
                    {
                        application.Status = "Shortlisted";
                        shortlistedApplications.Add(application);
                    }
                    else if (rating.OverallScore >= 75)
                    {
                        application.Status = "Waitlisted";
                        waitlistedApplications.Add(application);
                    }
                    else
                    {
                        application.Status = "Rejected";
                        rejectedApplications.Add(application);
                    }
                    
                    application.ModifiedDate = DateTime.UtcNow;
                }

                await _context.SaveChangesAsync();
                
                _logger.LogInformation("AI Auto-Analysis completed for job {JobId}: {Shortlisted} shortlisted, {Waitlisted} waitlisted, {Rejected} rejected", 
                    jobId, shortlistedApplications.Count, waitlistedApplications.Count, rejectedApplications.Count);

                return shortlistedApplications.OrderByDescending(a => a.MatchScore).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error auto-shortlisting applications for job {JobId}", jobId);
                return new List<Application>();
            }
        }

        public async Task<decimal> CalculateAIMatchScoreAsync(Application application, JobPosting jobPosting)
        {
            try
            {
                var rating = await AutoAnalyzeAndRateApplicationAsync(application, jobPosting);
                return rating.OverallScore;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating AI match score for application {ApplicationId}", application.ApplicationId);
                return 0;
            }
        }

        public async Task<AIResumeParseResult> ParseResumeWithAIAsync(string filePath)
        {
            try
            {
                var extractedText = await ReadCvTextAsync(filePath);
                
                var prompt = $@"Parse and analyze this resume/CV with advanced AI capabilities. Extract all relevant information.

RESUME TEXT:
{extractedText}

Extract and analyze in JSON format:
{{
    ""fullName"": ""<extracted full name>"",
    ""email"": ""<extracted email>"",
    ""phoneNumber"": ""<extracted phone number>"",
    ""yearsOfExperience"": <calculated years of experience>,
    ""skills"": [""<skill1>"", ""<skill2>"", ""<skill3>""],
    ""education"": [""<education1>"", ""<education2>""],
    ""certifications"": [""<certification1>"", ""<certification2>""],
    ""previousJobTitles"": [""<title1>"", ""<title2>""],
    ""workHistory"": ""<detailed work history summary>"",
    ""extractedText"": ""<full extracted text>"",
    ""aiSummary"": ""<AI-generated professional summary>"",
    ""keyAchievements"": [""<achievement1>"", ""<achievement2>""],
    ""careerProgression"": ""<analysis of career progression>""
}}";

                var response = await CallAIAsync(prompt);
                return ParseAIResumeResult(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error parsing resume with AI from {FilePath}", filePath);
                return new AIResumeParseResult();
            }
        }

        public async Task<string> GenerateDetailedAnalysisReportAsync(Application application, JobPosting jobPosting)
        {
            try
            {
                var rating = await AutoAnalyzeAndRateApplicationAsync(application, jobPosting);
                
                var report = $@"
# AI-Powered Application Analysis Report

## Candidate Information
- **Name:** {application.FirstName} {application.LastName}
- **Email:** {application.Email}
- **Position Applied:** {jobPosting.JobTitle}
- **Department:** {jobPosting.Department}

## Overall Rating: {rating.OverallScore:F1}%

### Detailed Scores:
- **Skills Match:** {rating.SkillsMatch:F1}%
- **Experience Match:** {rating.ExperienceMatch:F1}%
- **Education Match:** {rating.EducationMatch:F1}%
- **Qualifications Match:** {rating.QualificationsMatch:F1}%

### Recommendation: {rating.Recommendation}
**Status:** {rating.Status}

## Detailed Analysis
{rating.DetailedAnalysis}

## Strengths
{string.Join("\n- ", rating.Strengths.Select(s => $"- {s}"))}

## Areas for Improvement
{string.Join("\n- ", rating.Weaknesses.Select(w => $"- {w}"))}

## Missing Skills
{string.Join("\n- ", rating.MissingSkills.Select(s => $"- {s}"))}

## Matching Skills
{string.Join("\n- ", rating.MatchingSkills.Select(s => $"- {s}"))}

## Reasoning
{rating.Reasoning}

---
*Report generated by AI Recruitment System on {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC*
";

                return report;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating detailed analysis report for application {ApplicationId}", application.ApplicationId);
                return "Error generating analysis report.";
            }
        }

        // Helper Methods
        private async Task<string> CallAIAsync(string prompt)
        {
            var requestBody = new
            {
                model = _settings.Model,
                messages = new[]
                {
                    new { role = "system", content = "You are an expert HR recruiter and talent acquisition specialist with deep knowledge of the automotive industry. Always respond with valid JSON format." },
                    new { role = "user", content = prompt }
                },
                temperature = _settings.Temperature,
                max_tokens = _settings.MaxTokens
            };

            var json = JsonSerializer.Serialize(requestBody);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync($"{_settings.BaseUrl}/chat/completions", content);
            response.EnsureSuccessStatusCode();

            var responseContent = await response.Content.ReadAsStringAsync();
            var responseObject = JsonSerializer.Deserialize<AIResponse>(responseContent);

            return responseObject?.choices?.FirstOrDefault()?.message?.content ?? string.Empty;
        }

        private async Task<string> ReadCvTextAsync(string filePath)
        {
            if (string.IsNullOrEmpty(filePath) || !File.Exists(filePath))
                return string.Empty;

            try
            {
                using var pdfReader = new iText.Kernel.Pdf.PdfReader(filePath);
                using var pdfDocument = new iText.Kernel.Pdf.PdfDocument(pdfReader);
                var strategy = new iText.Kernel.Pdf.Canvas.Parser.Listener.SimpleTextExtractionStrategy();
                var text = string.Empty;

                for (int i = 1; i <= pdfDocument.GetNumberOfPages(); i++)
                {
                    var page = pdfDocument.GetPage(i);
                    text += iText.Kernel.Pdf.Canvas.Parser.PdfTextExtractor.GetTextFromPage(page, strategy);
                }

                return text;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error reading CV text from {FilePath}", filePath);
                return string.Empty;
            }
        }

        // JSON Parsing Methods
        private ApplicationRatingResult ParseApplicationRating(string response)
        {
            try
            {
                var result = JsonSerializer.Deserialize<ApplicationRatingResult>(response);
                return result ?? new ApplicationRatingResult { OverallScore = 0 };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error parsing application rating response: {Response}", response);
                return new ApplicationRatingResult { OverallScore = 0 };
            }
        }

        private AIResumeParseResult ParseAIResumeResult(string response)
        {
            try
            {
                var result = JsonSerializer.Deserialize<AIResumeParseResult>(response);
                return result ?? new AIResumeParseResult();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error parsing AI resume result: {Response}", response);
                return new AIResumeParseResult();
            }
        }
    }

    // Configuration Models
    public class AISettings
    {
        public string BaseUrl { get; set; } = string.Empty;
        public string ApiKey { get; set; } = string.Empty;
        public string Model { get; set; } = "gpt-4";
        public double Temperature { get; set; } = 0.3;
        public int MaxTokens { get; set; } = 2000;
    }

    public class AIResponse
    {
        public AIChoice[] choices { get; set; } = Array.Empty<AIChoice>();
    }

    public class AIChoice
    {
        public AIMessage message { get; set; } = new AIMessage();
    }

    public class AIMessage
    {
        public string content { get; set; } = string.Empty;
    }
}
