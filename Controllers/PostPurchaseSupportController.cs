using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using AutoEdge.Data;
using AutoEdge.Models.Entities;
using AutoEdge.Models.ViewModels;
using AutoEdge.Services;
using System.Security.Claims;

namespace AutoEdge.Controllers
{
    [Authorize]
    public class PostPurchaseSupportController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<PostPurchaseSupportController> _logger;
        private readonly IQRCodeService _qrCodeService;
        private readonly IEmailService _emailService;
        private readonly IPdfService _pdfService;

        public PostPurchaseSupportController(ApplicationDbContext context, ILogger<PostPurchaseSupportController> logger, IQRCodeService qrCodeService, IEmailService emailService, IPdfService pdfService)
        {
            _context = context;
            _logger = logger;
            _qrCodeService = qrCodeService;
            _emailService = emailService;
            _pdfService = pdfService;
        }

        // GET: PostPurchaseSupport
        public async Task<IActionResult> Index()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var customer = await _context.Customers
                .Include(c => c.User)
                .FirstOrDefaultAsync(c => c.UserId == userId);

            if (customer == null)
            {
                return RedirectToAction("Index", "Home");
            }

            var contracts = await _context.Contracts
                .Include(c => c.Vehicle)
                .Include(c => c.Warranties)
                .Include(c => c.ServiceReminders)
                .Where(c => c.CustomerId == customer.Id && c.Status == "Completed")
                .OrderByDescending(c => c.CreatedDate)
                .ToListAsync();

            return View(contracts);
        }

        // GET: PostPurchaseSupport/ActivateWarranty/5
        public async Task<IActionResult> ActivateWarranty(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var customer = await _context.Customers
                .Include(c => c.User)
                .FirstOrDefaultAsync(c => c.UserId == userId);

            if (customer == null)
            {
                return Unauthorized();
            }

            var contract = await _context.Contracts
                .Include(c => c.Vehicle)
                .Include(c => c.Warranties)
                .FirstOrDefaultAsync(c => c.Id == id && c.CustomerId == customer.Id);

            if (contract == null)
            {
                return NotFound();
            }

            var viewModel = new WarrantyActivationViewModel
            {
                ContractId = contract.Id,
                VehicleInfo = $"{contract.Vehicle.Year} {contract.Vehicle.Make} {contract.Vehicle.Model}",
                VIN = contract.Vehicle.VIN,
                PurchaseDate = contract.CreatedDate,
                ExistingWarranties = contract.Warranties.ToList()
            };

            return View(viewModel);
        }

        // POST: PostPurchaseSupport/ActivateWarranty
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ActivateWarranty(WarrantyActivationViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var customer = await _context.Customers
                .Include(c => c.User)
                .FirstOrDefaultAsync(c => c.UserId == userId);

            if (customer == null)
            {
                return Unauthorized();
            }

            var contract = await _context.Contracts
                .Include(c => c.Vehicle)
                .FirstOrDefaultAsync(c => c.Id == model.ContractId && c.CustomerId == customer.Id);

            if (contract == null)
            {
                return NotFound();
            }

            try
            {
                var warranty = new Warranty
                {
                    ContractId = contract.Id,
                    WarrantyType = model.WarrantyType,
                    StartDate = DateTime.Now,
                    EndDate = DateTime.Now.AddYears(model.DurationYears),
                    CoverageDetails = model.CoverageDetails,
                    Terms = model.Terms,
                    IsActive = true,
                    CreatedDate = DateTime.Now,
                    LastUpdatedDate = DateTime.Now
                };

                _context.Warranties.Add(warranty);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Warranty has been successfully activated!";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error activating warranty for contract {ContractId}", model.ContractId);
                ModelState.AddModelError("", "An error occurred while activating the warranty. Please try again.");
                return View(model);
            }
        }

        // GET: PostPurchaseSupport/ServiceReminders/5
        public async Task<IActionResult> ServiceReminders(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var customer = await _context.Customers
                .Include(c => c.User)
                .FirstOrDefaultAsync(c => c.UserId == userId);

            if (customer == null)
            {
                return Unauthorized();
            }

            var contract = await _context.Contracts
                .Include(c => c.Vehicle)
                .Include(c => c.ServiceReminders)
                .FirstOrDefaultAsync(c => c.Id == id && c.CustomerId == customer.Id);

            if (contract == null)
            {
                return NotFound();
            }

            return View(contract);
        }

        // POST: PostPurchaseSupport/ScheduleService
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ScheduleService(int contractId, string serviceType, DateTime preferredDate, string notes)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var customer = await _context.Customers
                .Include(c => c.User)
                .FirstOrDefaultAsync(c => c.UserId == userId);

            if (customer == null)
            {
                return Json(new { success = false, message = "Customer not found" });
            }

            var contract = await _context.Contracts
                .FirstOrDefaultAsync(c => c.Id == contractId && c.CustomerId == customer.Id);

            if (contract == null)
            {
                return Json(new { success = false, message = "Contract not found" });
            }

            try
            {
                var serviceReminder = new ServiceReminder
                {
                    ContractId = contractId,
                    ServiceType = serviceType,
                    ScheduledDate = preferredDate,
                    Status = "Scheduled",
                    Notes = notes,
                    CreatedDate = DateTime.Now,
                    IsActive = true
                };

                _context.ServiceReminders.Add(serviceReminder);
                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "Service has been scheduled successfully!" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error scheduling service for contract {ContractId}", contractId);
                return Json(new { success = false, message = "An error occurred while scheduling the service" });
            }
        }

        // GET: PostPurchaseSupport/Feedback/5
        public async Task<IActionResult> Feedback(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var customer = await _context.Customers
                .Include(c => c.User)
                .FirstOrDefaultAsync(c => c.UserId == userId);

            if (customer == null)
            {
                return Unauthorized();
            }

            var contract = await _context.Contracts
                .Include(c => c.Vehicle)
                .Include(c => c.CustomerFeedbacks)
                .FirstOrDefaultAsync(c => c.Id == id && c.CustomerId == customer.Id);

            if (contract == null)
            {
                return NotFound();
            }

            var viewModel = new CustomerFeedbackViewModel
            {
                ContractId = contract.Id,
                VehicleInfo = $"{contract.Vehicle.Year} {contract.Vehicle.Make} {contract.Vehicle.Model}",
                PurchaseDate = contract.CreatedDate,
                ExistingFeedback = contract.CustomerFeedbacks.FirstOrDefault()
            };

            return View(viewModel);
        }

        // POST: PostPurchaseSupport/Feedback
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Feedback(CustomerFeedbackViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var customer = await _context.Customers
                .FirstOrDefaultAsync(c => c.UserId == userId);

            if (customer == null)
            {
                return Unauthorized();
            }

            var contract = await _context.Contracts
                .Include(c => c.CustomerFeedbacks)
                .FirstOrDefaultAsync(c => c.Id == model.ContractId && c.CustomerId == customer.Id);

            if (contract == null)
            {
                return NotFound();
            }

            try
            {
                var existingFeedback = contract.CustomerFeedbacks.FirstOrDefault();
                
                if (existingFeedback != null)
                {
                    // Update existing feedback
                    existingFeedback.Rating = model.Rating;
                    existingFeedback.Comments = model.Comments;
                    existingFeedback.ServiceRating = model.ServiceRating;
                    existingFeedback.VehicleRating = model.VehicleRating;
                    existingFeedback.RecommendToOthers = model.RecommendToOthers;
                    existingFeedback.LastUpdatedDate = DateTime.Now;
                }
                else
                {
                    // Create new feedback
                    var feedback = new CustomerFeedback
                    {
                        ContractId = contract.Id,
                        Rating = model.Rating,
                        Comments = model.Comments,
                        ServiceRating = model.ServiceRating,
                        VehicleRating = model.VehicleRating,
                        RecommendToOthers = model.RecommendToOthers,
                        SubmittedDate = DateTime.Now,
                        IsActive = true
                    };

                    _context.CustomerFeedbacks.Add(feedback);
                }

                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Thank you for your feedback!";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error submitting feedback for contract {ContractId}", model.ContractId);
                ModelState.AddModelError("", "An error occurred while submitting your feedback. Please try again.");
                return View(model);
            }
        }

        // GET: PostPurchaseSupport/ManualDelivery/5
        public async Task<IActionResult> ManualDelivery(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var customer = await _context.Customers
                .FirstOrDefaultAsync(c => c.UserId == userId);

            if (customer == null)
            {
                return Unauthorized();
            }

            var contract = await _context.Contracts
                .Include(c => c.Vehicle)
                .FirstOrDefaultAsync(c => c.Id == id && c.CustomerId == customer.Id);

            if (contract == null)
            {
                return NotFound();
            }

            var viewModel = new ManualDeliveryViewModel
            {
                ContractId = contract.Id,
                VehicleInfo = $"{contract.Vehicle?.Year} {contract.Vehicle?.Make ?? ""} {contract.Vehicle?.Model ?? ""}",
                VIN = contract.Vehicle?.VIN ?? "",
                CustomerName = $"{customer.User?.FirstName ?? ""} {customer.User?.LastName ?? ""}",
                CustomerEmail = customer.User?.Email ?? "",
                CustomerPhone = customer.User?.PhoneNumber ?? ""
            };

            return View(viewModel);
        }

        // POST: PostPurchaseSupport/ManualDelivery
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ManualDelivery(ManualDeliveryViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var customer = await _context.Customers
                .FirstOrDefaultAsync(c => c.UserId == userId);

            if (customer == null)
            {
                return Unauthorized();
            }

            var contract = await _context.Contracts
                .Include(c => c.Vehicle)
                .FirstOrDefaultAsync(c => c.Id == model.ContractId && c.CustomerId == customer.Id);

            if (contract == null)
            {
                return NotFound();
            }

            try
            {
                var delivery = new Delivery
                {
                    ContractId = contract.Id,
                    TrackingNumber = GenerateTrackingNumber(),
                    DeliveryType = "Manual",
                    Status = "Scheduled",
                    ScheduledDate = model.DeliveryDate,
                    DeliveryAddress = model.DeliveryAddress,
                    City = model.DeliveryCity,
                    State = model.DeliveryProvince,
                    ZipCode = model.DeliveryPostalCode,
                    ContactPersonName = model.ContactPersonName,
                    ContactPersonPhone = model.ContactPersonPhone,
                    ContactPersonEmail = model.ContactPersonEmail,
                    DeliveryInstructions = model.SpecialInstructions,
                    VehicleLicensePlate = contract.Vehicle.LicensePlate,
                    CreatedDate = DateTime.Now,
                    IsActive = true
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
                            .FirstOrDefaultAsync(c => c.Id == model.ContractId);

                        if (contractWithDetails != null)
                        {
                            var vehicle = contractWithDetails.Purchase?.Vehicle;
                            var contractCustomer = contractWithDetails.Customer;
                            var vehicleInfo = vehicle != null ? $"{vehicle.Year} {vehicle.Make} {vehicle.Model}" : "Vehicle";
                            var customerName = $"{contractCustomer.User.FirstName} {contractCustomer.User.LastName}";
                            
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
                                model.ContactPersonEmail,
                                customerName,
                                vehicleInfo,
                                delivery.TrackingNumber,
                                delivery.ScheduledDate,
                                pdfBytes
                            );
                            
                            _logger.LogInformation($"Delivery QR code email sent to {model.ContactPersonEmail} for tracking {delivery.TrackingNumber}");
                        }
                    }
                    catch (Exception emailEx)
                    {
                        // Log the error but don't fail the delivery creation
                        _logger.LogError(emailEx, $"Error sending QR code email for delivery {delivery.Id}: {emailEx.Message}");
                    }
                }
                catch (Exception ex)
                {
                    // Log the error but don't fail the delivery creation
                    // QR code can be regenerated later if needed
                    _logger.LogError(ex, $"Error generating QR code for delivery {delivery.Id}: {ex.Message}");
                }

                TempData["SuccessMessage"] = $"Manual delivery has been scheduled! Tracking number: {delivery.TrackingNumber}";
                return RedirectToAction("MyDeliveries", "Delivery");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error scheduling manual delivery for contract {ContractId}", model.ContractId);
                ModelState.AddModelError("", "An error occurred while scheduling the delivery. Please try again.");
                return View(model);
            }
        }

        private string GenerateTrackingNumber()
        {
            return $"AE{DateTime.Now:yyyyMMdd}{Random.Shared.Next(1000, 9999)}";
        }
    }
}