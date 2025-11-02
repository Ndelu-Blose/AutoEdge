using AutoEdge.Data;
using AutoEdge.Models.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;

namespace AutoEdge.Controllers
{
    [Authorize(Roles = "Administrator")]
    public class EmploymentOfferController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IEmailSender _emailSender;
        private readonly UserManager<ApplicationUser> _userManager;

        public EmploymentOfferController(
            ApplicationDbContext context,
            IEmailSender emailSender,
            UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _emailSender = emailSender;
            _userManager = userManager;
        }

        // GET: EmploymentOffer
        public async Task<IActionResult> Index()
        {
            var offers = await _context.EmploymentOffers
                .Include(e => e.Application)
                .Where(e => e.IsActive)
                .OrderByDescending(e => e.OfferSentDate)
                .ToListAsync();

            return View(offers);
        }

        // GET: EmploymentOffer/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var employmentOffer = await _context.EmploymentOffers
                .Include(e => e.Application)
                .Include(e => e.PreEmploymentDocumentation)
                .FirstOrDefaultAsync(m => m.OfferId == id);

            if (employmentOffer == null)
            {
                return NotFound();
            }

            return View(employmentOffer);
        }

        // GET: EmploymentOffer/Create
        public async Task<IActionResult> Create(int? applicationId)
        {
            if (applicationId == null)
            {
                // If no applicationId provided, show list of applications to choose from
                var applications = await _context.Applications
                    .Include(a => a.JobPosting)
                    .Where(a => a.Status == "Hire" && a.IsActive)
                    .OrderByDescending(a => a.SubmittedDate)
                    .ToListAsync();

                ViewBag.Applications = applications;
                return View("SelectApplication");
            }

            var application = await _context.Applications
                .Include(a => a.JobPosting)
                .FirstOrDefaultAsync(a => a.ApplicationId == applicationId);

            if (application == null)
            {
                return NotFound();
            }

            var model = new EmploymentOffer
            {
                ApplicationId = application.ApplicationId,
                JobTitle = application.JobPosting.JobTitle,
                Department = application.JobPosting.Department,
                EmploymentType = "Full-time",
                WorkLocation = "AutoEdge, Durban",
                StartDate = DateTime.Now.AddDays(14), // Default to 2 weeks from now
                OfferExpiryDate = DateTime.Now.AddDays(7), // 7 days to accept
                AccessToken = GenerateAccessToken()
            };

            return View(model);
        }

        // POST: EmploymentOffer/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(EmploymentOffer employmentOffer)
        {
            if (ModelState.IsValid)
            {
                employmentOffer.OfferSentDate = DateTime.UtcNow;
                employmentOffer.Status = "Pending";
                employmentOffer.IsActive = true;

                _context.Add(employmentOffer);
                await _context.SaveChangesAsync();

                // Send email to the applicant
                await SendEmploymentOfferEmail(employmentOffer);

                TempData["SuccessMessage"] = "Employment offer sent successfully!";
                return RedirectToAction(nameof(Index));
            }

            return View(employmentOffer);
        }

        // GET: EmploymentOffer/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var employmentOffer = await _context.EmploymentOffers
                .Include(e => e.Application)
                .FirstOrDefaultAsync(m => m.OfferId == id);

            if (employmentOffer == null)
            {
                return NotFound();
            }

            return View(employmentOffer);
        }

        // POST: EmploymentOffer/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, EmploymentOffer employmentOffer)
        {
            if (id != employmentOffer.OfferId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    employmentOffer.UpdatedDate = DateTime.UtcNow;
                    _context.Update(employmentOffer);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!EmploymentOfferExists(employmentOffer.OfferId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }

            return View(employmentOffer);
        }

        // GET: EmploymentOffer/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var employmentOffer = await _context.EmploymentOffers
                .Include(e => e.Application)
                .FirstOrDefaultAsync(m => m.OfferId == id);

            if (employmentOffer == null)
            {
                return NotFound();
            }

            return View(employmentOffer);
        }

        // POST: EmploymentOffer/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var employmentOffer = await _context.EmploymentOffers.FindAsync(id);
            if (employmentOffer != null)
            {
                employmentOffer.IsActive = false;
                employmentOffer.UpdatedDate = DateTime.UtcNow;
                _context.Update(employmentOffer);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // GET: EmploymentOffer/ResendEmail/5
        public async Task<IActionResult> ResendEmail(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var employmentOffer = await _context.EmploymentOffers
                .Include(e => e.Application)
                .FirstOrDefaultAsync(m => m.OfferId == id);

            if (employmentOffer == null)
            {
                return NotFound();
            }

            await SendEmploymentOfferEmail(employmentOffer);
            TempData["SuccessMessage"] = "Employment offer email resent successfully!";
            return RedirectToAction(nameof(Details), new { id = employmentOffer.OfferId });
        }

        private bool EmploymentOfferExists(int id)
        {
            return _context.EmploymentOffers.Any(e => e.OfferId == id);
        }

        private string GenerateAccessToken()
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            var random = new Random();
            return new string(Enumerable.Repeat(chars, 40)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        private async Task SendEmploymentOfferEmail(EmploymentOffer offer)
        {
            var application = await _context.Applications
                .Include(a => a.JobPosting)
                .FirstOrDefaultAsync(a => a.ApplicationId == offer.ApplicationId);

            if (application == null) return;

            var accessUrl = Url.Action("ReviewOffer", "EmployeeOnboarding", 
                new { token = offer.AccessToken }, Request.Scheme);

            var emailBody = $@"
                <div style='background-color: #28a745; color: white; padding: 20px; text-align: center; border-radius: 8px 8px 0 0;'>
                    <h1 style='margin: 0; font-size: 24px;'>🎉 Congratulations!</h1>
                </div>
                <div style='background-color: white; padding: 30px; border: 1px solid #ddd; border-top: none; border-radius: 0 0 8px 8px;'>
                    <p style='font-size: 18px; font-weight: bold; margin-bottom: 20px;'>Dear {application.FirstName} {application.LastName},</p>
                    
                    <p style='font-size: 16px; line-height: 1.6; margin-bottom: 20px;'>
                        Congratulations! We are pleased to offer you the position of <strong>{offer.JobTitle}</strong> at AutoEdge.
                    </p>
                    
                    <p style='font-size: 16px; line-height: 1.6; margin-bottom: 20px;'>
                        Your performance throughout the recruitment process has been outstanding.
                    </p>

                    <div style='background-color: #f8f9fa; padding: 20px; border-radius: 8px; margin: 20px 0;'>
                        <h3 style='color: #495057; margin-top: 0;'>OFFER DETAILS:</h3>
                        <ul style='list-style: none; padding: 0;'>
                            <li style='margin: 8px 0;'><strong>Job Title:</strong> {offer.JobTitle}</li>
                            <li style='margin: 8px 0;'><strong>Department:</strong> {offer.Department}</li>
                            <li style='margin: 8px 0;'><strong>Salary Offered:</strong> R{offer.SalaryOffered:N0} per month</li>
                            <li style='margin: 8px 0;'><strong>Start Date:</strong> {offer.StartDate:dd MMMM yyyy}</li>
                            <li style='margin: 8px 0;'><strong>Employment Type:</strong> {offer.EmploymentType}</li>
                            <li style='margin: 8px 0;'><strong>Work Location:</strong> {offer.WorkLocation}</li>
                        </ul>
                    </div>

                    <div style='background-color: #e3f2fd; padding: 20px; border-radius: 8px; margin: 20px 0;'>
                        <h3 style='color: #1976d2; margin-top: 0;'>NEXT STEPS:</h3>
                        <ol style='color: #1976d2;'>
                            <li style='margin: 8px 0;'>Review attached employment contract</li>
                            <li style='margin: 8px 0;'>Complete pre-employment documentation</li>
                            <li style='margin: 8px 0;'>Schedule onboarding session</li>
                        </ol>
                    </div>

                    <p style='font-size: 16px; line-height: 1.6; margin: 20px 0;'>
                        Please confirm your acceptance by <strong>{offer.OfferExpiryDate:dddd, dd MMMM yyyy}</strong>.
                    </p>

                    <div style='text-align: center; margin: 30px 0;'>
                        <a href='{accessUrl}' 
                           style='background-color: #007bff; color: white; padding: 15px 30px; text-decoration: none; border-radius: 5px; font-size: 16px; font-weight: bold; display: inline-block;'>
                            Review & Accept Offer
                        </a>
                    </div>

                    <p style='font-size: 16px; line-height: 1.6; margin: 20px 0;'>
                        Welcome to the AutoEdge team!
                    </p>

                    <p style='font-size: 16px; line-height: 1.6; margin: 20px 0;'>
                        Best regards,<br>
                        AutoEdge HR Team
                    </p>
                </div>";

            await _emailSender.SendEmailAsync(
                application.Email,
                "Employment Offer - AutoEdge",
                emailBody);
        }
    }
}
