using AutoEdge.Data;
using AutoEdge.Models.Entities;
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas.Parser;
using iText.Kernel.Pdf.Canvas.Parser.Listener;
using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;

namespace AutoEdge.Services
{
    public class ResumeParserService : IResumeParserService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<ResumeParserService> _logger;
        private readonly IAIRecruitmentService _aiRecruitmentService;

        public ResumeParserService(ApplicationDbContext context, ILogger<ResumeParserService> logger, IAIRecruitmentService aiRecruitmentService)
        {
            _context = context;
            _logger = logger;
            _aiRecruitmentService = aiRecruitmentService;
        }

        public async Task<ResumeParseResult> ParseResumeAsync(string filePath)
        {
            try
            {
                var result = new ResumeParseResult();

                // Extract text from PDF
                var extractedText = ExtractTextFromPdf(filePath);
                result.ExtractedText = extractedText;

                // Parse different sections
                result.FullName = ExtractFullName(extractedText);
                result.Email = ExtractEmail(extractedText);
                result.PhoneNumber = ExtractPhoneNumber(extractedText);
                result.YearsOfExperience = ExtractYearsOfExperience(extractedText);
                result.Skills = ExtractSkills(extractedText);
                result.Education = ExtractEducation(extractedText);
                result.Certifications = ExtractCertifications(extractedText);
                result.PreviousJobTitles = ExtractPreviousJobTitles(extractedText);
                result.WorkHistory = ExtractWorkHistory(extractedText);

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error parsing resume from file: {FilePath}", filePath);
                throw;
            }
        }

        public async Task<bool> ManuallyShortlistApplicationsAsync(List<int> applicationIds)
        {
            try
            {
                var applications = await _context.Applications
                    .Where(a => applicationIds.Contains(a.ApplicationId))
                    .ToListAsync();

                foreach (var application in applications)
                {
                    application.Status = "Shortlisted";
                    application.ModifiedDate = DateTime.UtcNow;
                }

                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error manually shortlisting applications");
                return false;
            }
        }

        public async Task<bool> ResetApplicationsToSubmittedAsync(int jobId)
        {
            try
            {
                var applications = await _context.Applications
                    .Where(a => a.JobId == jobId && a.IsActive)
                    .ToListAsync();

                foreach (var application in applications)
                {
                    application.Status = "Submitted";
                    application.ModifiedDate = DateTime.UtcNow;
                }

                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error resetting applications to submitted status");
                return false;
            }
        }

        public async Task<decimal> CalculateMatchScoreAsync(Application application, JobPosting jobPosting)
        {
            try
            {
                decimal totalScore = 0;
                decimal maxScore = 100;

                // Years of experience match (30 points)
                var experienceScore = CalculateExperienceScore(application.YearsOfExperience, jobPosting.MinYearsExperience);
                totalScore += experienceScore * 0.30m;

                // Skills match (40 points)
                var skillsScore = CalculateSkillsScore(application.ExtractedSkills, jobPosting.Requirements);
                totalScore += skillsScore * 0.40m;

                // Qualification match (20 points)
                var qualificationScore = CalculateQualificationScore(application.ExtractedEducation, jobPosting.RequiredQualifications);
                totalScore += qualificationScore * 0.20m;

                // Certifications (10 points)
                var certificationScore = CalculateCertificationScore(application.ExtractedSkills, jobPosting.Requirements);
                totalScore += certificationScore * 0.10m;

                return Math.Min(totalScore, maxScore);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating match score for application {ApplicationId}", application.ApplicationId);
                return 0;
            }
        }

        public async Task<List<Application>> GetShortlistedApplicationsAsync(int jobId, decimal threshold = 70, bool autoUpdateStatus = true)
        {
            try
            {
                var applications = await _context.Applications
                    .Where(a => a.JobId == jobId && a.IsActive)
                    .ToListAsync();

                var shortlistedApplications = new List<Application>();

                foreach (var application in applications)
                {
                    var jobPosting = await _context.JobPostings.FindAsync(jobId);
                    if (jobPosting != null)
                    {
                        var matchScore = await CalculateMatchScoreAsync(application, jobPosting);
                        application.MatchScore = matchScore;

                        // Only auto-update status if autoUpdateStatus is true and it's still "Submitted"
                        if (autoUpdateStatus && application.Status == "Submitted")
                        {
                            if (matchScore >= threshold)
                            {
                                application.Status = "Shortlisted";
                                shortlistedApplications.Add(application);
                            }
                            else
                            {
                                application.Status = "Rejected";
                            }
                        }
                        else if (application.Status == "Shortlisted")
                        {
                            // If already shortlisted (manually or automatically), include in results
                            shortlistedApplications.Add(application);
                        }
                        else if (!autoUpdateStatus && application.Status == "Submitted")
                        {
                            // If not auto-updating, include submitted applications in results for manual review
                            shortlistedApplications.Add(application);
                        }

                        application.ModifiedDate = DateTime.UtcNow;
                    }
                }

                await _context.SaveChangesAsync();
                return shortlistedApplications.OrderByDescending(a => a.MatchScore).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting shortlisted applications for job {JobId}", jobId);
                return new List<Application>();
            }
        }

        private string ExtractTextFromPdf(string filePath)
        {
            try
            {
                using var pdfReader = new PdfReader(filePath);
                using var pdfDocument = new PdfDocument(pdfReader);
                var strategy = new SimpleTextExtractionStrategy();
                var text = string.Empty;

                for (int i = 1; i <= pdfDocument.GetNumberOfPages(); i++)
                {
                    var page = pdfDocument.GetPage(i);
                    text += PdfTextExtractor.GetTextFromPage(page, strategy);
                }

                return text;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error extracting text from PDF: {FilePath}", filePath);
                return string.Empty;
            }
        }

        private string ExtractFullName(string text)
        {
            // Look for common name patterns at the beginning of the document
            var lines = text.Split('\n', StringSplitOptions.RemoveEmptyEntries);
            if (lines.Length > 0)
            {
                var firstLine = lines[0].Trim();
                if (Regex.IsMatch(firstLine, @"^[A-Za-z\s]{2,50}$"))
                {
                    return firstLine;
                }
            }
            return string.Empty;
        }

        private string ExtractEmail(string text)
        {
            var emailPattern = @"\b[A-Za-z0-9._%+-]+@[A-Za-z0-9.-]+\.[A-Z|a-z]{2,}\b";
            var match = Regex.Match(text, emailPattern);
            return match.Success ? match.Value : string.Empty;
        }

        private string ExtractPhoneNumber(string text)
        {
            var phonePattern = @"(\+?1[-.\s]?)?\(?([0-9]{3})\)?[-.\s]?([0-9]{3})[-.\s]?([0-9]{4})";
            var match = Regex.Match(text, phonePattern);
            return match.Success ? match.Value : string.Empty;
        }

        private int ExtractYearsOfExperience(string text)
        {
            var experiencePatterns = new[]
            {
                @"(\d+)\s*years?\s*of\s*experience",
                @"(\d+)\s*years?\s*experience",
                @"experience.*?(\d+)\s*years?",
                @"(\d+)\+?\s*years?"
            };

            foreach (var pattern in experiencePatterns)
            {
                var match = Regex.Match(text, pattern, RegexOptions.IgnoreCase);
                if (match.Success && int.TryParse(match.Groups[1].Value, out int years))
                {
                    return years;
                }
            }

            return 0;
        }

        private List<string> ExtractSkills(string text)
        {
            var commonSkills = new[]
            {
                "C#", "Java", "Python", "JavaScript", "SQL", "HTML", "CSS", "React", "Angular", "Vue",
                "ASP.NET", "Entity Framework", "Azure", "AWS", "Docker", "Kubernetes", "Git", "Agile", "Scrum",
                "Project Management", "Leadership", "Communication", "Problem Solving", "Teamwork", "Analytical",
                "CAD", "AutoCAD", "SolidWorks", "Mechanical Design", "Engineering", "Manufacturing",
                "Sales", "Customer Service", "Negotiation", "Marketing", "CRM", "Lead Generation",
                "Driving", "CDL", "Transportation", "Logistics", "Safety", "Vehicle Maintenance",
                "IT Support", "Help Desk", "Troubleshooting", "Hardware", "Software", "Networking", "Windows", "Linux"
            };

            var foundSkills = new List<string>();
            foreach (var skill in commonSkills)
            {
                if (text.Contains(skill, StringComparison.OrdinalIgnoreCase))
                {
                    foundSkills.Add(skill);
                }
            }

            return foundSkills;
        }

        private List<string> ExtractEducation(string text)
        {
            var educationPatterns = new[]
            {
                @"Bachelor.*?Degree",
                @"Master.*?Degree",
                @"PhD|Ph\.D\.|Doctorate",
                @"Associate.*?Degree",
                @"Diploma",
                @"Certificate",
                @"High School",
                @"University",
                @"College"
            };

            var education = new List<string>();
            foreach (var pattern in educationPatterns)
            {
                var matches = Regex.Matches(text, pattern, RegexOptions.IgnoreCase);
                foreach (Match match in matches)
                {
                    if (!education.Contains(match.Value))
                    {
                        education.Add(match.Value);
                    }
                }
            }

            return education;
        }

        private List<string> ExtractCertifications(string text)
        {
            var certificationPatterns = new[]
            {
                @"Certified.*?Professional",
                @"Microsoft.*?Certified",
                @"AWS.*?Certified",
                @"Google.*?Certified",
                @"PMP",
                @"ITIL",
                @"CompTIA",
                @"Cisco.*?Certified",
                @"Oracle.*?Certified"
            };

            var certifications = new List<string>();
            foreach (var pattern in certificationPatterns)
            {
                var matches = Regex.Matches(text, pattern, RegexOptions.IgnoreCase);
                foreach (Match match in matches)
                {
                    if (!certifications.Contains(match.Value))
                    {
                        certifications.Add(match.Value);
                    }
                }
            }

            return certifications;
        }

        private List<string> ExtractPreviousJobTitles(string text)
        {
            var jobTitlePatterns = new[]
            {
                @"Software.*?Engineer",
                @"Senior.*?Developer",
                @"Project.*?Manager",
                @"Sales.*?Representative",
                @"Mechanical.*?Engineer",
                @"IT.*?Support",
                @"Help.*?Desk",
                @"Driver",
                @"Technician",
                @"Analyst",
                @"Consultant",
                @"Coordinator",
                @"Specialist"
            };

            var jobTitles = new List<string>();
            foreach (var pattern in jobTitlePatterns)
            {
                var matches = Regex.Matches(text, pattern, RegexOptions.IgnoreCase);
                foreach (Match match in matches)
                {
                    if (!jobTitles.Contains(match.Value))
                    {
                        jobTitles.Add(match.Value);
                    }
                }
            }

            return jobTitles;
        }

        private string ExtractWorkHistory(string text)
        {
            // Extract work experience section
            var workHistoryPattern = @"(?:experience|employment|work history|professional experience)(.*?)(?:education|skills|certifications|$)"
                + @"|(?:experience|employment|work history|professional experience)(.*?)$";
            
            var match = Regex.Match(text, workHistoryPattern, RegexOptions.IgnoreCase | RegexOptions.Singleline);
            return match.Success ? match.Groups[1].Value.Trim() : string.Empty;
        }

        private decimal CalculateExperienceScore(int applicantExperience, int requiredExperience)
        {
            if (applicantExperience >= requiredExperience)
            {
                return 100; // Full score if meets or exceeds requirement
            }
            else if (requiredExperience > 0)
            {
                return (decimal)applicantExperience / requiredExperience * 100;
            }
            return 0;
        }

        private decimal CalculateSkillsScore(string applicantSkills, string jobRequirements)
        {
            if (string.IsNullOrEmpty(applicantSkills) || string.IsNullOrEmpty(jobRequirements))
                return 0;

            var applicantSkillsList = applicantSkills.Split(',').Select(s => s.Trim()).ToList();
            var requiredSkills = ExtractSkills(jobRequirements);
            
            if (requiredSkills.Count == 0)
                return 0;

            var matchedSkills = applicantSkillsList.Count(skill => 
                requiredSkills.Any(req => skill.Contains(req, StringComparison.OrdinalIgnoreCase)));

            return (decimal)matchedSkills / requiredSkills.Count * 100;
        }

        private decimal CalculateQualificationScore(string applicantEducation, string jobRequirements)
        {
            if (string.IsNullOrEmpty(applicantEducation) || string.IsNullOrEmpty(jobRequirements))
                return 0;

            var educationList = ExtractEducation(applicantEducation);
            var requiredEducation = ExtractEducation(jobRequirements);

            if (requiredEducation.Count == 0)
                return 100; // No specific education requirement

            var matchedEducation = educationList.Count(edu => 
                requiredEducation.Any(req => edu.Contains(req, StringComparison.OrdinalIgnoreCase)));

            return (decimal)matchedEducation / requiredEducation.Count * 100;
        }

        private decimal CalculateCertificationScore(string applicantSkills, string jobRequirements)
        {
            if (string.IsNullOrEmpty(applicantSkills) || string.IsNullOrEmpty(jobRequirements))
                return 0;

            var applicantCerts = ExtractCertifications(applicantSkills);
            var requiredCerts = ExtractCertifications(jobRequirements);

            if (requiredCerts.Count == 0)
                return 100; // No specific certification requirement

            var matchedCerts = applicantCerts.Count(cert => 
                requiredCerts.Any(req => cert.Contains(req, StringComparison.OrdinalIgnoreCase)));

            return (decimal)matchedCerts / requiredCerts.Count * 100;
        }

        // AI-Enhanced Methods
        public async Task<decimal> CalculateAIMatchScoreAsync(Application application, JobPosting jobPosting)
        {
            try
            {
                return await _aiRecruitmentService.CalculateAIMatchScoreAsync(application, jobPosting);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating AI match score for application {ApplicationId}", application.ApplicationId);
                // Fallback to traditional calculation
                return await CalculateMatchScoreAsync(application, jobPosting);
            }
        }

        public async Task<List<Application>> GetAIShortlistedApplicationsAsync(int jobId)
        {
            try
            {
                return await _aiRecruitmentService.AutoShortlistApplicationsAsync(jobId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting AI shortlisted applications for job {JobId}", jobId);
                // Fallback to traditional shortlisting
                return await GetShortlistedApplicationsAsync(jobId, 70, true);
            }
        }
    }
}
