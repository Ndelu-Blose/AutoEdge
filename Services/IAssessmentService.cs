using AutoEdge.Models.Entities;

namespace AutoEdge.Services
{
    public interface IAssessmentService
    {
        Task<Assessment?> GetAssessmentByTokenAsync(string token);
        Task<List<Assessment>> GetAssessmentsForUserAsync(string userId);
        Task<List<Question>> GetQuestionsForAssessmentAsync(int assessmentId);
        Task<bool> SubmitAssessmentAnswersAsync(int assessmentId, int applicationId, Dictionary<int, string> answers);
        Task<decimal> CalculateAssessmentScoreAsync(int assessmentId, int applicationId);
        Task<Assessment?> CreateAssessmentAsync(int applicationId, string assessmentType, string recruiterId, string recruiterName, string recruiterEmail);
        Task<bool> AssignQuestionsToAssessmentAsync(int assessmentId, string assessmentType);
    }
}
