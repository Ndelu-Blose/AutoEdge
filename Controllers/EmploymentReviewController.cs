using AutoEdge.Data;
using AutoEdge.Models.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace AutoEdge.Controllers
{
    [Authorize(Roles = "Administrator")]
    public class EmploymentReviewController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IEmailSender _emailSender;
        private readonly UserManager<ApplicationUser> _userManager;

        public EmploymentReviewController(
            ApplicationDbContext context,
            IEmailSender emailSender,
            UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _emailSender = emailSender;
            _userManager = userManager;
        }

        // GET: EmploymentReview
        public async Task<IActionResult> Index()
        {
            var documentations = await _context.PreEmploymentDocumentations
                .Include(p => p.EmploymentOffer)
                .ThenInclude(e => e.Application)
                .Where(p => p.IsCompleted && p.IsActive)
                .OrderByDescending(p => p.CompletedDate)
                .ToListAsync();

            return View(documentations);
        }

        // GET: EmploymentReview/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var documentation = await _context.PreEmploymentDocumentations
                .Include(p => p.EmploymentOffer)
                .ThenInclude(e => e.Application)
                .Include(p => p.Reviewer)
                .FirstOrDefaultAsync(m => m.DocumentationId == id);

            if (documentation == null)
            {
                return NotFound();
            }

            return View(documentation);
        }

        // POST: EmploymentReview/Approve/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Approve(int id, string adminNotes, bool scheduleOnboarding)
        {
            var documentation = await _context.PreEmploymentDocumentations
                .Include(p => p.EmploymentOffer)
                .ThenInclude(e => e.Application)
                .FirstOrDefaultAsync(p => p.DocumentationId == id);

            if (documentation == null)
            {
                return NotFound();
            }

            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                return Unauthorized();
            }

            documentation.Approved = true;
            documentation.AdminReviewed = true;
            documentation.AdminReviewedDate = DateTime.UtcNow;
            documentation.ReviewedBy = currentUser.Id;
            documentation.AdminNotes = adminNotes;
            documentation.UpdatedDate = DateTime.UtcNow;

            var offer = documentation.EmploymentOffer;
            offer.Status = "Approved";
            offer.UpdatedDate = DateTime.UtcNow;

            _context.Update(documentation);
            _context.Update(offer);
            await _context.SaveChangesAsync();

            // Send approval email
            await SendApprovalEmail(documentation);

            TempData["SuccessMessage"] = "Documentation approved successfully!";
            return RedirectToAction(nameof(Index));
        }

        // POST: EmploymentReview/RequestCorrections/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RequestCorrections(int id, string correctionRequests)
        {
            if (string.IsNullOrEmpty(correctionRequests))
            {
                TempData["ErrorMessage"] = "Please specify what corrections are needed.";
                return RedirectToAction(nameof(Details), new { id = id });
            }

            var documentation = await _context.PreEmploymentDocumentations
                .Include(p => p.EmploymentOffer)
                .ThenInclude(e => e.Application)
                .FirstOrDefaultAsync(p => p.DocumentationId == id);

            if (documentation == null)
            {
                return NotFound();
            }

            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                return Unauthorized();
            }

            documentation.AdminReviewed = true;
            documentation.AdminReviewedDate = DateTime.UtcNow;
            documentation.ReviewedBy = currentUser.Id;
            documentation.CorrectionRequests = correctionRequests;
            documentation.UpdatedDate = DateTime.UtcNow;

            _context.Update(documentation);
            await _context.SaveChangesAsync();

            // Send correction request email
            await SendCorrectionRequestEmail(documentation);

            TempData["SuccessMessage"] = "Correction request sent to employee successfully!";
            return RedirectToAction(nameof(Index));
        }

        // POST: EmploymentReview/Reject/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Reject(int id, string rejectionReason)
        {
            if (string.IsNullOrEmpty(rejectionReason))
            {
                TempData["ErrorMessage"] = "Please provide a reason for rejection.";
                return RedirectToAction(nameof(Details), new { id = id });
            }

            var documentation = await _context.PreEmploymentDocumentations
                .Include(p => p.EmploymentOffer)
                .ThenInclude(e => e.Application)
                .FirstOrDefaultAsync(p => p.DocumentationId == id);

            if (documentation == null)
            {
                return NotFound();
            }

            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                return Unauthorized();
            }

            documentation.Approved = false;
            documentation.AdminReviewed = true;
            documentation.AdminReviewedDate = DateTime.UtcNow;
            documentation.ReviewedBy = currentUser.Id;
            documentation.AdminNotes = rejectionReason;
            documentation.UpdatedDate = DateTime.UtcNow;

            var offer = documentation.EmploymentOffer;
            offer.Status = "Rejected";
            offer.UpdatedDate = DateTime.UtcNow;

            _context.Update(documentation);
            _context.Update(offer);
            await _context.SaveChangesAsync();

            // Send rejection email
            await SendRejectionEmail(documentation);

            TempData["SuccessMessage"] = "Documentation rejected successfully!";
            return RedirectToAction(nameof(Index));
        }

        // GET: EmploymentReview/DownloadDocument/5
        public async Task<IActionResult> DownloadDocument(int id, string documentType)
        {
            var documentation = await _context.PreEmploymentDocumentations
                .FirstOrDefaultAsync(p => p.DocumentationId == id);

            if (documentation == null)
            {
                return NotFound();
            }

            string? filePath = documentType switch
            {
                "certifiedId" => documentation.CertifiedIdPath,
                "proofOfAddress" => documentation.ProofOfAddressPath,
                "driversLicense" => documentation.DriversLicensePath,
                "bankStatement" => documentation.BankStatementPath,
                "taxClearance" => documentation.TaxClearancePath,
                _ => null
            };

            if (string.IsNullOrEmpty(filePath))
            {
                return NotFound();
            }

            var fullPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", filePath.TrimStart('/'));
            
            if (!System.IO.File.Exists(fullPath))
            {
                return NotFound();
            }

            var fileName = Path.GetFileName(fullPath);
            var contentType = GetContentType(fileName);
            
            return PhysicalFile(fullPath, contentType, fileName);
        }

        // GET: EmploymentReview/ViewDocument/5
        public async Task<IActionResult> ViewDocument(int id, string documentType)
        {
            var documentation = await _context.PreEmploymentDocumentations
                .FirstOrDefaultAsync(p => p.DocumentationId == id);

            if (documentation == null)
            {
                return NotFound();
            }

            string? filePath = documentType switch
            {
                "certifiedId" => documentation.CertifiedIdPath,
                "proofOfAddress" => documentation.ProofOfAddressPath,
                "driversLicense" => documentation.DriversLicensePath,
                "bankStatement" => documentation.BankStatementPath,
                "taxClearance" => documentation.TaxClearancePath,
                _ => null
            };

            if (string.IsNullOrEmpty(filePath))
            {
                return NotFound();
            }

            ViewBag.DocumentPath = filePath;
            ViewBag.DocumentType = documentType;
            return View();
        }

        private async Task SendApprovalEmail(PreEmploymentDocumentation documentation)
        {
            var application = documentation.EmploymentOffer.Application;

            var emailBody = $@"
                <div style='background-color: #28a745; color: white; padding: 20px; text-align: center; border-radius: 8px 8px 0 0;'>
                    <h1 style='margin: 0; font-size: 24px;'>✅ Documentation Approved!</h1>
                </div>
                <div style='background-color: white; padding: 30px; border: 1px solid #ddd; border-top: none; border-radius: 0 0 8px 8px;'>
                    <p style='font-size: 18px; font-weight: bold; margin-bottom: 20px;'>Dear {application.FirstName} {application.LastName},</p>
                    
                    <p style='font-size: 16px; line-height: 1.6; margin-bottom: 20px;'>
                        Great news! Your pre-employment documentation has been approved.
                    </p>
                    
                    <p style='font-size: 16px; line-height: 1.6; margin-bottom: 20px;'>
                        We are excited to welcome you to the AutoEdge team.
                    </p>

                    <p style='font-size: 16px; line-height: 1.6; margin-bottom: 20px;'>
                        Your onboarding session will be scheduled soon. You will receive 
                        a separate email with the date, time, and location details.
                    </p>

                    <div style='background-color: #e3f2fd; padding: 20px; border-radius: 8px; margin: 20px 0;'>
                        <h3 style='color: #1976d2; margin-top: 0;'>START DATE: {documentation.EmploymentOffer.StartDate:dd MMMM yyyy}</h3>
                        
                        <p style='color: #1976d2; margin-bottom: 10px;'>Please prepare to bring:</p>
                        <ul style='color: #1976d2;'>
                            <li style='margin: 5px 0;'>Original ID document</li>
                            <li style='margin: 5px 0;'>Original qualification certificates</li>
                            <li style='margin: 5px 0;'>Driver's license (if applicable)</li>
                        </ul>
                    </div>

                    <p style='font-size: 16px; line-height: 1.6; margin: 20px 0;'>
                        See you soon!
                    </p>

                    <p style='font-size: 16px; line-height: 1.6; margin: 20px 0;'>
                        Best regards,<br>
                        AutoEdge HR Team
                    </p>
                </div>";

            await _emailSender.SendEmailAsync(
                application.Email,
                "Documentation Approved - Next Steps for Onboarding",
                emailBody);
        }

        private async Task SendCorrectionRequestEmail(PreEmploymentDocumentation documentation)
        {
            var application = documentation.EmploymentOffer.Application;
            var accessUrl = Url.Action("Documentation", "EmployeeOnboarding", 
                new { token = documentation.EmploymentOffer.AccessToken }, Request.Scheme);

            var emailBody = $@"
                <div style='background-color: #ffc107; color: #212529; padding: 20px; text-align: center; border-radius: 8px 8px 0 0;'>
                    <h1 style='margin: 0; font-size: 24px;'>📝 Documentation Review Required</h1>
                </div>
                <div style='background-color: white; padding: 30px; border: 1px solid #ddd; border-top: none; border-radius: 0 0 8px 8px;'>
                    <p style='font-size: 18px; font-weight: bold; margin-bottom: 20px;'>Dear {application.FirstName} {application.LastName},</p>
                    
                    <p style='font-size: 16px; line-height: 1.6; margin-bottom: 20px;'>
                        Thank you for submitting your pre-employment documentation. After review, we need some corrections before we can proceed.
                    </p>

                    <div style='background-color: #fff3cd; padding: 20px; border-radius: 8px; margin: 20px 0; border-left: 4px solid #ffc107;'>
                        <h3 style='color: #856404; margin-top: 0;'>Corrections Needed:</h3>
                        <p style='color: #856404; margin-bottom: 0;'>{documentation.CorrectionRequests}</p>
                    </div>

                    <div style='text-align: center; margin: 30px 0;'>
                        <a href='{accessUrl}' 
                           style='background-color: #007bff; color: white; padding: 15px 30px; text-decoration: none; border-radius: 5px; font-size: 16px; font-weight: bold; display: inline-block;'>
                            Make Corrections
                        </a>
                    </div>

                    <p style='font-size: 16px; line-height: 1.6; margin: 20px 0;'>
                        Please make the necessary corrections and resubmit your documentation.
                    </p>

                    <p style='font-size: 16px; line-height: 1.6; margin: 20px 0;'>
                        Best regards,<br>
                        AutoEdge HR Team
                    </p>
                </div>";

            await _emailSender.SendEmailAsync(
                application.Email,
                "Documentation Review - Corrections Required",
                emailBody);
        }

        private async Task SendRejectionEmail(PreEmploymentDocumentation documentation)
        {
            var application = documentation.EmploymentOffer.Application;

            var emailBody = $@"
                <div style='background-color: #dc3545; color: white; padding: 20px; text-align: center; border-radius: 8px 8px 0 0;'>
                    <h1 style='margin: 0; font-size: 24px;'>❌ Documentation Rejected</h1>
                </div>
                <div style='background-color: white; padding: 30px; border: 1px solid #ddd; border-top: none; border-radius: 0 0 8px 8px;'>
                    <p style='font-size: 18px; font-weight: bold; margin-bottom: 20px;'>Dear {application.FirstName} {application.LastName},</p>
                    
                    <p style='font-size: 16px; line-height: 1.6; margin-bottom: 20px;'>
                        After careful review of your pre-employment documentation, we regret to inform you that we cannot proceed with your employment at this time.
                    </p>

                    <div style='background-color: #f8d7da; padding: 20px; border-radius: 8px; margin: 20px 0; border-left: 4px solid #dc3545;'>
                        <h3 style='color: #721c24; margin-top: 0;'>Reason:</h3>
                        <p style='color: #721c24; margin-bottom: 0;'>{documentation.AdminNotes}</p>
                    </div>

                    <p style='font-size: 16px; line-height: 1.6; margin: 20px 0;'>
                        We appreciate your interest in joining AutoEdge and wish you the best in your future endeavors.
                    </p>

                    <p style='font-size: 16px; line-height: 1.6; margin: 20px 0;'>
                        Best regards,<br>
                        AutoEdge HR Team
                    </p>
                </div>";

            await _emailSender.SendEmailAsync(
                application.Email,
                "Employment Application Update - AutoEdge",
                emailBody);
        }

        private string GetContentType(string fileName)
        {
            var extension = Path.GetExtension(fileName).ToLowerInvariant();
            return extension switch
            {
                ".pdf" => "application/pdf",
                ".jpg" or ".jpeg" => "image/jpeg",
                ".png" => "image/png",
                _ => "application/octet-stream"
            };
        }
    }
}
