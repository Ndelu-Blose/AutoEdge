using AutoEdge.Data;
using AutoEdge.Models.Entities;
using AutoEdge.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace AutoEdge.Controllers
{
    public class RecruitmentApplicantController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IResumeParserService _resumeParserService;
        private readonly IRecruitmentEmailService _emailService;
        private readonly IAssessmentService _assessmentService;
        private readonly ILogger<RecruitmentApplicantController> _logger;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly UserManager<ApplicationUser> _userManager;

        public RecruitmentApplicantController(
            ApplicationDbContext context,
            IResumeParserService resumeParserService,
            IRecruitmentEmailService emailService,
            IAssessmentService assessmentService,
            ILogger<RecruitmentApplicantController> logger,
            IWebHostEnvironment webHostEnvironment,
            UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _resumeParserService = resumeParserService;
            _emailService = emailService;
            _assessmentService = assessmentService;
            _logger = logger;
            _webHostEnvironment = webHostEnvironment;
            _userManager = userManager;
        }

        // GET: RecruitmentApplicant/VacantPositions
        public async Task<IActionResult> VacantPositions()
        {
            var jobPostings = await _context.JobPostings
                .Where(j => j.IsActive && j.Status == "Active" && j.ClosingDate > DateTime.Now)
                .OrderByDescending(j => j.PostedDate)
                .ToListAsync();

            return View(jobPostings);
        }

        // GET: RecruitmentApplicant/JobDetails/5
        public async Task<IActionResult> JobDetails(int? id)
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

            return View(jobPosting);
        }

        // GET: RecruitmentApplicant/Apply/5
        [Authorize]
        public async Task<IActionResult> Apply(int? id)
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

            ViewBag.JobPosting = jobPosting;
            return View();
        }

        // POST: RecruitmentApplicant/Apply
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> Apply(int jobId, Application application, IFormFile resumeFile, IFormFile coverLetterFile, IFormFile idDocumentFile, IFormFile certificatesFile)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    // Get the logged-in user
                    var user = await _userManager.GetUserAsync(User);
                    if (user == null)
                    {
                        TempData["ErrorMessage"] = "You must be logged in to apply for jobs.";
                        return RedirectToAction("VacantPositions");
                    }

                    // Set user information from logged-in user
                    application.UserId = user.Id;
                    application.Email = user.Email ?? application.Email; // Use logged-in user's email
                    // application.FirstName = user.FirstName;
                    // application.LastName = user.LastName;

                    var currentUser = await _userManager.GetUserAsync(User);
                    if (currentUser == null)
                    {
                        TempData["ErrorMessage"] = "You must be logged in to apply for jobs.";
                        return RedirectToAction("VacantPositions");
                    }
                    application.FirstName = currentUser.FirstName;
                    application.LastName = currentUser.LastName;

                    // Handle file uploads
                    var uploadsPath = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", "applications");
                    if (!Directory.Exists(uploadsPath))
                    {
                        Directory.CreateDirectory(uploadsPath);
                    }

                    var applicationId = Guid.NewGuid().ToString();
                    var applicationFolder = Path.Combine(uploadsPath, applicationId);
                    Directory.CreateDirectory(applicationFolder);

                    // Save resume file
                    if (resumeFile != null && resumeFile.Length > 0)
                    {
                        var resumeFileName = $"resume_{Path.GetFileName(resumeFile.FileName)}";
                        var resumeFilePath = Path.Combine(applicationFolder, resumeFileName);
                        using (var stream = new FileStream(resumeFilePath, FileMode.Create))
                        {
                            await resumeFile.CopyToAsync(stream);
                        }
                        application.ResumeFilePath = $"/uploads/applications/{applicationId}/{resumeFileName}";
                    }

                    // Save cover letter file
                    if (coverLetterFile != null && coverLetterFile.Length > 0)
                    {
                        var coverLetterFileName = $"cover_letter_{Path.GetFileName(coverLetterFile.FileName)}";
                        var coverLetterFilePath = Path.Combine(applicationFolder, coverLetterFileName);
                        using (var stream = new FileStream(coverLetterFilePath, FileMode.Create))
                        {
                            await coverLetterFile.CopyToAsync(stream);
                        }
                        application.CoverLetterPath = $"/uploads/applications/{applicationId}/{coverLetterFileName}";
                    }

                    // Save ID document file
                    if (idDocumentFile != null && idDocumentFile.Length > 0)
                    {
                        var idDocumentFileName = $"id_document_{Path.GetFileName(idDocumentFile.FileName)}";
                        var idDocumentFilePath = Path.Combine(applicationFolder, idDocumentFileName);
                        using (var stream = new FileStream(idDocumentFilePath, FileMode.Create))
                        {
                            await idDocumentFile.CopyToAsync(stream);
                        }
                        application.IdDocumentPath = $"/uploads/applications/{applicationId}/{idDocumentFileName}";
                    }

                    // Save certificates file
                    if (certificatesFile != null && certificatesFile.Length > 0)
                    {
                        var certificatesFileName = $"certificates_{Path.GetFileName(certificatesFile.FileName)}";
                        var certificatesFilePath = Path.Combine(applicationFolder, certificatesFileName);
                        using (var stream = new FileStream(certificatesFilePath, FileMode.Create))
                        {
                            await certificatesFile.CopyToAsync(stream);
                        }
                        application.CertificatesPath = $"/uploads/applications/{applicationId}/{certificatesFileName}";
                    }

                    // Set application properties
                    application.JobId = jobId;
                    application.SubmittedDate = DateTime.UtcNow;
                    application.CreatedDate = DateTime.UtcNow;
                    application.ModifiedDate = DateTime.UtcNow;
                    application.IsActive = true;
                    application.Status = "Submitted";

                    _context.Applications.Add(application);
                    await _context.SaveChangesAsync();

                    // Parse resume if uploaded
                    if (!string.IsNullOrEmpty(application.ResumeFilePath))
                    {
                        var fullResumePath = Path.Combine(_webHostEnvironment.WebRootPath, application.ResumeFilePath.TrimStart('/'));
                        if (System.IO.File.Exists(fullResumePath))
                        {
                            var parseResult = await _resumeParserService.ParseResumeAsync(fullResumePath);
                            
                            // Update application with parsed data
                            application.ParsedResumeText = parseResult.ExtractedText;
                            //application.YearsOfExperience = parseResult.YearsOfExperience;
                            // CORRECT - Only use parser result if user didn't provide input
                            if (application.YearsOfExperience == 0 && parseResult.YearsOfExperience > 0)
                            {
                                application.YearsOfExperience = parseResult.YearsOfExperience;
                            }
                            application.ExtractedSkills = string.Join(", ", parseResult.Skills);
                            application.ExtractedEducation = string.Join(", ", parseResult.Education);

                            // Calculate match score
                            var jobPosting = await _context.JobPostings.FindAsync(jobId);
                            if (jobPosting != null)
                            {
                                application.MatchScore = await _resumeParserService.CalculateMatchScoreAsync(application, jobPosting);
                            }

                            _context.Update(application);
                            await _context.SaveChangesAsync();
                        }
                    }

                    // Send confirmation email
                    await _emailService.SendApplicationConfirmationEmailAsync(application);

                    // Set popup flag and message for success popup
                    TempData["ShowSuccessPopup"] = true;
                    TempData["SuccessMessage"] = "Application submitted successfully! You will receive a confirmation email shortly.";
                    
                    // Redirect to home page where popup will be shown
                    return RedirectToAction("Index", "Home");
                }

                var jobPostingDetails = await _context.JobPostings.FindAsync(jobId);
                ViewBag.JobPosting = jobPostingDetails;
                return View(application);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error submitting application for job {JobId}", jobId);
                TempData["ErrorMessage"] = "Error submitting application. Please try again.";
                return RedirectToAction(nameof(VacantPositions));
            }
        }

        // GET: RecruitmentApplicant/ApplicationStatus
        public async Task<IActionResult> ApplicationStatus(string email)
        {
            var user = await _userManager.GetUserAsync(User);
            
            if (user != null)
            {
                // If user is logged in, get their applications by UserId
                var applications = await _context.Applications
                    .Include(a => a.JobPosting)
                    .Where(a => a.UserId == user.Id && a.IsActive)
                    .OrderByDescending(a => a.SubmittedDate)
                    .ToListAsync();

                return View(applications);
            }
            else if (!string.IsNullOrEmpty(email))
            {
                // Fallback to email-based lookup for non-logged-in users
            var applications = await _context.Applications
                .Include(a => a.JobPosting)
                .Where(a => a.Email == email && a.IsActive)
                .OrderByDescending(a => a.SubmittedDate)
                .ToListAsync();

            return View(applications);
            }

            return View(new List<Application>());
        }

        // GET: RecruitmentApplicant/MyAssessments
        [Authorize]
        public async Task<IActionResult> MyAssessments()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound();
            }

            _logger.LogInformation("MyAssessments called for user {UserId} ({Email})", user.Id, user.Email);

            // Get assessments for the logged-in user using the service
            var assessments = await _assessmentService.GetAssessmentsForUserAsync(user.Id);

            _logger.LogInformation("Found {Count} assessments for user {UserId}", assessments.Count, user.Id);

            return View(assessments);
        }

        // GET: RecruitmentApplicant/DepartmentQuestionnaire/{department}
        public async Task<IActionResult> DepartmentQuestionnaire(string department, int? jobId)
        {
            if (string.IsNullOrEmpty(department))
            {
                return NotFound();
            }

            // Get job posting if jobId is provided
            JobPosting? jobPosting = null;
            if (jobId.HasValue)
            {
                jobPosting = await _context.JobPostings.FindAsync(jobId.Value);
            }

            // Generate questions for the department
            var questions = GenerateDepartmentQuestions(department);
            var instructions = GenerateDepartmentInstructions(department);

            ViewBag.Department = department;
            ViewBag.JobPosting = jobPosting;
            ViewBag.Questions = questions;
            ViewBag.Instructions = instructions;

            return View();
        }

        // POST: RecruitmentApplicant/SubmitDepartmentQuestionnaire
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SubmitDepartmentQuestionnaire(string department, int? jobId, Dictionary<string, string> answers)
        {
            try
            {
                if (string.IsNullOrEmpty(department))
                {
                    return BadRequest("Department is required.");
                }

                // Get job posting if jobId is provided
                JobPosting? jobPosting = null;
                if (jobId.HasValue)
                {
                    jobPosting = await _context.JobPostings.FindAsync(jobId.Value);
                }

                // Calculate score based on answers
                var questions = GenerateDepartmentQuestions(department);
                var score = CalculateQuestionnaireScore(questions, answers);

                // Store questionnaire results in session for later use when applicant applies
                var questionnaireResult = new
                {
                    Department = department,
                    JobId = jobId,
                    Questions = questions,
                    Answers = answers,
                    Score = score,
                    CompletedDate = DateTime.UtcNow,
                    IsPassed = score >= 70
                };

                HttpContext.Session.SetString($"Questionnaire_{department}_{jobId}", 
                    System.Text.Json.JsonSerializer.Serialize(questionnaireResult));

                TempData["SuccessMessage"] = $"Questionnaire completed successfully! Your score: {score:F1}%";
                TempData["QuestionnaireScore"] = score;
                TempData["QuestionnairePassed"] = score >= 70;

                return RedirectToAction("VacantPositions");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error submitting department questionnaire for {Department}", department);
                TempData["ErrorMessage"] = "Error submitting questionnaire. Please try again.";
                return RedirectToAction("DepartmentQuestionnaire", new { department, jobId });
            }
        }

        // GET: RecruitmentApplicant/Assessment/5
        [Authorize]
        public async Task<IActionResult> Assessment(string token)
        {
            if (string.IsNullOrEmpty(token))
            {
                return NotFound();
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound();
            }

            var assessment = await _assessmentService.GetAssessmentByTokenAsync(token);
            if (assessment == null)
            {
                return NotFound();
            }

            // Verify that the authenticated user has access to this assessment
            if (assessment.Application?.UserId != user.Id)
            {
                _logger.LogWarning("Access denied to assessment {AssessmentId} for user {UserId} ({Email})", 
                    assessment.AssessmentId, user.Id, user.Email);
                return Forbid("You don't have access to this assessment.");
            }

            // Check if assessment is already completed
            if (assessment.IsCompleted)
            {
                return View("AssessmentCompleted", assessment);
            }

            // Check if assessment has expired
            if (assessment.DueDate < DateTime.Now)
            {
                return View("AssessmentExpired", assessment);
            }

            // Get questions for the assessment
            var questions = await _assessmentService.GetQuestionsForAssessmentAsync(assessment.AssessmentId);
            if (!questions.Any())
            {
                _logger.LogWarning("No questions found for assessment {AssessmentId}", assessment.AssessmentId);
                TempData["ErrorMessage"] = "No questions available for this assessment. Please contact support.";
                return RedirectToAction("MyAssessments");
            }

            ViewBag.Questions = questions;
            return View(assessment);
        }



        // POST: RecruitmentApplicant/SubmitAssessment
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SubmitAssessment(string token, Dictionary<string, string> answers)
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                {
                    return NotFound();
                }

                var assessment = await _assessmentService.GetAssessmentByTokenAsync(token);
                if (assessment == null)
                {
                    return NotFound();
                }

                // Verify that the authenticated user has access to this assessment
                if (assessment.Application?.UserId != user.Id)
                {
                    return Forbid("You don't have access to this assessment.");
                }

                if (assessment.IsCompleted)
                {
                    TempData["ErrorMessage"] = "Assessment has already been completed.";
                    return RedirectToAction(nameof(Assessment), new { token });
                }

                if (assessment.DueDate < DateTime.Now)
                {
                    TempData["ErrorMessage"] = "Assessment deadline has passed.";
                    return RedirectToAction(nameof(Assessment), new { token });
                }

                // Convert string keys to int (question IDs)
                var questionAnswers = new Dictionary<int, string>();
                foreach (var answer in answers)
                {
                    if (int.TryParse(answer.Key, out int questionId))
                    {
                        questionAnswers[questionId] = answer.Value;
                    }
                }

                // Submit answers using the service
                var success = await _assessmentService.SubmitAssessmentAnswersAsync(
                    assessment.AssessmentId, 
                    assessment.ApplicationId, 
                    questionAnswers);

                if (!success)
                {
                    TempData["ErrorMessage"] = "Error saving assessment answers. Please try again.";
                    return RedirectToAction(nameof(Assessment), new { token });
                }

                // Calculate and save the score
                var score = await _assessmentService.CalculateAssessmentScoreAsync(
                    assessment.AssessmentId, 
                    assessment.ApplicationId);

                TempData["SuccessMessage"] = $"Assessment completed! Your score: {score:F1}%";
                return RedirectToAction(nameof(Assessment), new { token });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error submitting assessment with token {Token}", token);
                TempData["ErrorMessage"] = "Error submitting assessment. Please try again.";
                return RedirectToAction(nameof(Assessment), new { token });
            }
        }


        private List<object> GenerateDepartmentQuestions(string department)
        {
            return department.ToLower() switch
            {
                "mechanical engineer" => new List<object>
                {
                    new { Id = "q1", Type = "multiplechoice", Question = "What is the primary purpose of CAD software in mechanical engineering?", Options = new[] { "Documentation", "Design and modeling", "Cost estimation", "Quality control" }, CorrectAnswer = "Design and modeling", Points = 10, Required = true },
                    new { Id = "q2", Type = "multiplechoice", Question = "Which material property is most important for structural components?", Options = new[] { "Density", "Tensile strength", "Color", "Thermal conductivity" }, CorrectAnswer = "Tensile strength", Points = 10, Required = true },
                    new { Id = "q3", Type = "shortanswer", Question = "Explain the difference between stress and strain in materials.", CorrectAnswer = "stress, strain, force, deformation", Points = 15, Required = true },
                    new { Id = "q4", Type = "essay", Question = "Describe your experience with 3D modeling and simulation software.", Points = 20, Required = true },
                    new { Id = "q5", Type = "multiplechoice", Question = "What does FEA stand for in engineering?", Options = new[] { "Finite Element Analysis", "Final Engineering Assessment", "Fabrication Engineering Analysis", "Field Engineering Application" }, CorrectAnswer = "Finite Element Analysis", Points = 10, Required = true }
                },
                "sales representative" => new List<object>
                {
                    new { Id = "q1", Type = "multiplechoice", Question = "What is the most important factor in building customer relationships?", Options = new[] { "Price", "Trust and communication", "Product features", "Speed of delivery" }, CorrectAnswer = "Trust and communication", Points = 10, Required = true },
                    new { Id = "q2", Type = "multiplechoice", Question = "Which sales technique involves asking questions to understand customer needs?", Options = new[] { "Cold calling", "Consultative selling", "Hard selling", "Direct marketing" }, CorrectAnswer = "Consultative selling", Points = 10, Required = true },
                    new { Id = "q3", Type = "shortanswer", Question = "How would you handle a customer complaint about a delayed delivery?", CorrectAnswer = "apologize, investigate, solution, follow-up", Points = 15, Required = true },
                    new { Id = "q4", Type = "essay", Question = "Describe your approach to identifying and qualifying sales leads.", Points = 20, Required = true },
                    new { Id = "q5", Type = "multiplechoice", Question = "What is the first step in the sales process?", Options = new[] { "Closing", "Prospecting", "Presentation", "Follow-up" }, CorrectAnswer = "Prospecting", Points = 10, Required = true }
                },
                "driver" => new List<object>
                {
                    new { Id = "q1", Type = "multiplechoice", Question = "What is the maximum speed limit in a school zone?", Options = new[] { "25 mph", "30 mph", "35 mph", "40 mph" }, CorrectAnswer = "25 mph", Points = 10, Required = true },
                    new { Id = "q2", Type = "multiplechoice", Question = "How often should you check your vehicle's tire pressure?", Options = new[] { "Monthly", "Weekly", "Daily", "Only when tires look flat" }, CorrectAnswer = "Monthly", Points = 10, Required = true },
                    new { Id = "q3", Type = "shortanswer", Question = "What should you check before starting your vehicle each day?", CorrectAnswer = "tires, brakes, lights, fluids, mirrors", Points = 15, Required = true },
                    new { Id = "q4", Type = "essay", Question = "Describe your experience with long-distance driving and route planning.", Points = 20, Required = true },
                    new { Id = "q5", Type = "multiplechoice", Question = "What is the minimum following distance in good weather?", Options = new[] { "1 second", "2 seconds", "3 seconds", "4 seconds" }, CorrectAnswer = "3 seconds", Points = 10, Required = true }
                },
                "desktop technician" => new List<object>
                {
                    new { Id = "q1", Type = "multiplechoice", Question = "What does RAM stand for in computer terminology?", Options = new[] { "Random Access Memory", "Read Access Memory", "Rapid Access Memory", "Remote Access Memory" }, CorrectAnswer = "Random Access Memory", Points = 10, Required = true },
                    new { Id = "q2", Type = "multiplechoice", Question = "Which port is commonly used for connecting external hard drives?", Options = new[] { "USB", "VGA", "HDMI", "Ethernet" }, CorrectAnswer = "USB", Points = 10, Required = true },
                    new { Id = "q3", Type = "shortanswer", Question = "How would you troubleshoot a computer that won't start?", CorrectAnswer = "power, cables, hardware, BIOS, operating system", Points = 15, Required = true },
                    new { Id = "q4", Type = "essay", Question = "Describe your experience with network troubleshooting and user support.", Points = 20, Required = true },
                    new { Id = "q5", Type = "multiplechoice", Question = "What is the purpose of a firewall?", Options = new[] { "Speed up internet", "Block unauthorized access", "Store files", "Print documents" }, CorrectAnswer = "Block unauthorized access", Points = 10, Required = true }
                },
                _ => new List<object>
                {
                    new { Id = "q1", Type = "multiplechoice", Question = "What is your greatest strength?", Options = new[] { "Communication", "Problem solving", "Teamwork", "Leadership" }, CorrectAnswer = "", Points = 10, Required = true },
                    new { Id = "q2", Type = "essay", Question = "Why are you interested in this position?", Points = 20, Required = true }
                }
            };
        }

        private string GenerateDepartmentInstructions(string department)
        {
            return department.ToLower() switch
            {
                "mechanical engineer" => "This questionnaire evaluates your technical knowledge in mechanical engineering, CAD design, and problem-solving skills. Please answer all questions accurately to demonstrate your expertise.",
                "sales representative" => "This questionnaire evaluates your sales skills, customer service abilities, and product knowledge. Your responses will help us assess your suitability for the sales role.",
                "driver" => "This questionnaire evaluates your knowledge of road safety, vehicle maintenance, and transportation regulations. Please provide accurate answers based on your driving experience.",
                "desktop technician" => "This questionnaire evaluates your IT troubleshooting skills, hardware knowledge, and software support abilities. Your technical expertise will be assessed through these questions.",
                _ => "This questionnaire evaluates your skills and knowledge relevant to the position. Please answer all questions to the best of your ability."
            };
        }

        private decimal CalculateQuestionnaireScore(List<object> questions, Dictionary<string, string> answers)
        {
            decimal totalPoints = 0;
            decimal earnedPoints = 0;

            foreach (dynamic question in questions)
            {
                totalPoints += question.Points;
                
                    if (answers.ContainsKey(question.Id))
                    {
                        var answer = answers[question.Id];
                    
                    if (question.Type == "multiplechoice")
                    {
                        if (answer.Equals(question.CorrectAnswer, StringComparison.OrdinalIgnoreCase))
                        {
                            earnedPoints += question.Points;
                        }
                    }
                    else if (question.Type == "shortanswer")
                    {
                        // Simple keyword matching for short answers
                        var keywords = question.CorrectAnswer.ToString().Split(',');
                        var answerLower = answer.ToLower();
                        var matchingKeywords = 0;
                        foreach (var keyword in keywords)
                        {
                            if (answerLower.Contains(keyword.Trim().ToLower()))
                            {
                                matchingKeywords++;
                            }
                        }
                        
                        if (keywords.Length > 0)
                        {
                            var percentage = (decimal)matchingKeywords / keywords.Length;
                            earnedPoints += question.Points * percentage;
                        }
                    }
                    else if (question.Type == "essay")
                    {
                        // For essays, give partial credit based on length and content
                        if (!string.IsNullOrWhiteSpace(answer) && answer.Length > 50)
                        {
                            earnedPoints += question.Points * 0.8m; // 80% for substantial answers
                        }
                        else if (!string.IsNullOrWhiteSpace(answer))
                        {
                            earnedPoints += question.Points * 0.5m; // 50% for basic answers
                        }
                    }
                }
            }

            return totalPoints > 0 ? (earnedPoints / totalPoints) * 100 : 0;
        }
    }

    // Assessment question model
    public class AssessmentQuestion
    {
        public string Id { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public string Question { get; set; } = string.Empty;
        public List<string> Options { get; set; } = new List<string>();
        public string? CorrectAnswer { get; set; }
        public int Points { get; set; } = 10;
    }
}
