using AutoEdge.Data;
using AutoEdge.Models.Entities;
using AutoEdge.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AutoEdge.Controllers
{
    [Authorize]
    public class CustomerServicePortalController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly IServiceNotificationService _notificationService;
        private readonly ILogger<CustomerServicePortalController> _logger;

        public CustomerServicePortalController(ApplicationDbContext db, IServiceNotificationService notificationService, 
            ILogger<CustomerServicePortalController> logger)
        {
            _db = db;
            _notificationService = notificationService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> MyServices()
        {
            try
            {
                var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized();
                }

                var services = await _db.ServiceBookings
                    .Include(b => b.ServiceJob)
                    .ThenInclude(j => j.Mechanic)
                    .Include(b => b.ServiceInvoices)
                    .ThenInclude(i => i.ServicePayments)
                    .Where(b => b.CustomerId == userId)
                    .OrderByDescending(b => b.CreatedAt)
                    .ToListAsync();

                return View(services);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading customer services for user {UserId}", User.Identity?.Name);
                return View("Error");
            }
        }

        [HttpGet]
        public async Task<IActionResult> ServiceDetails(int id)
        {
            try
            {
                var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized();
                }

                var service = await _db.ServiceBookings
                    .Include(b => b.ServiceJob)
                    .ThenInclude(j => j.Mechanic)
                    .Include(b => b.ServiceInvoices)
                    .ThenInclude(i => i.ServicePayments)
                    .Include(b => b.ServiceExecutions)
                    .FirstOrDefaultAsync(b => b.Id == id && b.CustomerId == userId);

                if (service == null)
                {
                    return NotFound();
                }

                return View(service);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading service details for booking {BookingId}", id);
                return View("Error");
            }
        }

        [HttpGet]
        public async Task<IActionResult> PayInvoice(int id)
        {
            try
            {
                var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized();
                }

                var invoice = await _db.ServiceInvoices
                    .Include(i => i.ServiceBooking)
                    .Include(i => i.ServicePayments)
                    .FirstOrDefaultAsync(i => i.Id == id && i.ServiceBooking.CustomerId == userId);

                if (invoice == null)
                {
                    return NotFound();
                }

                // Check if already paid
                var totalPaid = invoice.ServicePayments
                    .Where(p => p.Status == "Completed")
                    .Sum(p => p.Amount);

                if (totalPaid >= invoice.TotalAmount)
                {
                    TempData["InfoMessage"] = "This invoice has already been paid in full.";
                    return RedirectToAction("ServiceDetails", new { id = invoice.ServiceBookingId });
                }

                ViewBag.RemainingAmount = invoice.TotalAmount - totalPaid;
                return View(invoice);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading payment page for invoice {InvoiceId}", id);
                return View("Error");
            }
        }

        [HttpGet]
        public async Task<IActionResult> PaymentHistory()
        {
            try
            {
                var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized();
                }

                var payments = await _db.ServicePayments
                    .Include(p => p.ServiceInvoice)
                    .ThenInclude(i => i.ServiceBooking)
                    .Where(p => p.ServiceInvoice.ServiceBooking.CustomerId == userId)
                    .OrderByDescending(p => p.PaymentDate)
                    .ToListAsync();

                return View(payments);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading payment history for user {UserId}", User.Identity?.Name);
                return View("Error");
            }
        }

        [HttpGet]
        public async Task<IActionResult> DownloadReceipt(int id)
        {
            try
            {
                var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized();
                }

                var payment = await _db.ServicePayments
                    .Include(p => p.ServiceInvoice)
                    .ThenInclude(i => i.ServiceBooking)
                    .FirstOrDefaultAsync(p => p.Id == id && p.ServiceInvoice.ServiceBooking.CustomerId == userId);

                if (payment == null)
                {
                    return NotFound();
                }

                // Generate receipt PDF
                var pdfBytes = await GenerateReceiptPDFAsync(payment);
                
                return File(pdfBytes, "application/pdf", $"Receipt-{payment.TransactionId}.pdf");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating receipt PDF for payment {PaymentId}", id);
                return View("Error");
            }
        }

        [HttpPost]
        public async Task<IActionResult> RateService(int bookingId, int rating, string? comments = null)
        {
            try
            {
                var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized();
                }

                var booking = await _db.ServiceBookings
                    .FirstOrDefaultAsync(b => b.Id == bookingId && b.CustomerId == userId);

                if (booking == null)
                {
                    return NotFound();
                }

                if (rating < 1 || rating > 5)
                {
                    return Json(new { success = false, message = "Rating must be between 1 and 5" });
                }

                // Store rating and comments
                booking.CustomerRating = rating;
                booking.CustomerComments = comments;
                booking.RatedAt = DateTime.UtcNow;

                await _db.SaveChangesAsync();

                _logger.LogInformation("Service rating submitted for booking {BookingId}: {Rating} stars", bookingId, rating);
                return Json(new { success = true, message = "Thank you for your feedback!" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error submitting rating for booking {BookingId}", bookingId);
                return Json(new { success = false, message = "An error occurred while submitting your rating" });
            }
        }

        [HttpGet]
        public async Task<IActionResult> ServiceStatus(int id)
        {
            try
            {
                var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized();
                }

                var booking = await _db.ServiceBookings
                    .Include(b => b.ServiceJob)
                    .ThenInclude(j => j.Mechanic)
                    .Include(b => b.ServiceExecutions)
                    .FirstOrDefaultAsync(b => b.Id == id && b.CustomerId == userId);

                if (booking == null)
                {
                    return NotFound();
                }

                return Json(new
                {
                    status = booking.Status.ToString(),
                    progress = GetServiceProgress(booking),
                    estimatedCompletion = booking.ServiceJob != null ? 
                        booking.ServiceJob.ScheduledDate.ToDateTime(booking.ServiceJob.ScheduledStart).AddMinutes(booking.ServiceJob.DurationMin).ToString("yyyy-MM-dd HH:mm") : null,
                    mechanic = booking.ServiceJob?.Mechanic?.Name,
                    lastUpdate = booking.UpdatedAt
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting service status for booking {BookingId}", id);
                return Json(new { error = "An error occurred while retrieving service status" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> RequestPickup(int bookingId, string pickupLocation, DateTime pickupDate, TimeSpan pickupTime)
        {
            try
            {
                var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized();
                }

                var booking = await _db.ServiceBookings
                    .FirstOrDefaultAsync(b => b.Id == bookingId && b.CustomerId == userId);

                if (booking == null)
                {
                    return NotFound();
                }

                // Create pickup request
                var pickupRequest = new CreatePickupRequest
                {
                    ServiceBookingId = bookingId,
                    CustomerId = userId,
                    CustomerName = booking.CustomerName,
                    CustomerPhone = booking.CustomerPhone ?? "",
                    PickupLocation = pickupLocation,
                    PickupDate = pickupDate,
                    PickupTime = pickupTime,
                    VehicleMake = booking.Make,
                    VehicleModel = booking.Model,
                    VehicleYear = booking.Year,
                    VehicleLicensePlate = booking.VIN
                };

                // Use pickup service to create request
                var pickupService = HttpContext.RequestServices.GetRequiredService<IPickupDropoffService>();
                var pickup = await pickupService.CreatePickupRequestAsync(pickupRequest);

                return Json(new { success = true, message = "Pickup request submitted successfully!", pickupId = pickup.VehiclePickupId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating pickup request for booking {BookingId}", bookingId);
                return Json(new { success = false, message = "An error occurred while creating the pickup request" });
            }
        }

        private string GetServiceProgress(ServiceBooking booking)
        {
            return booking.Status switch
            {
                ServiceBookingStatus.Pending => "Booking received, awaiting confirmation",
                ServiceBookingStatus.Confirmed => "Service confirmed, awaiting scheduling",
                ServiceBookingStatus.Waitlisted => "On waitlist, will be contacted when slot available",
                ServiceBookingStatus.Canceled => "Service canceled",
                ServiceBookingStatus.Completed => "Service completed, ready for pickup",
                _ => "Status unknown"
            };
        }

        private async Task<byte[]> GenerateReceiptPDFAsync(ServicePayment payment)
        {
            // Implement PDF generation for receipt
            // This is a placeholder - implement actual PDF generation
            var html = $@"
                <html>
                <head><title>Receipt {payment.TransactionId}</title></head>
                <body>
                    <h1>Payment Receipt</h1>
                    <p>Transaction ID: {payment.TransactionId}</p>
                    <p>Amount: ${payment.Amount:F2}</p>
                    <p>Payment Method: {payment.PaymentMethod}</p>
                    <p>Payment Date: {payment.PaymentDate:yyyy-MM-dd HH:mm}</p>
                </body>
                </html>
            ";

            return System.Text.Encoding.UTF8.GetBytes(html);
        }
    }
}
