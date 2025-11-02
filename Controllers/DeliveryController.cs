using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using AutoEdge.Data;
using AutoEdge.Models.Entities;
using AutoEdge.Repositories;
using AutoEdge.Services;
using System.Security.Claims;

namespace AutoEdge.Controllers
{
    [Authorize]
    public class DeliveryController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ApplicationDbContext _context;
        private readonly IQRCodeService _qrCodeService;
        private readonly IEmailService _emailService;
        private readonly IPdfService _pdfService;

        public DeliveryController(IUnitOfWork unitOfWork, ApplicationDbContext context, IQRCodeService qrCodeService, IEmailService emailService, IPdfService pdfService)
        {
            _unitOfWork = unitOfWork;
            _context = context;
            _qrCodeService = qrCodeService;
            _emailService = emailService;
            _pdfService = pdfService;
        }

        // GET: Delivery/Schedule/{contractId}
        [HttpGet]
        public async Task<IActionResult> Schedule(int contractId)
        {
            var contract = await _context.Contracts
                .Include(c => c.Purchase)
                .ThenInclude(p => p.Vehicle)
                .Include(c => c.Customer)
                .ThenInclude(c => c.User)
                .FirstOrDefaultAsync(c => c.Id == contractId);

            if (contract == null)
            {
                return NotFound("Contract not found.");
            }

            // Check if user is authorized to schedule delivery for this contract
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (contract.Customer.UserId != userId && !User.IsInRole("Administrator"))
            {
                return Forbid();
            }

            // Check if delivery already exists
            var existingDelivery = await _context.Deliveries
                .FirstOrDefaultAsync(d => d.ContractId == contractId);

            if (existingDelivery != null)
            {
                return RedirectToAction("Track", new { id = existingDelivery.Id });
            }

            ViewBag.Contract = contract;
            return View();
        }

        // POST: Delivery/Schedule
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Schedule(int contractId, string deliveryType, DateTime scheduledDate, 
            string deliveryAddress, string city, string state, string zipCode, string country,
            string contactPersonName, string contactPersonPhone, string contactPersonEmail,
            string deliveryInstructions)
        {
            var contract = await _context.Contracts
                .Include(c => c.Customer)
                .FirstOrDefaultAsync(c => c.Id == contractId);

            if (contract == null)
            {
                return NotFound("Contract not found.");
            }

            // Validate authorization
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (contract.Customer.UserId != userId && !User.IsInRole("Administrator"))
            {
                return Forbid();
            }

            // Create delivery record
            var delivery = new Delivery
            {
                ContractId = contractId,
                DeliveryType = deliveryType,
                ScheduledDate = scheduledDate,
                DeliveryAddress = deliveryAddress,
                City = city,
                State = state,
                ZipCode = zipCode,
                Country = country,
                ContactPersonName = contactPersonName,
                ContactPersonPhone = contactPersonPhone,
                ContactPersonEmail = contactPersonEmail,
                DeliveryInstructions = deliveryInstructions,
                Status = "Scheduled",
                TrackingNumber = GenerateTrackingNumber(),
                CreatedDate = DateTime.UtcNow,
                ModifiedDate = DateTime.UtcNow,
                IsActive = true
            };

            // Set delivery fee based on type
            delivery.DeliveryFee = deliveryType switch
            {
                "DealerPickup" => 0,
                "HomeDelivery" => 150,
                "ThirdPartyDelivery" => 200,
                _ => 0
            };

            _context.Deliveries.Add(delivery);
            await _context.SaveChangesAsync();

            // Generate QR code for the delivery
            try
            {
                var qrCode = await _qrCodeService.GenerateQRCodeForDeliveryAsync(delivery.Id);
                delivery.QRCode = qrCode;
                delivery.QRCodeExpiry = DateTime.UtcNow.AddDays(30); // QR code expires in 30 days
                
                _context.Deliveries.Update(delivery);
                await _context.SaveChangesAsync();

                // Send QR code email to the buyer
                try
                {
                    // Get contract and vehicle information for the email
                    var contractWithDetails = await _context.Contracts
                        .Include(c => c.Purchase)
                        .ThenInclude(p => p.Vehicle)
                        .Include(c => c.Customer)
                        .ThenInclude(c => c.User)
                        .FirstOrDefaultAsync(c => c.Id == contractId);

                    if (contractWithDetails != null)
                    {
                        var vehicle = contractWithDetails.Purchase?.Vehicle;
                        var customer = contractWithDetails.Customer;
                        var vehicleInfo = vehicle != null ? $"{vehicle.Year} {vehicle.Make} {vehicle.Model}" : "Vehicle";
                        var customerName = $"{customer.User.FirstName} {customer.User.LastName}";
                        
                        // Generate QR code image
                        var qrCodeImageBytes = await _qrCodeService.GenerateQRCodeImageAsync(delivery.QRCode);
                        
                        // Generate PDF with QR code
                        var pdfBytes = await _pdfService.GenerateQRCodePdfAsync(
                            qrCodeImageBytes,
                            customerName,
                            vehicleInfo,
                            delivery.TrackingNumber,
                            delivery.ScheduledDate
                        );
                        
                        await _emailService.SendDeliveryQRCodeEmailWithPdfAsync(
                            contactPersonEmail,
                            customerName,
                            vehicleInfo,
                            delivery.TrackingNumber,
                            delivery.ScheduledDate,
                            pdfBytes
                        );
                    }
                }
                catch (Exception emailEx)
                {
                    // Log the error but don't fail the delivery creation
                    Console.WriteLine($"Error sending QR code email for delivery {delivery.Id}: {emailEx.Message}");
                }
            }
            catch (Exception ex)
            {
                // Log the error but don't fail the delivery creation
                // QR code can be regenerated later if needed
                Console.WriteLine($"Error generating QR code for delivery {delivery.Id}: {ex.Message}");
            }

            TempData["SuccessMessage"] = "Delivery has been scheduled successfully!";
            return RedirectToAction("Track", new { id = delivery.Id });
    }

    [HttpPost]
    public async Task<IActionResult> VerifyQRCode(string qrCode)
    {
        try
        {
            if (string.IsNullOrEmpty(qrCode))
            {
                return Json(new { success = false, message = "QR code is required." });
            }

            // Get current driver user ID
            var driverUserId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(driverUserId))
            {
                return Json(new { success = false, message = "Driver authentication required." });
            }

            // Verify the QR code and complete the delivery
            var result = await _qrCodeService.CompleteDeliveryWithQRCodeAsync(qrCode, driverUserId);
            
            if (result)
            {
                return Json(new { success = true, message = "Delivery completed successfully!" });
            }
            else
            {
                return Json(new { success = false, message = "Invalid or expired QR code." });
            }
        }
        catch (Exception ex)
        {
            return Json(new { success = false, message = "An error occurred while verifying the QR code." });
        }
    }

    [HttpGet]
    public async Task<IActionResult> ScanQRCode()
    {
        // Return the QR code scanning view for drivers
        return View();
    }

        // GET: Delivery/Track/{id}
        [HttpGet]
        public async Task<IActionResult> Track(int id)
        {
            var delivery = await _context.Deliveries
                .Include(d => d.Contract)
                .ThenInclude(c => c.Purchase)
                .ThenInclude(p => p.Vehicle)
                .Include(d => d.Contract.Customer)
                .ThenInclude(c => c.User)
                .Include(d => d.Driver)
                .FirstOrDefaultAsync(d => d.Id == id);

            if (delivery == null)
            {
                return NotFound("Delivery not found.");
            }

            // Check authorization
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (delivery.Contract.Customer.UserId != userId && 
                delivery.DriverUserId != userId && 
                !User.IsInRole("Administrator"))
            {
                return Forbid();
            }

            return View(delivery);
        }

        // GET: Delivery/MyDeliveries (for customers)
        [HttpGet]
        public async Task<IActionResult> MyDeliveries()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            
            var deliveries = await _context.Deliveries
                .Include(d => d.Contract)
                .ThenInclude(c => c.Purchase)
                .ThenInclude(p => p.Vehicle)
                .Include(d => d.Contract.Customer)
                .ThenInclude(c => c.User)
                .Include(d => d.Driver)
                .Where(d => d.Contract.Customer.UserId == userId)
                .OrderByDescending(d => d.CreatedDate)
                .ToListAsync();

            return View(deliveries);
        }

        // GET: Delivery/DriverAssignments (for drivers)
        [HttpGet]
        [Authorize(Roles = "Driver,Administrator")]
        public async Task<IActionResult> DriverAssignments()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            
            var deliveries = await _context.Deliveries
                .Include(d => d.Contract)
                .ThenInclude(c => c.Purchase)
                .ThenInclude(p => p.Vehicle)
                .Include(d => d.Contract.Customer)
                .Where(d => d.DriverUserId == userId || User.IsInRole("Administrator"))
                .OrderBy(d => d.ScheduledDate)
                .ToListAsync();

            return View(deliveries);
        }

        // POST: Delivery/UpdateStatus
        [HttpPost]
        [Authorize(Roles = "Driver,Administrator")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStatus(int id, string status, decimal? latitude, decimal? longitude)
        {
            var delivery = await _context.Deliveries.FindAsync(id);
            if (delivery == null)
            {
                return NotFound();
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (delivery.DriverUserId != userId && !User.IsInRole("Administrator"))
            {
                return Forbid();
            }

            delivery.Status = status;
            delivery.ModifiedDate = DateTime.UtcNow;

            // Update GPS coordinates if provided
            if (latitude.HasValue && longitude.HasValue)
            {
                delivery.CurrentLatitude = latitude;
                delivery.CurrentLongitude = longitude;
                delivery.LastLocationUpdate = DateTime.UtcNow;
            }

            // Set completion date if delivered
            if (status == "Delivered")
            {
                delivery.CompletedDate = DateTime.UtcNow;
                delivery.ActualArrival = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();

            return Json(new { success = true, message = "Status updated successfully" });
        }

        // GET: Delivery/AssignDriver/{id}
        [HttpGet]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> AssignDriver(int id)
        {
            var delivery = await _context.Deliveries
                .Include(d => d.Contract)
                .ThenInclude(c => c.Purchase)
                .ThenInclude(p => p.Vehicle)
                .Include(d => d.Contract.Customer)
                .ThenInclude(c => c.User)
                .FirstOrDefaultAsync(d => d.Id == id);

            if (delivery == null)
            {
                return NotFound("Delivery not found.");
            }

            // Get available drivers
            var drivers = await _context.Users
                .Where(u => _context.UserRoles
                    .Any(ur => ur.UserId == u.Id && 
                        _context.Roles.Any(r => r.Id == ur.RoleId && r.Name == "Driver")))
                .Select(u => new { u.Id, u.FirstName, u.LastName, u.PhoneNumber })
                .ToListAsync();

            ViewBag.Delivery = delivery;
            ViewBag.Drivers = drivers;
            return View();
        }

        // POST: Delivery/AssignDriver
        [HttpPost]
        [Authorize(Roles = "Administrator")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AssignDriver(int deliveryId, string driverUserId, string driverName, string driverPhone)
        {
            var delivery = await _context.Deliveries.FindAsync(deliveryId);
            if (delivery == null)
            {
                return NotFound();
            }

            delivery.DriverUserId = driverUserId;
            delivery.DriverName = driverName;
            delivery.DriverPhone = driverPhone;
            // delivery.Status = "Pending";
            delivery.ModifiedDate = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Driver assigned successfully!";
            return RedirectToAction("Track", new { id = deliveryId });
        }

        // POST: Delivery/Reschedule
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Reschedule(int id, DateTime newScheduledDate, string rescheduleReason)
        {
            var delivery = await _context.Deliveries
                .Include(d => d.Contract.Customer)
                .FirstOrDefaultAsync(d => d.Id == id);

            if (delivery == null)
            {
                return NotFound();
            }

            // Check authorization
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (delivery.Contract.Customer.UserId != userId && !User.IsInRole("Administrator"))
            {
                return Forbid();
            }

            delivery.ScheduledDate = newScheduledDate;
            delivery.RescheduleReason = rescheduleReason;
            delivery.RescheduleCount++;
            delivery.LastRescheduleDate = DateTime.UtcNow;
            delivery.ModifiedDate = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Delivery has been rescheduled successfully!";
            return RedirectToAction("Track", new { id = id });
        }

        // POST: Delivery/SubmitRating
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SubmitRating(int deliveryId, int customerRating, string customerFeedback)
        {
            var delivery = await _context.Deliveries
                .Include(d => d.Contract.Customer)
                .FirstOrDefaultAsync(d => d.Id == deliveryId);

            if (delivery == null)
            {
                return NotFound();
            }

            // Check authorization
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (delivery.Contract.Customer.UserId != userId)
            {
                return Forbid();
            }

            // Validate that delivery is completed and not already rated
            if (delivery.Status != "Delivered" || delivery.CustomerRating > 0)
            {
                TempData["ErrorMessage"] = "This delivery cannot be rated.";
                return RedirectToAction("MyDeliveries");
            }

            // Validate rating range
            if (customerRating < 1 || customerRating > 5)
            {
                TempData["ErrorMessage"] = "Please provide a valid rating between 1 and 5 stars.";
                return RedirectToAction("MyDeliveries");
            }

            // Update delivery with rating and feedback
            delivery.CustomerRating = customerRating;
            delivery.CustomerFeedback = customerFeedback ?? string.Empty;
            delivery.ModifiedDate = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Thank you for your feedback! Your rating has been submitted successfully.";
            return RedirectToAction("MyDeliveries");
        }

        // Helper method to generate tracking number
        private string GenerateTrackingNumber()
        {
            return "AE" + DateTime.UtcNow.ToString("yyyyMMdd") + new Random().Next(1000, 9999).ToString();
        }
    }
}