using AutoEdge.Data;
using AutoEdge.Models.Entities;
using AutoEdge.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AutoEdge.Controllers
{
    [Authorize(Roles = "Administrator")]
    public class RecruitmentAdminController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IResumeParserService _resumeParserService;
        private readonly IRecruitmentEmailService _emailService;
        private readonly IVideoMeetingService _videoMeetingService;
        private readonly IAIRecruitmentService _aiRecruitmentService;
        private readonly IInterviewSchedulingService _interviewSchedulingService;
        private readonly ILogger<RecruitmentAdminController> _logger;

        public RecruitmentAdminController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            IResumeParserService resumeParserService,
            IRecruitmentEmailService emailService,
            IVideoMeetingService videoMeetingService,
            IAIRecruitmentService aiRecruitmentService,
            IInterviewSchedulingService interviewSchedulingService,
            ILogger<RecruitmentAdminController> logger)
        {
            _context = context;
            _userManager = userManager;
            _resumeParserService = resumeParserService;
            _emailService = emailService;
            _videoMeetingService = videoMeetingService;
            _aiRecruitmentService = aiRecruitmentService;
            _interviewSchedulingService = interviewSchedulingService;
            _logger = logger;
        }

        // GET: RecruitmentAdmin
        public async Task<IActionResult> Index()
        {
            var jobPostings = await _context.JobPostings
                .Where(j => j.IsActive)
                .OrderByDescending(j => j.PostedDate)
                .ToListAsync();

            return View(jobPostings);
        }

        // GET: RecruitmentAdmin/JobPostings
        public async Task<IActionResult> JobPostings()
        {
            var jobPostings = await _context.JobPostings
                .Include(j => j.CreatedBy)
                .Where(j => j.IsActive)
                .OrderByDescending(j => j.PostedDate)
                .ToListAsync();

            return View(jobPostings);
        }

        // GET: RecruitmentAdmin/CreateJobPosting
        public IActionResult CreateJobPosting()
        {
            var departments = new List<string> { "Mechanical Engineer", "Sales Representative", "Driver", "Desktop Technician" };
            ViewBag.Departments = departments;
            return View();
        }

        // POST: RecruitmentAdmin/CreateJobPosting
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateJobPosting(JobPosting jobPosting)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.GetUserAsync(User);
                if (user != null)
                {
                    jobPosting.CreatedByUserId = user.Id;
                    jobPosting.CreatedDate = DateTime.UtcNow;
                    jobPosting.ModifiedDate = DateTime.UtcNow;
                    jobPosting.IsActive = true;

                    _context.Add(jobPosting);
                    await _context.SaveChangesAsync();
                    
                    TempData["SuccessMessage"] = "Job posting created successfully!";
                    return RedirectToAction(nameof(JobPostings));
                }
            }

            var departments = new List<string> { "Mechanical Engineer", "Sales Representative", "Driver", "Desktop Technician" };
            ViewBag.Departments = departments;
            return View(jobPosting);
        }

        // GET: RecruitmentAdmin/EditJobPosting/5
        public async Task<IActionResult> EditJobPosting(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var jobPosting = await _context.JobPostings.FindAsync(id);
            if (jobPosting == null)
            {
                return NotFound();
            }

            var departments = new List<string> { "Mechanical Engineer", "Sales Representative", "Driver", "Desktop Technician" };
            ViewBag.Departments = departments;
            return View(jobPosting);
        }

        // POST: RecruitmentAdmin/EditJobPosting/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditJobPosting(int id, JobPosting jobPosting)
        {
            if (id != jobPosting.JobId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    jobPosting.ModifiedDate = DateTime.UtcNow;
                    _context.Update(jobPosting);
                    await _context.SaveChangesAsync();
                    
                    TempData["SuccessMessage"] = "Job posting updated successfully!";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!JobPostingExists(jobPosting.JobId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(JobPostings));
            }

            var departments = new List<string> { "Mechanical Engineer", "Sales Representative", "Driver", "Desktop Technician" };
            ViewBag.Departments = departments;
            return View(jobPosting);
        }

        // GET: RecruitmentAdmin/Applications
        public async Task<IActionResult> Applications(int? jobId)
        {
            var query = _context.Applications
                .Include(a => a.JobPosting)
                .Where(a => a.IsActive);

            if (jobId.HasValue)
            {
                query = query.Where(a => a.JobId == jobId.Value);
            }

            var applications = await query
                .OrderByDescending(a => a.SubmittedDate)
                .ToListAsync();

            var jobPostings = await _context.JobPostings
                .Where(j => j.IsActive && j.Status == "Active")
                .OrderBy(j => j.JobTitle)
                .ToListAsync();

            ViewBag.JobPostings = jobPostings;
            ViewBag.SelectedJobId = jobId;

            return View(applications);
        }

        // GET: RecruitmentAdmin/ShortlistCandidates/5
        public async Task<IActionResult> ShortlistCandidates(int jobId)
        {
            try
            {
                _logger.LogInformation("ShortlistCandidates called with jobId: {JobId}", jobId);
                
                var jobPosting = await _context.JobPostings.FindAsync(jobId);
                if (jobPosting == null)
                {
                    _logger.LogWarning("Job posting with ID {JobId} not found", jobId);
                    return NotFound();
                }

                _logger.LogInformation("Found job posting: {JobTitle}", jobPosting.JobTitle);

                // Get shortlisted candidates (70%+ match score) - don't auto-update status to prevent overriding manual decisions
                var shortlistedApplications = await _resumeParserService.GetShortlistedApplicationsAsync(jobId, 70, false);
                if (shortlistedApplications == null)
                {
                    _logger.LogWarning("GetShortlistedApplicationsAsync returned null for jobId: {JobId}", jobId);
                    shortlistedApplications = new List<Application>();
                }
                
                _logger.LogInformation("Retrieved {Count} applications for jobId: {JobId}", shortlistedApplications.Count, jobId);
                
                // Filter to only show applications that are actually shortlisted (status = "Shortlisted" or match score >= 70%)
                shortlistedApplications = shortlistedApplications
                    .Where(a => a.Status == "Shortlisted" || a.MatchScore >= 70)
                    .ToList();
                
                _logger.LogInformation("Filtered to {Count} shortlisted applications", shortlistedApplications.Count);
            
            // Get rejected candidates
            var rejectedApplications = await _context.Applications
                .Where(a => a.JobId == jobId && a.IsActive && a.Status == "Rejected")
                .OrderByDescending(a => a.MatchScore)
                .ToListAsync();

            if (rejectedApplications == null)
            {
                rejectedApplications = new List<Application>();
            }

            // Get all applications for manual shortlisting
            var allApplications = await _context.Applications
                .Where(a => a.JobId == jobId && a.IsActive)
                .OrderByDescending(a => a.MatchScore)
                .ToListAsync();

                ViewBag.ShortlistedApplications = shortlistedApplications;
                ViewBag.RejectedApplications = rejectedApplications;
                ViewBag.AllApplications = allApplications;

                return View(jobPosting);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in ShortlistCandidates for jobId: {JobId}", jobId);
                TempData["ErrorMessage"] = "An error occurred while loading the shortlist candidates page.";
                return RedirectToAction("Applications");
            }
        }

        // POST: RecruitmentAdmin/ShortlistCandidates
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ShortlistCandidates(int jobId, List<int> selectedApplicationIds)
        {
            try
            {
                if (selectedApplicationIds != null && selectedApplicationIds.Any())
                {
                    var success = await _resumeParserService.ManuallyShortlistApplicationsAsync(selectedApplicationIds);
                    
                    if (success)
                    {
                        TempData["SuccessMessage"] = $"{selectedApplicationIds.Count} candidate(s) have been shortlisted successfully.";
                    }
                    else
                    {
                        TempData["ErrorMessage"] = "An error occurred while shortlisting candidates.";
                    }
                }
                else
                {
                    TempData["ErrorMessage"] = "Please select at least one candidate to shortlist.";
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "An error occurred while shortlisting candidates.";
                // Log the exception
            }

            return RedirectToAction("ShortlistCandidates", new { jobId = jobId });
        }

        // GET: RecruitmentAdmin/ResetApplications
        public async Task<IActionResult> ResetApplications(int jobId)
        {
            try
            {
                var success = await _resumeParserService.ResetApplicationsToSubmittedAsync(jobId);
                
                if (success)
                {
                    TempData["SuccessMessage"] = "All applications have been reset to Submitted status.";
                }
                else
                {
                    TempData["ErrorMessage"] = "An error occurred while resetting applications.";
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "An error occurred while resetting applications.";
            }

            return RedirectToAction("ShortlistCandidates", new { jobId = jobId });
        }

        // GET: RecruitmentAdmin/AutoShortlist
        public async Task<IActionResult> AutoShortlist(int jobId)
        {
            try
            {
                // Run automatic shortlisting with status updates
                var shortlistedApplications = await _resumeParserService.GetShortlistedApplicationsAsync(jobId, 70, true);
                
                TempData["SuccessMessage"] = $"Automatic shortlisting completed. {shortlistedApplications.Count} candidates shortlisted.";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "An error occurred during automatic shortlisting.";
            }

            return RedirectToAction("ShortlistCandidates", new { jobId = jobId });
        }

        // GET: RecruitmentAdmin/ScheduleInterviews
        public IActionResult ScheduleInterviews(int? jobId)
        {
            if (jobId == null)
            {
                return RedirectToAction("Index");
            }
            
            return RedirectToAction("ShortlistCandidates", new { jobId = jobId });
        }

        // POST: RecruitmentAdmin/ScheduleInterviews
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ScheduleInterviews(int jobId, List<int> selectedApplicationIds, DateTime interviewDate, string recruiterName, string recruiterEmail, int durationMinutes = 60)
        {
            try
            {
                _logger.LogInformation("ScheduleInterviews POST called with jobId: {JobId}", jobId);
                _logger.LogInformation("SelectedApplicationIds: {SelectedIds}", selectedApplicationIds != null ? string.Join(",", selectedApplicationIds) : "null");
                _logger.LogInformation("InterviewDate: {InterviewDate}", interviewDate);
                _logger.LogInformation("RecruiterName: {RecruiterName}", recruiterName);
                _logger.LogInformation("RecruiterEmail: {RecruiterEmail}", recruiterEmail);
                _logger.LogInformation("DurationMinutes: {DurationMinutes}", durationMinutes);
                
                // Validate input parameters
                if (selectedApplicationIds == null || !selectedApplicationIds.Any())
                {
                    _logger.LogWarning("No candidates selected for interview scheduling");
                    TempData["ErrorMessage"] = "Please select at least one candidate to schedule interviews.";
                    return RedirectToAction("ShortlistCandidates", new { jobId = jobId });
                }

                if (string.IsNullOrEmpty(recruiterName) || string.IsNullOrEmpty(recruiterEmail))
                {
                    TempData["ErrorMessage"] = "Recruiter name and email are required.";
                    return RedirectToAction("ShortlistCandidates", new { jobId = jobId });
                }

                if (interviewDate <= DateTime.Now)
                {
                    TempData["ErrorMessage"] = "Interview date must be in the future.";
                    return RedirectToAction("ShortlistCandidates", new { jobId = jobId });
                }

                // Get applications
                var applications = await _context.Applications
                    .Include(a => a.JobPosting)
                    .Where(a => selectedApplicationIds.Contains(a.ApplicationId))
                    .ToListAsync();

                // Use the new interview scheduling service with business hours validation
                var result = await _interviewSchedulingService.CreateInterviewSlotsAsync(
                    applications, 
                    interviewDate, 
                    durationMinutes, 
                    recruiterName, 
                    recruiterEmail
                );

                if (!result.Success)
                {
                    TempData["ErrorMessage"] = string.Join("; ", result.Errors);
                    return RedirectToAction("ShortlistCandidates", new { jobId = jobId });
                }

                // Send bulk interview invitations
                try
                {
                    await _emailService.SendBulkInterviewInvitationsAsync(result.CreatedInterviews);
                    TempData["SuccessMessage"] = $"{result.Summary}. Interview invitations sent to {result.CreatedInterviews.Count} candidates!";
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error sending interview invitation emails");
                    TempData["SuccessMessage"] = $"{result.Summary}, but there was an issue sending email notifications.";
                }

                // Add warnings if any
                if (result.Warnings.Any())
                {
                    TempData["WarningMessage"] = string.Join("; ", result.Warnings);
                }

                return RedirectToAction(nameof(ShortlistCandidates), new { jobId = applications.FirstOrDefault()?.JobId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error scheduling interviews");
                TempData["ErrorMessage"] = "Error scheduling interviews. Please try again.";
                return RedirectToAction(nameof(ShortlistCandidates));
            }
        }

        // GET: RecruitmentAdmin/InterviewSchedule
        public async Task<IActionResult> InterviewSchedule()
        {
            var interviews = await _context.Interviews
                .Include(i => i.Application)
                .Include(i => i.JobPosting)
                .Where(i => i.IsActive && i.ScheduledDateTime >= DateTime.Now)
                .OrderBy(i => i.ScheduledDateTime)
                .ToListAsync();

            return View(interviews);
        }

        // GET: RecruitmentAdmin/AssessmentResults
        public async Task<IActionResult> AssessmentResults()
        {
            var assessments = await _context.Assessments
                .Include(a => a.Application)
                .ThenInclude(app => app.JobPosting)
                .Where(a => a.IsActive && a.IsCompleted)
                .OrderByDescending(a => a.CompletedDate)
                .ToListAsync();

            return View(assessments);
        }

        // GET: RecruitmentAdmin/FinalDecision/5
        public async Task<IActionResult> FinalDecision(int applicationId)
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

        // GET: RecruitmentAdmin/ViewApplication/5
        public async Task<IActionResult> ViewApplication(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var application = await _context.Applications
                .Include(a => a.JobPosting)
                .Include(a => a.Interviews)
                .Include(a => a.Assessments)
                .FirstOrDefaultAsync(a => a.ApplicationId == id);

            if (application == null)
            {
                return NotFound();
            }

            return View(application);
        }

        // GET: RecruitmentAdmin/ViewAssessmentDetails/5
        public async Task<IActionResult> ViewAssessmentDetails(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var assessment = await _context.Assessments
                .Include(a => a.Application)
                .ThenInclude(app => app.JobPosting)
                .Include(a => a.AssessmentQuestions)
                .ThenInclude(aq => aq.Question)
                .FirstOrDefaultAsync(a => a.AssessmentId == id);

            if (assessment == null)
            {
                return NotFound();
            }

            // Get assessment answers if completed
            if (assessment.IsCompleted)
            {
                var answers = await _context.AssessmentAnswers
                    .Include(aa => aa.AssessmentQuestion)
                    .ThenInclude(aq => aq.Question)
                    .Where(aa => aa.ApplicationId == assessment.ApplicationId)
                    .ToListAsync();

                ViewBag.AssessmentAnswers = answers;
            }

            return View(assessment);
        }

        // GET: RecruitmentAdmin/DownloadAssessmentReport/5
        public async Task<IActionResult> DownloadAssessmentReport(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var assessment = await _context.Assessments
                .Include(a => a.Application)
                .ThenInclude(app => app.JobPosting)
                .Include(a => a.AssessmentQuestions)
                .ThenInclude(aq => aq.Question)
                .FirstOrDefaultAsync(a => a.AssessmentId == id);

            if (assessment == null)
            {
                return NotFound();
            }

            if (!assessment.IsCompleted)
            {
                TempData["ErrorMessage"] = "Assessment has not been completed yet.";
                return RedirectToAction("AssessmentResults");
            }

            // Get assessment answers
            var answers = await _context.AssessmentAnswers
                .Include(aa => aa.AssessmentQuestion)
                .ThenInclude(aq => aq.Question)
                .Where(aa => aa.ApplicationId == assessment.ApplicationId)
                .ToListAsync();

            // Generate PDF report (you can implement this based on your PDF service)
            // For now, return a simple text response
            var reportContent = GenerateAssessmentReportText(assessment, answers);
            
            var fileName = $"Assessment_Report_{assessment.Application?.FirstName}_{assessment.Application?.LastName}_{assessment.CompletedDate:yyyyMMdd}.txt";
            
            return File(System.Text.Encoding.UTF8.GetBytes(reportContent), "text/plain", fileName);
        }

        private string GenerateAssessmentReportText(Assessment assessment, List<AssessmentAnswer> answers)
        {
            var report = new System.Text.StringBuilder();
            
            report.AppendLine("ASSESSMENT REPORT");
            report.AppendLine("=================");
            report.AppendLine();
            
            report.AppendLine($"Candidate: {assessment.Application?.FirstName} {assessment.Application?.LastName}");
            report.AppendLine($"Email: {assessment.Application?.Email}");
            report.AppendLine($"Position: {assessment.Application?.JobPosting?.JobTitle}");
            report.AppendLine($"Department: {assessment.Application?.JobPosting?.Department}");
            report.AppendLine();
            
            report.AppendLine($"Assessment: {assessment.AssessmentTitle}");
            report.AppendLine($"Type: {assessment.AssessmentType}");
            report.AppendLine($"Sent Date: {assessment.SentDate:yyyy-MM-dd HH:mm}");
            report.AppendLine($"Due Date: {assessment.DueDate:yyyy-MM-dd HH:mm}");
            report.AppendLine($"Completed Date: {assessment.CompletedDate:yyyy-MM-dd HH:mm}");
            report.AppendLine($"Score: {assessment.Score:F1}%");
            report.AppendLine($"Passed: {(assessment.IsPassed ? "Yes" : "No")}");
            report.AppendLine();
            
            if (!string.IsNullOrEmpty(assessment.GradingNotes))
            {
                report.AppendLine("Grading Notes:");
                report.AppendLine(assessment.GradingNotes);
                report.AppendLine();
            }
            
            report.AppendLine("QUESTIONS AND ANSWERS");
            report.AppendLine("=====================");
            report.AppendLine();
            
            foreach (var answer in answers.OrderBy(a => a.AssessmentQuestion.Order))
            {
                report.AppendLine($"Q{answer.AssessmentQuestion.Order}: {answer.AssessmentQuestion.Question.QuestionText}");
                report.AppendLine($"Answer: {answer.AnswerText}");
                if (answer.Score.HasValue)
                {
                    report.AppendLine($"Score: {answer.Score:F1}");
                }
                if (!string.IsNullOrEmpty(answer.GradingNotes))
                {
                    report.AppendLine($"Notes: {answer.GradingNotes}");
                }
                report.AppendLine();
            }
            
            return report.ToString();
        }

        // POST: RecruitmentAdmin/MakeFinalDecision
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MakeFinalDecision(int applicationId, string decision, string? notes = null)
        {
            try
            {
                var application = await _context.Applications.FindAsync(applicationId);
                if (application == null)
                {
                    return NotFound();
                }

                application.Status = decision;
                application.AdminNotes = notes ?? string.Empty;
                application.ModifiedDate = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                // Send decision email
                await _emailService.SendHiringDecisionEmailAsync(application, decision, notes);

                TempData["SuccessMessage"] = $"Final decision sent to {application.FirstName} {application.LastName}!";
                return RedirectToAction(nameof(AssessmentResults));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error making final decision for application {ApplicationId}", applicationId);
                TempData["ErrorMessage"] = "Error processing final decision. Please try again.";
                return RedirectToAction(nameof(FinalDecision), new { applicationId });
            }
        }

        // AI Auto-Analysis Methods
        [HttpPost]
        public async Task<IActionResult> AutoAnalyzeApplication(int applicationId)
        {
            try
            {
                var application = await _context.Applications
                    .Include(a => a.JobPosting)
                    .FirstOrDefaultAsync(a => a.ApplicationId == applicationId);

                if (application == null)
                {
                    return Json(new { success = false, error = "Application not found" });
                }

                var rating = await _aiRecruitmentService.AutoAnalyzeAndRateApplicationAsync(application, application.JobPosting);
                
                // Update application with AI rating
                application.MatchScore = rating.OverallScore;
                application.Status = rating.Status;
                application.AdminNotes = $"AI Auto-Analysis (Score: {rating.OverallScore:F1}%)\n" +
                                       $"Status: {rating.Status}\n" +
                                       $"Detailed Analysis: {rating.DetailedAnalysis}\n" +
                                       $"Strengths: {string.Join(", ", rating.Strengths)}\n" +
                                       $"Weaknesses: {string.Join(", ", rating.Weaknesses)}\n" +
                                       $"Missing Skills: {string.Join(", ", rating.MissingSkills)}\n" +
                                       $"Matching Skills: {string.Join(", ", rating.MatchingSkills)}\n" +
                                       $"Reasoning: {rating.Reasoning}";
                
                application.ModifiedDate = DateTime.UtcNow;
                await _context.SaveChangesAsync();

                return Json(new { 
                    success = true, 
                    score = rating.OverallScore,
                    status = rating.Status,
                    recommendation = rating.Recommendation,
                    analysis = rating.DetailedAnalysis,
                    strengths = rating.Strengths,
                    weaknesses = rating.Weaknesses
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error auto-analyzing application {ApplicationId}", applicationId);
                return Json(new { success = false, error = "Error analyzing application with AI" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> AutoShortlistAllApplications(int jobId)
        {
            try
            {
                var shortlistedApplications = await _aiRecruitmentService.AutoShortlistApplicationsAsync(jobId);
                
                var jobPosting = await _context.JobPostings.FindAsync(jobId);
                var jobTitle = jobPosting?.JobTitle ?? "Unknown Position";
                
                TempData["SuccessMessage"] = $"AI auto-analysis completed for {jobTitle}. " +
                                           $"Shortlisted: {shortlistedApplications.Count} candidates. " +
                                           $"Check the applications list to see all results.";
                
                return Json(new { 
                    success = true, 
                    shortlistedCount = shortlistedApplications.Count,
                    message = $"AI auto-analysis completed. {shortlistedApplications.Count} candidates shortlisted."
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error auto-shortlisting applications for job {JobId}", jobId);
                return Json(new { success = false, error = "Error auto-shortlisting applications" });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GenerateAnalysisReport(int applicationId)
        {
            try
            {
                var application = await _context.Applications
                    .Include(a => a.JobPosting)
                    .FirstOrDefaultAsync(a => a.ApplicationId == applicationId);

                if (application == null)
                {
                    return NotFound();
                }

                var report = await _aiRecruitmentService.GenerateDetailedAnalysisReportAsync(application, application.JobPosting);
                
                return Content(report, "text/plain");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating analysis report for application {ApplicationId}", applicationId);
                return Content("Error generating analysis report.", "text/plain");
            }
        }

        // Interview Scheduling API Endpoints
        [HttpGet]
        public async Task<IActionResult> GetAvailableTimeSlots(DateTime date, int durationMinutes, string recruiterName)
        {
            try
            {
                var timeSlots = await _interviewSchedulingService.GetAvailableSlotsForDateAsync(date, durationMinutes, recruiterName);
                return Json(new { success = true, timeSlots = timeSlots });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting available time slots for date {Date}", date);
                return Json(new { success = false, error = "Error retrieving time slots" });
            }
        }

        [HttpPost]
        public IActionResult ValidateBusinessHours(DateTime dateTime)
        {
            try
            {
                var isValid = _interviewSchedulingService.IsWithinBusinessHours(dateTime);
                var message = isValid ? "Valid business hours" : "Outside business hours (Monday-Friday, 9:00 AM - 4:00 PM)";
                
                return Json(new { 
                    success = true, 
                    isValid = isValid, 
                    message = message,
                    businessHours = "Monday-Friday, 9:00 AM - 4:00 PM"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating business hours for {DateTime}", dateTime);
                return Json(new { success = false, error = "Error validating business hours" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> AutoScheduleWithIntervals(int jobId, List<int> selectedApplicationIds, DateTime startDate, int durationMinutes, string recruiterName, string recruiterEmail)
        {
            try
            {
                if (selectedApplicationIds == null || !selectedApplicationIds.Any())
                {
                    return Json(new { success = false, error = "Please select at least one candidate" });
                }

                var applications = await _context.Applications
                    .Include(a => a.JobPosting)
                    .Where(a => selectedApplicationIds.Contains(a.ApplicationId))
                    .ToListAsync();

                var result = await _interviewSchedulingService.CreateInterviewSlotsAsync(
                    applications, 
                    startDate, 
                    durationMinutes, 
                    recruiterName, 
                    recruiterEmail
                );

                if (result.Success)
                {
                    // Send email invitations
                    try
                    {
                        await _emailService.SendBulkInterviewInvitationsAsync(result.CreatedInterviews);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error sending interview invitations");
                    }
                }

                return Json(new { 
                    success = result.Success, 
                    message = result.Summary,
                    errors = result.Errors,
                    warnings = result.Warnings,
                    scheduledCount = result.CreatedInterviews.Count
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error auto-scheduling interviews with intervals");
                return Json(new { success = false, error = "Error scheduling interviews" });
            }
        }

        private bool JobPostingExists(int id)
        {
            return _context.JobPostings.Any(e => e.JobId == id);
        }
    }
}
