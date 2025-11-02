using AutoEdge.Data;
using AutoEdge.Models.Entities;
using AutoEdge.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AutoEdge.Controllers
{
    [Authorize(Roles = "Recruiter,Administrator")]
    public class RecruitmentRecruiterController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IRecruitmentEmailService _emailService;
        private readonly IAssessmentService _assessmentService;
        private readonly ILogger<RecruitmentRecruiterController> _logger;

        public RecruitmentRecruiterController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            IRecruitmentEmailService emailService,
            IAssessmentService assessmentService,
            ILogger<RecruitmentRecruiterController> logger)
        {
            _context = context;
            _userManager = userManager;
            _emailService = emailService;
            _assessmentService = assessmentService;
            _logger = logger;
        }

        // GET: RecruitmentRecruiter/Dashboard
        public async Task<IActionResult> Dashboard()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound();
            }

            var upcomingInterviews = await _context.Interviews
                .Include(i => i.Application)
                .Include(i => i.JobPosting)
                .Where(i => i.IsActive && 
                           i.ScheduledDateTime >= DateTime.Now && 
                           i.ScheduledDateTime <= DateTime.Now.AddDays(7) &&
                           i.RecruiterEmail == user.Email)
                .OrderBy(i => i.ScheduledDateTime)
                .ToListAsync();

            var completedInterviews = await _context.Interviews
                .Include(i => i.Application)
                .Include(i => i.JobPosting)
                .Where(i => i.IsActive && 
                           i.IsCompleted && 
                           i.RecruiterEmail == user.Email)
                .OrderByDescending(i => i.CompletedDate)
                .Take(10)
                .ToListAsync();

            ViewBag.UpcomingInterviews = upcomingInterviews;
            ViewBag.CompletedInterviews = completedInterviews;

            return View();
        }

        // GET: RecruitmentRecruiter/MyInterviews
        public async Task<IActionResult> MyInterviews()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound();
            }

            var interviews = await _context.Interviews
                .Include(i => i.Application)
                .Include(i => i.JobPosting)
                .Where(i => i.IsActive && i.RecruiterEmail == user.Email)
                .OrderByDescending(i => i.ScheduledDateTime)
                .ToListAsync();

            return View(interviews);
        }

        // GET: RecruitmentRecruiter/InterviewDetails/5
        public async Task<IActionResult> InterviewDetails(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound();
            }

            var interview = await _context.Interviews
                .Include(i => i.Application)
                .ThenInclude(a => a.JobPosting)
                .FirstOrDefaultAsync(i => i.InterviewId == id && i.RecruiterEmail == user.Email);

            if (interview == null)
            {
                return NotFound();
            }

            return View(interview);
        }

        // POST: RecruitmentRecruiter/CompleteInterview
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CompleteInterview(int interviewId, string interviewNotes, int interviewRating)
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                {
                    return NotFound();
                }

                var interview = await _context.Interviews
                    .Include(i => i.Application)
                    .Include(i => i.JobPosting)
                    .FirstOrDefaultAsync(i => i.InterviewId == interviewId && i.RecruiterEmail == user.Email);

                if (interview == null)
                {
                    return NotFound();
                }

                interview.IsCompleted = true;
                interview.CompletedDate = DateTime.UtcNow;
                interview.InterviewNotes = interviewNotes;
                interview.InterviewRating = Math.Max(1, Math.Min(10, interviewRating)); // Ensure rating is between 1-10
                interview.ModifiedDate = DateTime.UtcNow;

                _context.Update(interview);
                await _context.SaveChangesAsync();

                // Automatically send assessment if interview is completed
                // Use job posting department as default assessment type
                var defaultAssessmentType = interview.JobPosting?.Department ?? "General";
                await SendAssessmentAfterInterview(interview, defaultAssessmentType);

                TempData["SuccessMessage"] = "Interview completed successfully! Assessment has been sent to the candidate.";
                return RedirectToAction(nameof(InterviewDetails), new { id = interviewId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error completing interview {InterviewId}", interviewId);
                TempData["ErrorMessage"] = "Error completing interview. Please try again.";
                return RedirectToAction(nameof(InterviewDetails), new { id = interviewId });
            }
        }

        // GET: RecruitmentRecruiter/JoinInterview/5
        public async Task<IActionResult> JoinInterview(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound();
            }

            var interview = await _context.Interviews
                .Include(i => i.Application)
                .ThenInclude(a => a.JobPosting)
                .FirstOrDefaultAsync(i => i.InterviewId == id && i.RecruiterEmail == user.Email);

            if (interview == null)
            {
                return NotFound();
            }

            return View(interview);
        }

        // GET: RecruitmentRecruiter/InterviewNotes/5
        public async Task<IActionResult> InterviewNotes(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound();
            }

            var interview = await _context.Interviews
                .Include(i => i.Application)
                .ThenInclude(a => a.JobPosting)
                .FirstOrDefaultAsync(i => i.InterviewId == id && i.RecruiterEmail == user.Email);

            if (interview == null)
            {
                return NotFound();
            }

            return View(interview);
        }

        // POST: RecruitmentRecruiter/UpdateInterviewNotes
        // [HttpPost]
        // [ValidateAntiForgeryToken]
        // public async Task<IActionResult> UpdateInterviewNotes(int interviewId, string interviewNotes)
        // {
        //     try
        //     {
        //         var user = await _userManager.GetUserAsync(User);
        //         if (user == null)
        //         {
        //             return NotFound();
        //         }

        //         var interview = await _context.Interviews
        //             .FirstOrDefaultAsync(i => i.InterviewId == interviewId && i.RecruiterEmail == user.Email);

        //         if (interview == null)
        //         {
        //             return NotFound();
        //         }

        //         interview.InterviewNotes = interviewNotes;
        //         interview.ModifiedDate = DateTime.UtcNow;

        //         _context.Update(interview);
        //         await _context.SaveChangesAsync();

        //         TempData["SuccessMessage"] = "Interview notes updated successfully!";
        //         return RedirectToAction(nameof(InterviewDetails), new { id = interviewId });
        //     }
        //     catch (Exception ex)
        //     {
        //         _logger.LogError(ex, "Error updating interview notes for interview {InterviewId}", interviewId);
        //         TempData["ErrorMessage"] = "Error updating interview notes. Please try again.";
        //         return RedirectToAction(nameof(InterviewDetails), new { id = interviewId });
        //     }
        // }
        // POST: RecruitmentRecruiter/UpdateInterviewNotes
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateInterviewNotes(int interviewId, string interviewNotes, int? interviewRating, bool markAsCompleted = false, string selectedAssessment = null)
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                {
                    return NotFound();
                }

                var interview = await _context.Interviews
                    .Include(i => i.Application)
                    .Include(i => i.JobPosting)
                    .FirstOrDefaultAsync(i => i.InterviewId == interviewId && i.RecruiterEmail == user.Email);

                if (interview == null)
                {
                    return NotFound();
                }

                // Update notes
                interview.InterviewNotes = interviewNotes;
                interview.ModifiedDate = DateTime.UtcNow;

                // Handle completion if checkbox is checked
                if (markAsCompleted && interviewRating.HasValue)
                {
                    interview.IsCompleted = true;
                    interview.CompletedDate = DateTime.UtcNow;
                    interview.InterviewRating = Math.Max(1, Math.Min(10, interviewRating.Value));
                    
                    // Save interview changes first
                    _context.Update(interview);
                    await _context.SaveChangesAsync();
                    
                    // Automatically send assessment if interview is completed
                    if (!string.IsNullOrEmpty(selectedAssessment))
                    {
                        await SendAssessmentAfterInterview(interview, selectedAssessment);
                        TempData["SuccessMessage"] = $"Interview completed successfully! {selectedAssessment} assessment has been sent to the candidate.";
                    }
                    else
                    {
                        TempData["ErrorMessage"] = "Please select an assessment type before completing the interview.";
                        return RedirectToAction(nameof(InterviewNotes), new { id = interviewId });
                    }
                }
                else
                {
                    _context.Update(interview);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Interview notes updated successfully!";
                }

                return RedirectToAction(nameof(InterviewDetails), new { id = interviewId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating interview notes for interview {InterviewId}", interviewId);
                TempData["ErrorMessage"] = "Error updating interview notes. Please try again.";
                return RedirectToAction(nameof(InterviewDetails), new { id = interviewId });
            }
        }
        // GET: RecruitmentRecruiter/CandidateProfile/5
        public async Task<IActionResult> CandidateProfile(int applicationId)
        {
            var application = await _context.Applications
                .Include(a => a.JobPosting)
                .Include(a => a.Interviews)
                .Include(a => a.Assessments)
                .FirstOrDefaultAsync(a => a.ApplicationId == applicationId);

            if (application == null)
            {
                return NotFound();
            }

            return View(application);
        }

        private async Task SendAssessmentAfterInterview(Interview interview, string selectedAssessment)
        {
            try
            {
                // Load the interview with job posting information
                var interviewWithJob = await _context.Interviews
                    .Include(i => i.JobPosting)
                    .Include(i => i.Application)
                    .FirstOrDefaultAsync(i => i.InterviewId == interview.InterviewId);

                if (interviewWithJob?.JobPosting == null)
                {
                    _logger.LogError("Job posting not found for interview {InterviewId}", interview.InterviewId);
                    return;
                }

                // Get current user information
                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                {
                    _logger.LogError("User not found when creating assessment for interview {InterviewId}", interview.InterviewId);
                    return;
                }

                // Create assessment using the AssessmentService
                var assessment = await _assessmentService.CreateAssessmentAsync(
                    interviewWithJob.ApplicationId,
                    selectedAssessment,
                    user.Id,
                    user.UserName ?? user.Email ?? "Unknown",
                    user.Email ?? "unknown@example.com"
                );

                if (assessment == null)
                {
                    _logger.LogError("Failed to create assessment for interview {InterviewId}", interview.InterviewId);
                    return;
                }

                // Send assessment email
                if (interviewWithJob.Application != null)
                {
                    await _emailService.SendAssessmentEmailAsync(assessment, interviewWithJob.Application);
                }

                _logger.LogInformation("Assessment created and sent to application {ApplicationId} after interview completion", interviewWithJob.ApplicationId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending assessment after interview {InterviewId}", interview.InterviewId);
            }
        }

        private string GenerateAssessmentInstructions(string department)
        {
            return department.ToLower() switch
            {
                "mechanical engineer" => "This assessment evaluates your technical knowledge in mechanical engineering, CAD design, and problem-solving skills. You have 60 minutes to complete all questions.",
                "sales representative" => "This assessment evaluates your sales skills, customer service abilities, and product knowledge. You have 60 minutes to complete all questions.",
                "driver" => "This assessment evaluates your knowledge of road safety, vehicle maintenance, and transportation regulations. You have 60 minutes to complete all questions.",
                "desktop technician" => "This assessment evaluates your IT troubleshooting skills, hardware knowledge, and software support abilities. You have 60 minutes to complete all questions.",
                _ => "This assessment evaluates your skills and knowledge relevant to the position. You have 60 minutes to complete all questions."
            };
        }

        private string GenerateAssessmentQuestions(string department)
        {
            var questions = department.ToLower() switch
            {
                "mechanical engineer" => new List<object>
                {
                    new { Id = "q1", Type = "multiplechoice", Question = "What is the primary purpose of CAD software in mechanical engineering?", Options = new[] { "Documentation", "Design and modeling", "Cost estimation", "Quality control" }, CorrectAnswer = "Design and modeling", Points = 10 },
                    new { Id = "q2", Type = "shortanswer", Question = "Explain the difference between stress and strain in materials.", CorrectAnswer = "stress, strain, force, deformation", Points = 15 },
                    new { Id = "q3", Type = "essay", Question = "Describe your experience with 3D modeling and simulation software.", Points = 20 }
                },
                "sales representative" => new List<object>
                {
                    new { Id = "q1", Type = "multiplechoice", Question = "What is the most important factor in building customer relationships?", Options = new[] { "Price", "Trust and communication", "Product features", "Speed of delivery" }, CorrectAnswer = "Trust and communication", Points = 10 },
                    new { Id = "q2", Type = "shortanswer", Question = "How would you handle a customer complaint about a delayed delivery?", CorrectAnswer = "apologize, investigate, solution, follow-up", Points = 15 },
                    new { Id = "q3", Type = "essay", Question = "Describe your approach to identifying and qualifying sales leads.", Points = 20 }
                },
                "driver" => new List<object>
                {
                    new { Id = "q1", Type = "multiplechoice", Question = "What is the maximum speed limit in a school zone?", Options = new[] { "25 mph", "30 mph", "35 mph", "40 mph" }, CorrectAnswer = "25 mph", Points = 10 },
                    new { Id = "q2", Type = "shortanswer", Question = "What should you check before starting your vehicle each day?", CorrectAnswer = "tires, brakes, lights, fluids, mirrors", Points = 15 },
                    new { Id = "q3", Type = "essay", Question = "Describe your experience with long-distance driving and route planning.", Points = 20 }
                },
                "desktop technician" => new List<object>
                {
                    new { Id = "q1", Type = "multiplechoice", Question = "What does RAM stand for in computer terminology?", Options = new[] { "Random Access Memory", "Read Access Memory", "Rapid Access Memory", "Remote Access Memory" }, CorrectAnswer = "Random Access Memory", Points = 10 },
                    new { Id = "q2", Type = "shortanswer", Question = "How would you troubleshoot a computer that won't start?", CorrectAnswer = "power, cables, hardware, BIOS, operating system", Points = 15 },
                    new { Id = "q3", Type = "essay", Question = "Describe your experience with network troubleshooting and user support.", Points = 20 }
                },
                _ => new List<object>
                {
                    new { Id = "q1", Type = "multiplechoice", Question = "What is your greatest strength?", Options = new[] { "Communication", "Problem solving", "Teamwork", "Leadership" }, CorrectAnswer = "", Points = 10 },
                    new { Id = "q2", Type = "essay", Question = "Why are you interested in this position?", Points = 20 }
                }
            };

            return System.Text.Json.JsonSerializer.Serialize(questions);
        }
    }
}
