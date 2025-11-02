using AutoEdge.Data;
using AutoEdge.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace AutoEdge.Services
{
    public class AssessmentService : IAssessmentService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<AssessmentService> _logger;

        public AssessmentService(ApplicationDbContext context, ILogger<AssessmentService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<Assessment?> GetAssessmentByTokenAsync(string token)
        {
            return await _context.Assessments
                .Include(a => a.Application)
                .ThenInclude(app => app.JobPosting)
                .Include(a => a.AssessmentQuestions)
                .ThenInclude(aq => aq.Question)
                .Include(a => a.RecruiterAssignments)
                .FirstOrDefaultAsync(a => a.AccessToken == token && a.IsActive);
        }

        public async Task<List<Assessment>> GetAssessmentsForUserAsync(string userId)
        {
            return await _context.Assessments
                .Include(a => a.Application)
                .ThenInclude(app => app.JobPosting)
                .Include(a => a.AssessmentQuestions)
                .ThenInclude(aq => aq.Question)
                .Include(a => a.RecruiterAssignments)
                .Where(a => a.Application != null && a.Application.UserId == userId && a.IsActive)
                .OrderByDescending(a => a.CreatedDate)
                .ToListAsync();
        }

        public async Task<List<Question>> GetQuestionsForAssessmentAsync(int assessmentId)
        {
            return await _context.AssessmentQuestions
                .Include(aq => aq.Question)
                .Where(aq => aq.AssessmentId == assessmentId)
                .OrderBy(aq => aq.Order)
                .Select(aq => aq.Question)
                .ToListAsync();
        }

        public async Task<bool> SubmitAssessmentAnswersAsync(int assessmentId, int applicationId, Dictionary<int, string> answers)
        {
            try
            {
                // Get assessment questions
                var assessmentQuestions = await _context.AssessmentQuestions
                    .Where(aq => aq.AssessmentId == assessmentId)
                    .ToListAsync();

                // Save answers
                foreach (var answer in answers)
                {
                    var assessmentQuestion = assessmentQuestions.FirstOrDefault(aq => aq.QuestionId == answer.Key);
                    if (assessmentQuestion != null)
                    {
                        var assessmentAnswer = new AssessmentAnswer
                        {
                            AssessmentQuestionId = assessmentQuestion.AssessmentQuestionId,
                            ApplicationId = applicationId,
                            AnswerText = answer.Value,
                            SubmittedDate = DateTime.UtcNow
                        };

                        _context.AssessmentAnswers.Add(assessmentAnswer);
                    }
                }

                // Mark assessment as completed
                var assessment = await _context.Assessments.FindAsync(assessmentId);
                if (assessment != null)
                {
                    assessment.IsCompleted = true;
                    assessment.CompletedDate = DateTime.UtcNow;
                    assessment.ModifiedDate = DateTime.UtcNow;
                }

                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error submitting assessment answers for assessment {AssessmentId}", assessmentId);
                return false;
            }
        }

        public async Task<decimal> CalculateAssessmentScoreAsync(int assessmentId, int applicationId)
        {
            try
            {
                var answers = await _context.AssessmentAnswers
                    .Include(aa => aa.AssessmentQuestion)
                    .ThenInclude(aq => aq.Question)
                    .Where(aa => aa.AssessmentQuestion.AssessmentId == assessmentId && aa.ApplicationId == applicationId)
                    .ToListAsync();

                if (!answers.Any())
                    return 0;

                decimal totalScore = 0;
                decimal maxScore = 0;

                foreach (var answer in answers)
                {
                    var question = answer.AssessmentQuestion.Question;
                    maxScore += question.Points;

                    // Simple grading logic based on question type
                    switch (question.Type)
                    {
                        case QuestionType.MultipleChoice:
                        case QuestionType.TrueFalse:
                            if (!string.IsNullOrEmpty(question.CorrectAnswer) && 
                                answer.AnswerText.Equals(question.CorrectAnswer, StringComparison.OrdinalIgnoreCase))
                            {
                                totalScore += question.Points;
                            }
                            break;

                        case QuestionType.ShortAnswer:
                            if (!string.IsNullOrEmpty(question.CorrectAnswer) && !string.IsNullOrEmpty(answer.AnswerText))
                            {
                                var keywords = question.CorrectAnswer.Split(',', StringSplitOptions.RemoveEmptyEntries)
                                    .Select(k => k.Trim().ToLower()).ToList();
                                var answerLower = answer.AnswerText.ToLower();
                                var matchedKeywords = keywords.Count(k => answerLower.Contains(k));
                                totalScore += (matchedKeywords / (decimal)keywords.Count) * question.Points;
                            }
                            break;

                        case QuestionType.Essay:
                            // Basic scoring based on word count
                            var wordCount = answer.AnswerText.Split(' ', StringSplitOptions.RemoveEmptyEntries).Length;
                            if (wordCount < 50) totalScore += question.Points * 0.3m;
                            else if (wordCount < 100) totalScore += question.Points * 0.6m;
                            else if (wordCount < 200) totalScore += question.Points * 0.8m;
                            else totalScore += question.Points;
                            break;
                    }
                }

                var percentage = maxScore > 0 ? (totalScore / maxScore) * 100 : 0;

                // Update assessment score
                var assessment = await _context.Assessments.FindAsync(assessmentId);
                if (assessment != null)
                {
                    assessment.Score = Math.Min(percentage, 100);
                    assessment.IsPassed = percentage >= 70;
                    assessment.ModifiedDate = DateTime.UtcNow;
                    await _context.SaveChangesAsync();
                }

                return percentage;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating assessment score for assessment {AssessmentId}", assessmentId);
                return 0;
            }
        }

        public async Task<Assessment?> CreateAssessmentAsync(int applicationId, string assessmentType, string recruiterId, string recruiterName, string recruiterEmail)
        {
            try
            {
                var assessment = new Assessment
                {
                    ApplicationId = applicationId,
                    AssessmentTitle = $"Technical Assessment - {assessmentType}",
                    Instructions = GenerateAssessmentInstructions(assessmentType),
                    SentDate = DateTime.UtcNow,
                    DueDate = DateTime.UtcNow.AddDays(3),
                    AccessToken = Guid.NewGuid().ToString(),
                    AssessmentType = assessmentType,
                    TimeLimitMinutes = 60,
                    CreatedDate = DateTime.UtcNow,
                    ModifiedDate = DateTime.UtcNow,
                    IsActive = true
                };

                _context.Assessments.Add(assessment);
                await _context.SaveChangesAsync();

                // Create recruiter assignment record
                var recruiterAssignment = new RecruiterAssignment
                {
                    AssessmentId = assessment.AssessmentId,
                    RecruiterId = recruiterId,
                    RecruiterName = recruiterName,
                    RecruiterEmail = recruiterEmail,
                    ApplicationId = applicationId,
                    AssignedDate = DateTime.UtcNow,
                    ModifiedDate = DateTime.UtcNow,
                    IsActive = true
                };

                _context.RecruiterAssignments.Add(recruiterAssignment);

                // Assign questions to assessment
                await AssignQuestionsToAssessmentAsync(assessment.AssessmentId, assessmentType);

                await _context.SaveChangesAsync();

                _logger.LogInformation("Assessment {AssessmentId} created and assigned by recruiter {RecruiterId}", 
                    assessment.AssessmentId, recruiterId);

                return assessment;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating assessment for application {ApplicationId}", applicationId);
                return null;
            }
        }

        public async Task<bool> AssignQuestionsToAssessmentAsync(int assessmentId, string assessmentType)
        {
            try
            {
                // Get questions for the assessment type
                var questions = await _context.Questions
                    .Where(q => q.Department == assessmentType && q.IsActive)
                    .OrderBy(q => q.QuestionId)
                    .Take(10) // Limit to 10 questions per assessment
                    .ToListAsync();

                if (!questions.Any())
                {
                    _logger.LogWarning("No questions found for assessment type {AssessmentType}", assessmentType);
                    return false;
                }

                // Assign questions to assessment
                for (int i = 0; i < questions.Count; i++)
                {
                    var assessmentQuestion = new AssessmentQuestion
                    {
                        AssessmentId = assessmentId,
                        QuestionId = questions[i].QuestionId,
                        Order = i + 1,
                        CreatedDate = DateTime.UtcNow
                    };

                    _context.AssessmentQuestions.Add(assessmentQuestion);
                }

                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error assigning questions to assessment {AssessmentId}", assessmentId);
                return false;
            }
        }

        private string GenerateAssessmentInstructions(string assessmentType)
        {
            return assessmentType switch
            {
                "Mechanical Engineer" => "This assessment evaluates your technical knowledge in mechanical engineering principles, problem-solving skills, and practical applications. Please answer all questions to the best of your ability.",
                "Sales Representative" => "This assessment tests your sales knowledge, communication skills, and customer service abilities. Take your time to provide thoughtful responses.",
                "Driver" => "This assessment covers road safety, vehicle maintenance, and customer service skills essential for a professional driver position.",
                "Desktop Technician" => "This technical assessment evaluates your knowledge of computer hardware, software troubleshooting, and IT support best practices.",
                _ => "Please complete this assessment to the best of your ability. Read each question carefully and provide detailed responses where required."
            };
        }
    }
}
