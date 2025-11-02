using AutoEdge.Data;
using AutoEdge.Models.Entities;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace AutoEdge.Services
{
    public class RecruitmentEmailService : IRecruitmentEmailService
    {
        private readonly IEmailSender _emailSender;
        private readonly ApplicationDbContext _context;
        private readonly ILogger<RecruitmentEmailService> _logger;
        private readonly IConfiguration _configuration;

        public RecruitmentEmailService(IEmailSender emailSender, ApplicationDbContext context, ILogger<RecruitmentEmailService> logger, IConfiguration configuration)
        {
            _emailSender = emailSender;
            _context = context;
            _logger = logger;
            _configuration = configuration;
        }

        public async Task SendApplicationConfirmationEmailAsync(Application application)
        {
            try
            {
                var jobPosting = await _context.JobPostings.FindAsync(application.JobId);
                if (jobPosting == null)
                {
                    _logger.LogWarning("Job posting not found for application {ApplicationId}", application.ApplicationId);
                    return;
                }

                var subject = $"Application Received - {jobPosting.JobTitle} at AutoEdge";
                var body = GenerateApplicationConfirmationEmailBody(application, jobPosting);

                await _emailSender.SendEmailAsync(application.Email, subject, body);
                _logger.LogInformation("Application confirmation email sent to {Email}", application.Email);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending application confirmation email to {Email}", application.Email);
            }
        }

        public async Task SendInterviewInvitationEmailAsync(Interview interview, Application application)
        {
            try
            {
                var subject = $"Interview Invitation - {interview.JobPosting?.JobTitle} at AutoEdge";
                var body = GenerateInterviewInvitationEmailBody(interview, application);

                await _emailSender.SendEmailAsync(application.Email, subject, body);
                
                // Update interview record
                interview.EmailSent = true;
                interview.EmailSentDate = DateTime.UtcNow;
                await _context.SaveChangesAsync();
                
                _logger.LogInformation("Interview invitation email sent to {Email}", application.Email);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending interview invitation email to {Email}", application.Email);
            }
        }

        public async Task SendAssessmentEmailAsync(Assessment assessment, Application application)
        {
            try
            {
                var subject = $"Complete Your Assessment - {assessment.AssessmentTitle} at AutoEdge";
                var body = GenerateAssessmentEmailBody(assessment, application);

                await _emailSender.SendEmailAsync(application.Email, subject, body);
                
                // Update assessment record
                assessment.EmailSent = true;
                assessment.EmailSentDate = DateTime.UtcNow;
                await _context.SaveChangesAsync();
                
                _logger.LogInformation("Assessment email sent to {Email}", application.Email);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending assessment email to {Email}", application.Email);
            }
        }

        public async Task SendHiringDecisionEmailAsync(Application application, string decision, string? notes = null)
        {
            try
            {
                var jobPosting = await _context.JobPostings.FindAsync(application.JobId);
                if (jobPosting == null)
                {
                    _logger.LogWarning("Job posting not found for application {ApplicationId}", application.ApplicationId);
                    return;
                }

                string subject;
                string body;

                if (decision.ToLower() == "hire")
                {
                    subject = $"Congratulations! Job Offer - {jobPosting.JobTitle} at AutoEdge";
                    body = GenerateHiringSuccessEmailBody(application, jobPosting, notes);
                }
                else
                {
                    subject = $"Application Update - {jobPosting.JobTitle} at AutoEdge";
                    body = GenerateRejectionEmailBody(application, jobPosting, notes);
                }

                await _emailSender.SendEmailAsync(application.Email, subject, body);
                _logger.LogInformation("Hiring decision email sent to {Email} - Decision: {Decision}", application.Email, decision);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending hiring decision email to {Email}", application.Email);
            }
        }

        public async Task SendBulkInterviewInvitationsAsync(List<Interview> interviews)
        {
            try
            {
                var tasks = new List<Task>();
                
                foreach (var interview in interviews)
                {
                    var application = await _context.Applications
                        .Include(a => a.JobPosting)
                        .FirstOrDefaultAsync(a => a.ApplicationId == interview.ApplicationId);
                    
                    if (application != null)
                    {
                        tasks.Add(SendInterviewInvitationEmailAsync(interview, application));
                    }
                }

                await Task.WhenAll(tasks);
                _logger.LogInformation("Bulk interview invitations sent for {Count} interviews", interviews.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending bulk interview invitations");
            }
        }

        private string GenerateApplicationConfirmationEmailBody(Application application, JobPosting jobPosting)
        {
            return $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'>
    <title>Application Received</title>
</head>
<body style='font-family: Arial, sans-serif; line-height: 1.6; color: #333; max-width: 600px; margin: 0 auto;'>
    <div style='background-color: #1e3a8a; color: white; padding: 20px; text-align: center;'>
        <h1>AutoEdge Recruitment</h1>
    </div>
    
    <div style='padding: 20px;'>
        <h2>Dear {application.FirstName} {application.LastName},</h2>
        
        <p>Thank you for applying for the <strong>{jobPosting.JobTitle}</strong> position at AutoEdge.</p>
        
        <p>We have received your application and our recruitment team will review it.</p>
        
        <div style='background-color: #fef3c7; border-left: 4px solid #f59e0b; padding: 15px; margin: 20px 0;'>
            <p><strong>IMPORTANT:</strong> If you do not hear from us within 3 working days, please consider your application unsuccessful for this position.</p>
        </div>
        
        <p>We appreciate your interest in AutoEdge.</p>
        
        <p>Best regards,<br>
        AutoEdge Recruitment Team</p>
    </div>
    
    <div style='background-color: #f3f4f6; padding: 15px; text-align: center; font-size: 12px; color: #6b7280;'>
        <p>This is an automated message. Please do not reply to this email.</p>
    </div>
</body>
</html>";
        }

        private string GenerateInterviewInvitationEmailBody(Interview interview, Application application)
        {
            return $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'>
    <title>Interview Invitation</title>
</head>
<body style='font-family: Arial, sans-serif; line-height: 1.6; color: #333; max-width: 600px; margin: 0 auto;'>
    <div style='background-color: #1e3a8a; color: white; padding: 20px; text-align: center;'>
        <h1>AutoEdge Recruitment</h1>
    </div>
    
    <div style='padding: 20px;'>
        <h2>Congratulations {application.FirstName}!</h2>
        
        <p>Your application for <strong>{interview.JobPosting?.JobTitle}</strong> has been shortlisted.</p>
        
        <h3>INTERVIEW DETAILS:</h3>
        <div style='background-color: #f8fafc; padding: 15px; border-radius: 5px; margin: 15px 0;'>
            <p>📅 <strong>Date:</strong> {interview.ScheduledDateTime:dddd, dd MMMM yyyy}</p>
            <p>⏰ <strong>Time:</strong> {interview.ScheduledDateTime:HH:mm}</p>
            <p>⏱️ <strong>Duration:</strong> {interview.DurationMinutes} minutes</p>
            <p>👤 <strong>Interviewer:</strong> {interview.RecruiterName}</p>
        </div>
        
        <h3>JOIN VIRTUAL INTERVIEW:</h3>
        <div style='background-color: #ecfdf5; padding: 15px; border-radius: 5px; margin: 15px 0;'>
            <p>🔗 <strong>Meeting Link:</strong> <a href='{interview.MeetingLink}' style='color: #059669;'>{interview.MeetingLink}</a></p>
            {(!string.IsNullOrEmpty(interview.MeetingPassword) ? $"<p>🔑 <strong>Meeting Password:</strong> {interview.MeetingPassword}</p>" : "")}
        </div>
        
        <h3>INSTRUCTIONS:</h3>
        <ul>
            <li>✓ Join 5 minutes before scheduled time</li>
            <li>✓ Ensure stable internet connection</li>
            <li>✓ Test camera and microphone beforehand</li>
            <li>✓ Find a quiet, well-lit location</li>
            <li>✓ Have your resume available</li>
        </ul>
        
        <p>To reschedule, contact us 24 hours in advance.</p>
        
        <p>We look forward to speaking with you!</p>
        
        <p>Best regards,<br>
        {interview.RecruiterName}<br>
        AutoEdge Recruitment Team<br>
        Email: {interview.RecruiterEmail}</p>
    </div>
    
    <div style='background-color: #f3f4f6; padding: 15px; text-align: center; font-size: 12px; color: #6b7280;'>
        <p>This is an automated message. Please do not reply to this email.</p>
    </div>
</body>
</html>";
        }

        private string GenerateAssessmentEmailBody(Assessment assessment, Application application)
        {
            return $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'>
    <title>Assessment Invitation</title>
</head>
<body style='font-family: Arial, sans-serif; line-height: 1.6; color: #333; max-width: 600px; margin: 0 auto;'>
    <div style='background-color: #1e3a8a; color: white; padding: 20px; text-align: center;'>
        <h1>AutoEdge Recruitment</h1>
    </div>
    
    <div style='padding: 20px;'>
        <h2>Dear {application.FirstName},</h2>
        
        <p>Thank you for attending the interview for <strong>{assessment.AssessmentTitle}</strong>.</p>
        
        <p>As the next step, please complete an online assessment.</p>
        
        <h3>ASSESSMENT DETAILS:</h3>
        <div style='background-color: #f8fafc; padding: 15px; border-radius: 5px; margin: 15px 0;'>
            <p>📝 <strong>Assessment:</strong> {assessment.AssessmentTitle}</p>
            <p>⏰ <strong>Time Limit:</strong> {assessment.TimeLimitMinutes} minutes</p>
            <p>📅 <strong>Due Date:</strong> {assessment.DueDate:dddd, dd MMMM yyyy 'at' HH:mm}</p>
        </div>
        
        <h3>ACCESS YOUR ASSESSMENT:</h3>
        <div style='background-color: #ecfdf5; padding: 15px; border-radius: 5px; margin: 15px 0; text-align: center;'>
            <p>🔐 <strong>Login Required:</strong> You need to log into your account to access the assessment</p>
            <p><a href='{GenerateLoginLink()}' style='background-color: #059669; color: white; padding: 12px 24px; text-decoration: none; border-radius: 5px; display: inline-block; font-weight: bold;'>Login to Access Assessment</a></p>
        </div>
        
        <h3>LOGIN INSTRUCTIONS:</h3>
        <ol>
            <li>Click the login button above</li>
            <li>Use your email: <strong>{application.Email}</strong></li>
            <li>If you don't have a password, use the ""Forgot Password"" option</li>
            <li>Once logged in, navigate to ""My Assessments"" to find your assessment</li>
        </ol>
        
        <h3>IMPORTANT:</h3>
        <ul>
            <li>Must be completed before due date</li>
            <li>Once started, cannot be paused</li>
            <li>Answer all questions</li>
            <li>Ensure stable internet connection</li>
        </ul>
        
        <p>Good luck!</p>
        
        <p>AutoEdge Recruitment Team</p>
    </div>
    
    <div style='background-color: #f3f4f6; padding: 15px; text-align: center; font-size: 12px; color: #6b7280;'>
        <p>This is an automated message. Please do not reply to this email.</p>
    </div>
</body>
</html>";
        }

        private string GenerateHiringSuccessEmailBody(Application application, JobPosting jobPosting, string? notes)
        {
            return $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'>
    <title>Job Offer</title>
</head>
<body style='font-family: Arial, sans-serif; line-height: 1.6; color: #333; max-width: 600px; margin: 0 auto;'>
    <div style='background-color: #059669; color: white; padding: 20px; text-align: center;'>
        <h1>🎉 Congratulations!</h1>
    </div>
    
    <div style='padding: 20px;'>
        <h2>Dear {application.FirstName} {application.LastName},</h2>
        
        <p>Congratulations! We are pleased to offer you the position of <strong>{jobPosting.JobTitle}</strong> at AutoEdge.</p>
        
        <p>Your performance throughout the recruitment process has been outstanding.</p>
        
        <h3>NEXT STEPS:</h3>
        <ol>
            <li>Review attached employment contract</li>
            <li>Complete pre-employment documentation</li>
            <li>Schedule onboarding session</li>
        </ol>
        
        <p>Please confirm your acceptance by <strong>{DateTime.Now.AddDays(7):dddd, dd MMMM yyyy}</strong>.</p>
        
        <p>Welcome to the AutoEdge team!</p>
        
        <p>Best regards,<br>
        AutoEdge HR Team</p>
    </div>
    
    <div style='background-color: #f3f4f6; padding: 15px; text-align: center; font-size: 12px; color: #6b7280;'>
        <p>This is an automated message. Please do not reply to this email.</p>
    </div>
</body>
</html>";
        }

        private string GenerateRejectionEmailBody(Application application, JobPosting jobPosting, string? notes)
        {
            return $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'>
    <title>Application Update</title>
</head>
<body style='font-family: Arial, sans-serif; line-height: 1.6; color: #333; max-width: 600px; margin: 0 auto;'>
    <div style='background-color: #1e3a8a; color: white; padding: 20px; text-align: center;'>
        <h1>AutoEdge Recruitment</h1>
    </div>
    
    <div style='padding: 20px;'>
        <h2>Dear {application.FirstName} {application.LastName},</h2>
        
        <p>Thank you for your interest in the <strong>{jobPosting.JobTitle}</strong> position.</p>
        
        <p>After careful consideration, we have decided to move forward with other candidates.</p>
        
        <p>We encourage you to apply for future opportunities at AutoEdge.</p>
        
        <p>We wish you success in your career.</p>
        
        <p>Best regards,<br>
        AutoEdge Recruitment Team</p>
    </div>
    
    <div style='background-color: #f3f4f6; padding: 15px; text-align: center; font-size: 12px; color: #6b7280;'>
        <p>This is an automated message. Please do not reply to this email.</p>
    </div>
</body>
</html>";
        }

        private string GenerateLoginLink()
        {
            var baseUrl = _configuration["Assessment:BaseUrl"] ?? "https://localhost:7213";
            return $"{baseUrl}/Identity/Account/Login";
        }

        private string GenerateAssessmentLink(string accessToken)
        {
            var baseUrl = _configuration["Assessment:BaseUrl"] ?? "https://localhost:7213";
            var assessmentPath = _configuration["Assessment:AssessmentPath"] ?? "/RecruitmentApplicant/Assessment";
            return $"{baseUrl}{assessmentPath}/{accessToken}";
        }
    }
}
