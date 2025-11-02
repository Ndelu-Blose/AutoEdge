using AutoEdge.Data;
using AutoEdge.Models.Entities;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace AutoEdge.Controllers
{
    public class EmployeeOnboardingController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IEmailSender _emailSender;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public EmployeeOnboardingController(
            ApplicationDbContext context,
            IEmailSender emailSender,
            IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _emailSender = emailSender;
            _webHostEnvironment = webHostEnvironment;
        }

        // GET: EmployeeOnboarding/ReviewOffer/{token}
        public async Task<IActionResult> ReviewOffer(string token)
        {
            if (string.IsNullOrEmpty(token))
            {
                return NotFound();
            }

            var offer = await _context.EmploymentOffers
                .Include(e => e.Application)
                .FirstOrDefaultAsync(e => e.AccessToken == token && e.IsActive);

            if (offer == null)
            {
                return View("TokenExpired");
            }

            if (offer.OfferExpiryDate < DateTime.UtcNow)
            {
                return View("TokenExpired");
            }

            if (offer.ContractAccepted)
            {
                return RedirectToAction("Documentation", new { token = token });
            }

            return View(offer);
        }

        // POST: EmployeeOnboarding/AcceptContract
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AcceptContract(string token, string signature, string hasReadContract, string acceptsTerms)
        {
            // Debug logging
            Console.WriteLine("=== AcceptContract Action Called ===");
            Console.WriteLine($"token: {token}");
            Console.WriteLine($"hasReadContract: {hasReadContract}");
            Console.WriteLine($"acceptsTerms: {acceptsTerms}");
            Console.WriteLine($"signature length: {signature?.Length ?? 0}");

            // Convert string values to boolean
            bool hasReadContractBool = hasReadContract == "true";
            bool acceptsTermsBool = acceptsTerms == "true";

            // Temporary bypass for testing
            if (string.IsNullOrEmpty(token))
            {
                Console.WriteLine("Token is empty - redirecting to ReviewOffer");
                TempData["ErrorMessage"] = "Invalid token.";
                return RedirectToAction("ReviewOffer", new { token = token });
            }

            if (!hasReadContractBool || !acceptsTermsBool || string.IsNullOrEmpty(signature))
            {
                Console.WriteLine("Validation failed:");
                Console.WriteLine($"hasReadContractBool: {hasReadContractBool}");
                Console.WriteLine($"acceptsTermsBool: {acceptsTermsBool}");
                Console.WriteLine($"signature empty: {string.IsNullOrEmpty(signature)}");
                
                TempData["ErrorMessage"] = "Please complete all required fields and provide your signature.";
                return RedirectToAction("ReviewOffer", new { token = token });
            }

            var offer = await _context.EmploymentOffers
                .Include(e => e.Application)
                .FirstOrDefaultAsync(e => e.AccessToken == token && e.IsActive);

            if (offer == null || offer.OfferExpiryDate < DateTime.UtcNow)
            {
                return View("TokenExpired");
            }

            offer.ContractAccepted = true;
            offer.ContractAcceptedDate = DateTime.UtcNow;
            offer.ContractSignature = signature;
            offer.Status = "ContractAccepted";
            offer.UpdatedDate = DateTime.UtcNow;

            _context.Update(offer);
            await _context.SaveChangesAsync();

            // Send confirmation email
            await SendContractAcceptanceEmail(offer);

            Console.WriteLine("=== Contract Accepted Successfully ===");
            TempData["SuccessMessage"] = "Contract accepted successfully! You can now proceed to complete your pre-employment documentation.";
            return RedirectToAction("Documentation", new { token = token });
        }

        // POST: EmployeeOnboarding/DeclineOffer
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeclineOffer(string token, string rejectionReason)
        {
            if (string.IsNullOrEmpty(token) || string.IsNullOrEmpty(rejectionReason))
            {
                TempData["ErrorMessage"] = "Please provide a reason for declining the offer.";
                return RedirectToAction("ReviewOffer", new { token = token });
            }

            var offer = await _context.EmploymentOffers
                .Include(e => e.Application)
                .FirstOrDefaultAsync(e => e.AccessToken == token && e.IsActive);

            if (offer == null)
            {
                return View("TokenExpired");
            }

            offer.Status = "ContractRejected";
            offer.RejectionReason = rejectionReason;
            offer.UpdatedDate = DateTime.UtcNow;

            _context.Update(offer);
            await _context.SaveChangesAsync();

            // Send notification email to admin
            await SendOfferDeclineNotification(offer);

            return View("OfferDeclined", offer);
        }

        // GET: EmployeeOnboarding/Documentation/{token}
        public async Task<IActionResult> Documentation(string token)
        {
            if (string.IsNullOrEmpty(token))
            {
                return NotFound();
            }

            var offer = await _context.EmploymentOffers
                .Include(e => e.Application)
                .Include(e => e.PreEmploymentDocumentation)
                .FirstOrDefaultAsync(e => e.AccessToken == token && e.IsActive);

            if (offer == null)
            {
                return View("TokenExpired");
            }

            if (!offer.ContractAccepted)
            {
                return RedirectToAction("ReviewOffer", new { token = token });
            }

            if (offer.PreEmploymentDocumentation?.IsCompleted == true)
            {
                return View("DocumentationCompleted", offer);
            }

            var model = offer.PreEmploymentDocumentation ?? new PreEmploymentDocumentation
            {
                OfferId = offer.OfferId
            };

            return View(model);
        }

        // POST: EmployeeOnboarding/SaveDocumentation
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveDocumentation(PreEmploymentDocumentation model, string token)
        {
            if (string.IsNullOrEmpty(token))
            {
                return NotFound();
            }

            var offer = await _context.EmploymentOffers
                .FirstOrDefaultAsync(e => e.AccessToken == token && e.IsActive);

            if (offer == null)
            {
                return View("TokenExpired");
            }

            if (!ModelState.IsValid)
            {
                return View("Documentation", model);
            }

            // Handle file uploads
            await HandleFileUploads(model);

            model.OfferId = offer.OfferId;
            model.UpdatedDate = DateTime.UtcNow;

            if (model.DocumentationId == 0)
            {
                _context.Add(model);
            }
            else
            {
                _context.Update(model);
            }

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Documentation saved successfully!";
            return RedirectToAction("Documentation", new { token = token });
        }

        // POST: EmployeeOnboarding/SubmitDocumentation
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SubmitDocumentation(string token, string signature, string allDeclarationsAccepted)
        {
            // Debug logging
            Console.WriteLine("=== SubmitDocumentation Action Called ===");
            Console.WriteLine($"token: {token}");
            Console.WriteLine($"signature length: {signature?.Length ?? 0}");
            Console.WriteLine($"allDeclarationsAccepted: {allDeclarationsAccepted}");
            
            // Convert string to boolean
            bool allDeclarationsAcceptedBool = allDeclarationsAccepted == "true";
            Console.WriteLine($"allDeclarationsAcceptedBool: {allDeclarationsAcceptedBool}");
            
            if (string.IsNullOrEmpty(token) || !allDeclarationsAcceptedBool || string.IsNullOrEmpty(signature))
            {
                Console.WriteLine("Validation failed:");
                Console.WriteLine($"token empty: {string.IsNullOrEmpty(token)}");
                Console.WriteLine($"allDeclarationsAcceptedBool: {allDeclarationsAcceptedBool}");
                Console.WriteLine($"signature empty: {string.IsNullOrEmpty(signature)}");
                
                TempData["ErrorMessage"] = "Please complete all required fields and accept all declarations.";
                return RedirectToAction("Documentation", new { token = token });
            }

            var offer = await _context.EmploymentOffers
                .Include(e => e.PreEmploymentDocumentation)
                .FirstOrDefaultAsync(e => e.AccessToken == token && e.IsActive);

            if (offer == null)
            {
                return View("TokenExpired");
            }

            if (offer.PreEmploymentDocumentation == null)
            {
                TempData["ErrorMessage"] = "Please complete the documentation form first.";
                return RedirectToAction("Documentation", new { token = token });
            }

            var documentation = offer.PreEmploymentDocumentation;
            documentation.IsCompleted = true;
            documentation.CompletedDate = DateTime.UtcNow;
            documentation.DigitalSignature = signature;
            documentation.SignedDate = DateTime.UtcNow;
            documentation.UpdatedDate = DateTime.UtcNow;

            offer.Status = "DocumentationCompleted";
            offer.UpdatedDate = DateTime.UtcNow;

            _context.Update(documentation);
            _context.Update(offer);
            await _context.SaveChangesAsync();

            // Send confirmation email
            await SendDocumentationSubmissionEmail(offer);

            Console.WriteLine("=== Documentation Submitted Successfully ===");
            return View("DocumentationSubmitted", offer);
        }

        // GET: EmployeeOnboarding/UploadFile
        [HttpPost]
        public async Task<IActionResult> UploadFile(IFormFile file, string token, string fileType)
        {
            if (file == null || file.Length == 0)
            {
                return Json(new { success = false, message = "No file selected." });
            }

            if (file.Length > 5 * 1024 * 1024) // 5MB limit
            {
                return Json(new { success = false, message = "File size cannot exceed 5MB." });
            }

            var allowedExtensions = new[] { ".pdf", ".jpg", ".jpeg", ".png" };
            var fileExtension = Path.GetExtension(file.FileName).ToLowerInvariant();
            
            if (!allowedExtensions.Contains(fileExtension))
            {
                return Json(new { success = false, message = "Only PDF, JPG, and PNG files are allowed." });
            }

            try
            {
                var uploadsPath = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", "employment");
                if (!Directory.Exists(uploadsPath))
                {
                    Directory.CreateDirectory(uploadsPath);
                }

                var fileName = $"{token}_{fileType}_{Guid.NewGuid()}{fileExtension}";
                var filePath = Path.Combine(uploadsPath, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                var relativePath = $"/uploads/employment/{fileName}";
                return Json(new { success = true, filePath = relativePath, fileName = fileName });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error uploading file: " + ex.Message });
            }
        }

        private async Task HandleFileUploads(PreEmploymentDocumentation model)
        {
            // This method would handle the file uploads and set the appropriate paths
            // Implementation depends on how files are uploaded in the form
        }

        private async Task SendContractAcceptanceEmail(EmploymentOffer offer)
        {
            var application = await _context.Applications
                .FirstOrDefaultAsync(a => a.ApplicationId == offer.ApplicationId);

            if (application == null) return;

            var emailBody = $@"
                <div style='background-color: #28a745; color: white; padding: 20px; text-align: center; border-radius: 8px 8px 0 0;'>
                    <h1 style='margin: 0; font-size: 24px;'>✅ Contract Accepted!</h1>
                </div>
                <div style='background-color: white; padding: 30px; border: 1px solid #ddd; border-top: none; border-radius: 0 0 8px 8px;'>
                    <p style='font-size: 18px; font-weight: bold; margin-bottom: 20px;'>Dear {application.FirstName} {application.LastName},</p>
                    
                    <p style='font-size: 16px; line-height: 1.6; margin-bottom: 20px;'>
                        Thank you for accepting our employment offer! We're excited to have you join the AutoEdge team.
                    </p>

                    <p style='font-size: 16px; line-height: 1.6; margin-bottom: 20px;'>
                        Your next step is to complete the pre-employment documentation. This includes providing your personal information, banking details, and uploading required documents.
                    </p>

                    <div style='text-align: center; margin: 30px 0;'>
                        <a href='{Url.Action("Documentation", "EmployeeOnboarding", new { token = offer.AccessToken }, Request.Scheme)}' 
                           style='background-color: #007bff; color: white; padding: 15px 30px; text-decoration: none; border-radius: 5px; font-size: 16px; font-weight: bold; display: inline-block;'>
                            Complete Documentation
                        </a>
                    </div>

                    <p style='font-size: 16px; line-height: 1.6; margin: 20px 0;'>
                        Best regards,<br>
                        AutoEdge HR Team
                    </p>
                </div>";

            await _emailSender.SendEmailAsync(
                application.Email,
                "Contract Accepted - Next Steps",
                emailBody);
        }

        private async Task SendOfferDeclineNotification(EmploymentOffer offer)
        {
            var application = await _context.Applications
                .FirstOrDefaultAsync(a => a.ApplicationId == offer.ApplicationId);

            if (application == null) return;

            // Send notification to admin
            var adminEmails = new[] { "hr@autoedge.co.za", "admin@autoedge.co.za" };
            
            foreach (var email in adminEmails)
            {
                var emailBody = $@"
                    <h2>Employment Offer Declined</h2>
                    <p><strong>Applicant:</strong> {application.FirstName} {application.LastName}</p>
                    <p><strong>Position:</strong> {offer.JobTitle}</p>
                    <p><strong>Reason:</strong> {offer.RejectionReason}</p>
                    <p><strong>Date:</strong> {DateTime.UtcNow:yyyy-MM-dd HH:mm}</p>";

                await _emailSender.SendEmailAsync(email, "Employment Offer Declined", emailBody);
            }
        }

        private async Task SendDocumentationSubmissionEmail(EmploymentOffer offer)
        {
            var application = await _context.Applications
                .FirstOrDefaultAsync(a => a.ApplicationId == offer.ApplicationId);

            if (application == null) return;

            var emailBody = $@"
                <div style='background-color: #28a745; color: white; padding: 20px; text-align: center; border-radius: 8px 8px 0 0;'>
                    <h1 style='margin: 0; font-size: 24px;'>📋 Documentation Submitted!</h1>
                </div>
                <div style='background-color: white; padding: 30px; border: 1px solid #ddd; border-top: none; border-radius: 0 0 8px 8px;'>
                    <p style='font-size: 18px; font-weight: bold; margin-bottom: 20px;'>Dear {application.FirstName} {application.LastName},</p>
                    
                    <p style='font-size: 16px; line-height: 1.6; margin-bottom: 20px;'>
                        Thank you for completing your pre-employment documentation.
                    </p>
                    
                    <p style='font-size: 16px; line-height: 1.6; margin-bottom: 20px;'>
                        We have successfully received all your information and documents.
                    </p>

                    <div style='background-color: #e3f2fd; padding: 20px; border-radius: 8px; margin: 20px 0;'>
                        <h3 style='color: #1976d2; margin-top: 0;'>NEXT STEPS:</h3>
                        <ul style='color: #1976d2;'>
                            <li style='margin: 8px 0;'>✓ Our HR team will review your documentation</li>
                            <li style='margin: 8px 0;'>✓ You will be contacted within 2 working days</li>
                            <li style='margin: 8px 0;'>✓ We will schedule your onboarding session</li>
                            <li style='margin: 8px 0;'>✓ Onboarding details will be sent via email</li>
                        </ul>
                    </div>

                    <p style='font-size: 16px; line-height: 1.6; margin: 20px 0;'>
                        <strong>Reference Number:</strong> REF{offer.OfferId:D6}<br>
                        <strong>Submitted:</strong> {DateTime.UtcNow:dd MMMM yyyy HH:mm}
                    </p>

                    <p style='font-size: 16px; line-height: 1.6; margin: 20px 0;'>
                        Welcome to the AutoEdge family!
                    </p>

                    <p style='font-size: 16px; line-height: 1.6; margin: 20px 0;'>
                        Best regards,<br>
                        AutoEdge HR Team
                    </p>
                </div>";

            await _emailSender.SendEmailAsync(
                application.Email,
                "Pre-Employment Documentation Received - AutoEdge",
                emailBody);
        }
    }
}
