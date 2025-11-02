using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using AutoEdge.Models;
using Microsoft.AspNetCore.Identity.UI.Services;
using AutoEdge.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using AutoEdge.Models.Entities;

namespace AutoEdge.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly IEmailSender _emailSender;
    private readonly IEmailService _emailService;
    private readonly IPaymentService _paymentService;
    private readonly IPdfService _pdfService;
    private readonly UserManager<ApplicationUser> _userManager;

    public HomeController(ILogger<HomeController> logger, IEmailSender emailSender, IEmailService emailService, IPaymentService paymentService, IPdfService pdfService, UserManager<ApplicationUser> userManager)
    {
        _logger = logger;
        _emailSender = emailSender;
        _emailService = emailService;
        _paymentService = paymentService;
        _pdfService = pdfService;
        _userManager = userManager;
    }

    public async Task<IActionResult> Index()
    {

        // Check if we have a success popup to show first
        if (TempData["ShowSuccessPopup"] != null && (bool)TempData["ShowSuccessPopup"])
        {
            // Keep the popup data and show the home page
            // TempData.Keep("ShowSuccessPopup");
            // TempData.Keep("SuccessMessage");
            return View();
        }
        // If user is authenticated, redirect to appropriate dashboard
        if (User.Identity.IsAuthenticated)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user != null)
            {
                var roles = await _userManager.GetRolesAsync(user);
                
                // Redirect based on primary role
                if (roles.Contains("Administrator"))
                {
                    return RedirectToAction("Index", "Admin");
                }
                else if (roles.Contains("Recruiter"))
                {
                    return RedirectToAction("Dashboard", "RecruitmentRecruiter");
                }
                else if (roles.Contains("Customer"))
                {
                    return RedirectToAction("Dashboard", "Customer");
                }
                else if (roles.Contains("Driver"))
                {
                    return RedirectToAction("Dashboard", "Driver");
                }
                else if (roles.Contains("Applicant"))
                {
                    return RedirectToAction("ApplicationStatus", "RecruitmentApplicant", new { email = user.Email });
                }
            }
        }
        
        return View();
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }

    [HttpGet]
    public async Task<IActionResult> TestEmail(string email = "test@example.com")
    {
        try
        {
            await _emailSender.SendEmailAsync(email, "Test Email - AutoEdge", "<h1>Test Email</h1><p>This is a test email to verify email functionality.</p>");
            return Json(new { success = true, message = $"Test email sent successfully to {email}" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send test email to {Email}", email);
            return Json(new { success = false, message = $"Failed to send test email: {ex.Message}" });
        }
    }

    [HttpGet]
    public async Task<IActionResult> TestPaymentEmail(string email = "test@example.com")
    {
        try
        {
            await _emailService.SendPaymentConfirmationEmailAsync(
                email, 
                "Test User", 
                "2024 Toyota Camry", 
                25000.00m, 
                "Credit Card", 
                "test_transaction_123"
            );
            return Json(new { success = true, message = $"Payment confirmation email sent successfully to {email}" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send payment confirmation email to {Email}", email);
            return Json(new { success = false, message = $"Failed to send payment confirmation email: {ex.Message}" });
        }
    }

    [HttpGet]
    public async Task<IActionResult> TestDeliveryQREmail(string email = "test@example.com")
    {
        try
        {
            // Generate a test QR code image
            var testQRData = "TEST_QR_CODE_DATA_12345";
            var qrCodeImageBytes = await GenerateTestQRCodeImage(testQRData);

            // Generate PDF with QR code
            var pdfBytes = await _pdfService.GenerateQRCodePdfAsync(
                qrCodeImageBytes,
                "Test Customer",
                "2024 Toyota Camry",
                "TRK123456789",
                DateTime.Now.AddDays(1)
            );

            await _emailService.SendDeliveryQRCodeEmailWithPdfAsync(
                email,
                "Test Customer",
                "2024 Toyota Camry",
                "TRK123456789",
                DateTime.Now.AddDays(1),
                pdfBytes
            );

            return Json(new { success = true, message = $"Delivery QR code email with PDF attachment sent successfully to {email}" });
        }
        catch (Exception ex)
        {
            return Json(new { success = false, message = $"Failed to send delivery QR email: {ex.Message}" });
        }
    }

    [HttpGet]
    public async Task<IActionResult> TestPaymentProofEmail(int paymentId)
    {
        try
        {
            var result = await _paymentService.SendPaymentProofEmailAsync(paymentId);
            return Json(new { success = result, message = result ? $"Payment proof email sent successfully for payment {paymentId}" : $"Failed to send payment proof email for payment {paymentId}" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send payment proof email for payment {PaymentId}", paymentId);
            return Json(new { success = false, message = $"Failed to send payment proof email: {ex.Message}" });
        }
    }

    private async Task<byte[]> GenerateTestQRCodeImage(string data)
    {
        try
        {
            // Simple test QR code generation
            var testImage = new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A }; // PNG header
            return testImage;
        }
        catch
        {
            // Return a minimal byte array if QR generation fails
            return new byte[0];
        }
    }
}
