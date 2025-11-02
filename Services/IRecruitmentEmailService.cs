using AutoEdge.Models.Entities;

namespace AutoEdge.Services
{
    public interface IRecruitmentEmailService
    {
        Task SendApplicationConfirmationEmailAsync(Application application);
        Task SendInterviewInvitationEmailAsync(Interview interview, Application application);
        Task SendAssessmentEmailAsync(Assessment assessment, Application application);
        Task SendHiringDecisionEmailAsync(Application application, string decision, string? notes = null);
        Task SendBulkInterviewInvitationsAsync(List<Interview> interviews);
    }
}
