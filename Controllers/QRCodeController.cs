using AutoEdge.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AutoEdge.Controllers
{
    [AllowAnonymous] // Allow access for QR code scanning without authentication
    public class QRCodeController : Controller
    {
        private readonly IQRCodeService _qrCodeService;
        private readonly ILogger<QRCodeController> _logger;

        public QRCodeController(IQRCodeService qrCodeService, ILogger<QRCodeController> logger)
        {
            _qrCodeService = qrCodeService;
            _logger = logger;
        }

        [HttpGet]
        public IActionResult Scan()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Scan(string qrCodeData)
        {
            if (string.IsNullOrEmpty(qrCodeData))
            {
                return Json(new { success = false, message = "QR code data is required" });
            }

            try
            {
                // Verify the QR code and get booking information
                var booking = await _qrCodeService.VerifyServiceBookingQRCodeAsync(qrCodeData);
                
                if (booking == null)
                {
                    return Json(new { 
                        success = false, 
                        message = "Invalid or expired QR code. Please contact customer service." 
                    });
                }

                if (booking.IsCheckedIn)
                {
                    return Json(new { 
                        success = false, 
                        message = "This booking has already been checked in.", 
                        alreadyCheckedIn = true 
                    });
                }

                // Return booking information for confirmation
                return Json(new {
                    success = true,
                    booking = new {
                        id = booking.Id,
                        reference = booking.Reference,
                        customerName = booking.CustomerName,
                        customerEmail = booking.CustomerEmail,
                        vehicle = $"{booking.Make} {booking.Model} ({booking.Year})",
                        serviceType = booking.ServiceType.ToString(),
                        scheduledDate = booking.PreferredDate.ToString("yyyy-MM-dd"),
                        scheduledTime = booking.PreferredStart.ToString("HH:mm"),
                        deliveryMethod = booking.DeliveryMethod.ToString(),
                        estimatedDuration = booking.EstimatedDurationMin,
                        estimatedCost = booking.EstimatedCost,
                        notes = booking.Notes
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing QR code scan");
                return Json(new { success = false, message = "Error processing QR code. Please try again." });
            }
        }

        [HttpPost]
        public async Task<IActionResult> CheckIn(string qrCodeData)
        {
            if (string.IsNullOrEmpty(qrCodeData))
            {
                return Json(new { success = false, message = "QR code data is required" });
            }

            try
            {
                var success = await _qrCodeService.CheckInServiceBookingWithQRCodeAsync(qrCodeData);
                
                if (success)
                {
                    // Get booking details for confirmation
                    var booking = await _qrCodeService.VerifyServiceBookingQRCodeAsync(qrCodeData);
                    return Json(new {
                        success = true,
                        message = "Booking checked in successfully!",
                        booking = booking != null ? new {
                            reference = booking.Reference,
                            customerName = booking.CustomerName,
                            vehicle = $"{booking.Make} {booking.Model} ({booking.Year})",
                            serviceType = booking.ServiceType.ToString(),
                            checkInTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
                        } : null
                    });
                }
                else
                {
                    return Json(new { 
                        success = false, 
                        message = "Failed to check in. Please verify the QR code is valid and not already used." 
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking in with QR code");
                return Json(new { success = false, message = "Error processing check-in. Please try again." });
            }
        }

        [HttpGet]
        public IActionResult Success()
        {
            return View();
        }
    }
}
