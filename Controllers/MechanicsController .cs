using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AutoEdge.Data;
using AutoEdge.Models.Entities;
using AutoEdge.Models.ViewModels;
using AutoEdge.Services;

namespace AutoEdge.Controllers;

[Authorize]
public class MechanicsController : Controller
{
    private readonly ApplicationDbContext _db;
    private readonly IQRCodeService _qrCodeService;
    private readonly IServiceChecklistService _checklistService;
    private readonly ILogger<MechanicsController> _logger;

    public MechanicsController(ApplicationDbContext db, IQRCodeService qrCodeService, IServiceChecklistService checklistService, ILogger<MechanicsController> logger)
    {
        _db = db;
        _qrCodeService = qrCodeService;
        _checklistService = checklistService;
        _logger = logger;
    }

    private async Task<int?> GetCurrentMechanicIdAsync()
    {
        var uid = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(uid)) return null;
        return await _db.MechanicUsers
            .Where(mu => mu.UserId == uid)
            .Select(mu => mu.MechanicId)
            .FirstOrDefaultAsync();
    }

    public async Task<IActionResult> Dashboard()
    {
        var mechanicId = await GetCurrentMechanicIdAsync();
        if (mechanicId == null) return Unauthorized();

        var today = DateOnly.FromDateTime(DateTime.Today);
        var todayJobs = await _db.ServiceJobs
            .Where(j => j.MechanicId == mechanicId && j.ScheduledDate == today && !j.IsCompleted)
            .Include(j => j.ServiceBooking)
            .OrderBy(j => j.ScheduledStart)
            .ToListAsync();

        var weekStart = today;
        var weekEnd = today.AddDays(7);
        var weekJobs = await _db.ServiceJobs
            .Where(j => j.MechanicId == mechanicId && j.ScheduledDate >= weekStart && j.ScheduledDate < weekEnd && !j.IsCompleted)
            .ToListAsync();

        var monthStart = new DateOnly(today.Year, today.Month, 1);
        var completedThisMonth = await _db.ServiceJobs
            .Where(j => j.MechanicId == mechanicId && j.ScheduledDate >= monthStart && j.IsCompleted)
            .CountAsync();

        var inProgressNow = await _db.ServiceJobs
            .Where(j => j.MechanicId == mechanicId && j.ScheduledDate == today && !j.IsCompleted)
            .CountAsync();

        var upcoming = await _db.ServiceJobs
            .Where(j => j.MechanicId == mechanicId && j.ScheduledDate > today && !j.IsCompleted)
            .Include(j => j.ServiceBooking)
            .OrderBy(j => j.ScheduledDate)
            .ThenBy(j => j.ScheduledStart)
            .Take(10)
            .ToListAsync();

        var completedJobs = await _db.ServiceJobs
            .Where(j => j.MechanicId == mechanicId && j.IsCompleted)
            .Include(j => j.ServiceBooking)
            .OrderByDescending(j => j.ScheduledDate)
            .Take(10)
            .ToListAsync();

        var mechanic = await _db.Mechanics.FindAsync(mechanicId);
        var vm = new MechanicsDashboardVm
        {
            Mechanic = mechanic ?? new Mechanic { Name = "Unknown" },
            Kpis = new MechanicKpis(
                todayJobs.Count,
                weekJobs.Count,
                completedThisMonth,
                inProgressNow
            ),
            Today = todayJobs,
            Upcoming = upcoming
        };

        // Add completed jobs to ViewBag
        ViewBag.CompletedJobs = completedJobs;

        return View(vm);
    }

    public async Task<IActionResult> Board()
    {
        var mechanicId = await GetCurrentMechanicIdAsync();
        if (mechanicId == null) return Unauthorized();

        var today = DateOnly.FromDateTime(DateTime.Today);
        var jobs = await _db.ServiceJobs
            .Where(j => j.MechanicId == mechanicId && j.ScheduledDate == today)
            .Include(j => j.ServiceBooking)
            .OrderBy(j => j.ScheduledStart)
            .ToListAsync();

        // Get the current mechanic
        var mechanic = await _db.Mechanics.FindAsync(mechanicId);
        if (mechanic == null) return NotFound();

        // Create the view model
        var vm = new MechanicsBoardVm
        {
            Mechanics = new List<Mechanic> { mechanic },
            JobsByMechanic = new Dictionary<int, List<ServiceJob>>
            {
                { mechanicId.Value, jobs }
            }
        };

        return View(vm);
    }

    [HttpPost]
    public async Task<IActionResult> SetCompleted(int id, bool completed)
    {
        var job = await _db.ServiceJobs.FindAsync(id);
        if (job == null) return NotFound();

        job.IsCompleted = completed;
        await _db.SaveChangesAsync();

        return RedirectToAction(nameof(Board));
    }

    [Authorize(Roles = "Mechanic,Technician,ServiceManager,Administrator")]
    [HttpGet]
    public IActionResult ScanQRCode()
    {
        return View();
    }

    [Authorize(Roles = "Mechanic,Technician,ServiceManager,Administrator")]
    [HttpPost]
    public async Task<IActionResult> ScanQRCode(string qrCodeData)
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

            // Get the associated service job
            var serviceJob = await _db.ServiceJobs
                .Include(j => j.ServiceBooking)
                .FirstOrDefaultAsync(j => j.ServiceBookingId == booking.Id);

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
                    notes = booking.Notes,
                    hasServiceJob = serviceJob != null,
                    serviceJobId = serviceJob?.Id
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing QR code scan");
            return Json(new { success = false, message = "Error processing QR code. Please try again." });
        }
    }

    [Authorize(Roles = "Mechanic,Technician,ServiceManager,Administrator")]
    [HttpPost]
    public async Task<IActionResult> CheckInBooking(string qrCodeData)
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

    [Authorize(Roles = "Mechanic,Technician,ServiceManager,Administrator")]
    [HttpGet]
    public async Task<IActionResult> TestQRCode(int bookingId)
    {
        try
        {
            // First check if booking exists, if not create a test booking
            var booking = await _db.ServiceBookings.FindAsync(bookingId);
            if (booking == null)
            {
                // Create a test booking
                booking = new ServiceBooking
                {
                    Id = bookingId,
                    Reference = $"TEST-{bookingId:D4}",
                    ServiceType = ServiceType.Maintenance,
                    CustomerName = "Test Customer",
                    CustomerEmail = "test@example.com",
                    CustomerPhone = "555-0123",
                    Make = "Test Make",
                    Model = "Test Model",
                    Year = 2024,
                    VIN = $"TEST{bookingId:D10}",
                    Mileage = 50000,
                    PreferredDate = DateOnly.FromDateTime(DateTime.Now.AddDays(1)),
                    PreferredStart = TimeOnly.FromDateTime(DateTime.Now.AddHours(9)),
                    EstimatedDurationMin = 120,
                    EstimatedCost = 150.00m,
                    DeliveryMethod = ServiceDeliveryMethod.Dropoff,
                    Notes = "Test booking for QR code testing",
                    Status = ServiceBookingStatus.Confirmed,
                    CreatedAtUtc = DateTime.UtcNow,
                    CreatedBy = "test"
                };

                _db.ServiceBookings.Add(booking);
                await _db.SaveChangesAsync();
                _logger.LogInformation("Created test booking with ID: {BookingId}", bookingId);
            }

            // Generate QR code for testing
            var qrCodeData = await _qrCodeService.GenerateQRCodeForServiceBookingAsync(bookingId);
            
            return Json(new {
                success = true,
                bookingId = bookingId,
                qrCodeData = qrCodeData,
                bookingReference = booking.Reference,
                customerName = booking.CustomerName,
                vehicle = $"{booking.Make} {booking.Model} ({booking.Year})",
                message = "QR code generated successfully. Use this data to test scanning."
            });
        }
        catch (Exception ex)
        {
            return Json(new {
                success = false,
                error = ex.Message,
                message = "Failed to generate QR code"
            });
        }
    }

    [Authorize(Roles = "Mechanic,Technician,ServiceManager,Administrator")]
    [HttpGet]
    public async Task<IActionResult> CreateTestBookings()
    {
        try
        {
            var testBookings = new List<object>();
            
            for (int i = 1; i <= 5; i++)
            {
                var existingBooking = await _db.ServiceBookings.FindAsync(i);
                if (existingBooking == null)
                {
                    var booking = new ServiceBooking
                    {
                        Id = i,
                        Reference = $"TEST-{i:D4}",
                        ServiceType = (ServiceType)((i % 3) + 1), // Cycle through Maintenance, Repairs, Inspection
                        CustomerName = $"Test Customer {i}",
                        CustomerEmail = $"test{i}@example.com",
                        CustomerPhone = $"555-{i:D4}",
                        Make = $"Test Make {i}",
                        Model = $"Test Model {i}",
                        Year = 2020 + i,
                        VIN = $"TEST{i:D10}",
                        Mileage = 50000 + (i * 1000),
                        PreferredDate = DateOnly.FromDateTime(DateTime.Now.AddDays(i)),
                        PreferredStart = TimeOnly.FromDateTime(DateTime.Now.AddHours(9 + i)),
                        EstimatedDurationMin = 60 + (i * 30),
                        EstimatedCost = 100m + (i * 50),
                        DeliveryMethod = i % 2 == 0 ? ServiceDeliveryMethod.Pickup : ServiceDeliveryMethod.Dropoff,
                        Notes = $"Test booking {i} for QR code testing",
                        Status = ServiceBookingStatus.Confirmed,
                        CreatedAtUtc = DateTime.UtcNow,
                        CreatedBy = "test"
                    };

                    _db.ServiceBookings.Add(booking);
                    testBookings.Add(new {
                        id = i,
                        reference = booking.Reference,
                        customerName = booking.CustomerName,
                        vehicle = $"{booking.Make} {booking.Model} ({booking.Year})"
                    });
                }
            }

            await _db.SaveChangesAsync();
            
            return Json(new {
                success = true,
                message = $"Created {testBookings.Count} test bookings",
                bookings = testBookings
            });
        }
        catch (Exception ex)
        {
            return Json(new {
                success = false,
                error = ex.Message,
                message = "Failed to create test bookings"
            });
        }
    }

    [Authorize(Roles = "Mechanic,Technician,ServiceManager,Administrator")]
    [HttpGet]
    public async Task<IActionResult> ListBookings()
    {
        try
        {
            var bookings = await _db.ServiceBookings
                .OrderByDescending(b => b.Id)
                .Take(10)
                .Select(b => new {
                    id = b.Id,
                    reference = b.Reference,
                    customerName = b.CustomerName,
                    vehicle = $"{b.Make} {b.Model} ({b.Year})",
                    serviceType = b.ServiceType.ToString(),
                    status = b.Status.ToString(),
                    hasQRCode = !string.IsNullOrEmpty(b.QRCode),
                    qrCode = b.QRCode
                })
                .ToListAsync();

            return Json(new {
                success = true,
                count = bookings.Count,
                bookings = bookings
            });
        }
        catch (Exception ex)
        {
            return Json(new {
                success = false,
                error = ex.Message,
                message = "Failed to list bookings"
            });
        }
    }

    [Authorize(Roles = "Mechanic,Technician,ServiceManager,Administrator")]
    [HttpGet]
    public async Task<IActionResult> QuickTest()
    {
        try
        {
            // Ensure test bookings exist
            for (int i = 1; i <= 5; i++)
            {
                var existingBooking = await _db.ServiceBookings.FindAsync(i);
                if (existingBooking == null)
                {
                    var booking = new ServiceBooking
                    {
                        Id = i,
                        Reference = $"TEST-{i:D4}",
                        ServiceType = (ServiceType)((i % 3) + 1), // Cycle through Maintenance, Repairs, Inspection
                        CustomerName = $"Test Customer {i}",
                        CustomerEmail = $"test{i}@example.com",
                        CustomerPhone = $"555-{i:D4}",
                        Make = $"Test Make {i}",
                        Model = $"Test Model {i}",
                        Year = 2020 + i,
                        VIN = $"TEST{i:D10}",
                        Mileage = 50000 + (i * 1000),
                        PreferredDate = DateOnly.FromDateTime(DateTime.Now.AddDays(i)),
                        PreferredStart = TimeOnly.FromDateTime(DateTime.Now.AddHours(9 + i)),
                        EstimatedDurationMin = 60 + (i * 30),
                        EstimatedCost = 100m + (i * 50),
                        DeliveryMethod = i % 2 == 0 ? ServiceDeliveryMethod.Pickup : ServiceDeliveryMethod.Dropoff,
                        Notes = $"Test booking {i} for QR code testing",
                        Status = ServiceBookingStatus.Confirmed,
                        CreatedAtUtc = DateTime.UtcNow,
                        CreatedBy = "test"
                    };

                    _db.ServiceBookings.Add(booking);
                }
            }

        await _db.SaveChangesAsync();

            // Generate QR codes for all bookings
            var qrCodes = new List<object>();
            for (int i = 1; i <= 5; i++)
            {
                var qrCodeData = await _qrCodeService.GenerateQRCodeForServiceBookingAsync(i);
                qrCodes.Add(new {
                    bookingId = i,
                    qrCode = qrCodeData,
                    reference = $"TEST-{i:D4}",
                    customerName = $"Test Customer {i}"
                });
            }

            return Json(new {
                success = true,
                message = "Test bookings created and QR codes generated successfully",
                qrCodes = qrCodes
            });
        }
        catch (Exception ex)
        {
            return Json(new {
                success = false,
                error = ex.Message,
                message = "Failed to create test setup"
            });
        }
    }

    [Authorize(Roles = "Mechanic,Technician,ServiceManager,Administrator")]
    [HttpGet]
    public async Task<IActionResult> ServiceChecklists()
    {
        try
        {
            var mechanicId = await GetCurrentMechanicIdAsync();
            if (mechanicId == null)
            {
                TempData["ErrorMessage"] = "Mechanic not found.";
                return RedirectToAction("Dashboard");
            }

            var checklists = await _checklistService.GetChecklistsByMechanicAsync(mechanicId.Value);
            return View(checklists);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving service checklists");
            TempData["ErrorMessage"] = "Error retrieving service checklists.";
            return RedirectToAction("Dashboard");
        }
    }

    [Authorize(Roles = "Mechanic,Technician,ServiceManager,Administrator")]
    [HttpGet]
    public async Task<IActionResult> ServiceChecklist(int id)
    {
        try
        {
            var checklist = await _checklistService.GetChecklistAsync(id);
            if (checklist == null)
            {
                TempData["ErrorMessage"] = "Service checklist not found.";
                return RedirectToAction("ServiceChecklists");
            }

            return View(checklist);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving service checklist {ChecklistId}", id);
            TempData["ErrorMessage"] = "Error retrieving service checklist.";
            return RedirectToAction("ServiceChecklists");
        }
    }

        [Authorize(Roles = "Mechanic,Technician,ServiceManager,Administrator")]
        [HttpPost]
        public async Task<IActionResult> StartChecklist(int serviceJobId)
        {
            try
            {
                var mechanicId = await GetCurrentMechanicIdAsync();
                if (mechanicId == null)
                {
                    return Json(new { success = false, message = "Mechanic not found." });
                }

                // Get service job details with booking information
                var serviceJob = await _db.ServiceJobs
                    .Include(j => j.ServiceBooking)
                    .FirstOrDefaultAsync(j => j.Id == serviceJobId);

                if (serviceJob == null)
                {
                    return Json(new { success = false, message = "Service job not found." });
                }

                // Check if booking has been checked in
                if (!serviceJob.ServiceBooking.IsCheckedIn)
                {
                    return Json(new { 
                        success = false, 
                        message = "Cannot start service checklist. The customer must check in first using their QR code.",
                        requiresCheckIn = true
                    });
                }

                // Check if checklist already exists
                var existingChecklist = await _checklistService.GetChecklistByServiceJobAsync(serviceJobId);
                if (existingChecklist != null)
                {
                    return Json(new { 
                        success = false, 
                        message = "Checklist already exists for this service job.",
                        checklistId = existingChecklist.Id
                    });
                }

                var checklist = await _checklistService.CreateChecklistAsync(
                    serviceJobId, 
                    mechanicId.Value, 
                    serviceJob.ServiceBooking.ServiceType.ToString()
                );

                return Json(new { 
                    success = true, 
                    message = "Service checklist created successfully.",
                    checklistId = checklist.Id
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error starting service checklist for job {ServiceJobId}", serviceJobId);
                return Json(new { success = false, message = "Error creating service checklist." });
            }
        }

        [Authorize(Roles = "Mechanic,Technician,ServiceManager,Administrator")]
        [HttpPost]
        public async Task<IActionResult> CompleteChecklistItem(int itemId, string? notes, decimal? actualCost, int? actualDurationMinutes)
        {
            try
            {
                var success = await _checklistService.CompleteChecklistItemAsync(itemId, notes, actualCost, actualDurationMinutes);
                
                if (success)
                {
                    return Json(new { success = true, message = "Checklist item completed successfully." });
                }
                else
                {
                    return Json(new { success = false, message = "Failed to complete checklist item." });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error completing checklist item {ItemId}", itemId);
                return Json(new { success = false, message = "Error completing checklist item." });
            }
        }

    [Authorize(Roles = "Mechanic,Technician,ServiceManager,Administrator")]
    [HttpPost]
    public async Task<IActionResult> CompleteChecklist(int checklistId, string? notes)
    {
        try
        {
            var success = await _checklistService.CompleteChecklistAsync(checklistId, notes);
            
            if (success)
            {
                return Json(new { success = true, message = "Service checklist completed successfully." });
            }
            else
            {
                return Json(new { success = false, message = "Failed to complete service checklist." });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error completing service checklist {ChecklistId}", checklistId);
            return Json(new { success = false, message = "Error completing service checklist." });
        }
    }

    [Authorize(Roles = "Mechanic,Technician,ServiceManager,Administrator")]
    [HttpPost]
    public async Task<IActionResult> UploadPhoto(int checklistId, IFormFile photo, string photoType, string? description)
    {
        try
        {
            if (photo == null || photo.Length == 0)
            {
                return Json(new { success = false, message = "No photo file provided." });
            }

            // Validate file type
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
            var fileExtension = Path.GetExtension(photo.FileName).ToLowerInvariant();
            if (!allowedExtensions.Contains(fileExtension))
            {
                return Json(new { success = false, message = "Invalid file type. Only JPG, PNG, and GIF files are allowed." });
            }

            // Create uploads directory if it doesn't exist
            var uploadsPath = Path.Combine("wwwroot", "uploads", "service-photos");
            if (!Directory.Exists(uploadsPath))
            {
                Directory.CreateDirectory(uploadsPath);
            }

            // Generate unique filename
            var fileName = $"{checklistId}_{photoType}_{DateTime.UtcNow:yyyyMMdd_HHmmss}{fileExtension}";
            var filePath = Path.Combine(uploadsPath, fileName);

            // Save file
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await photo.CopyToAsync(stream);
            }

            // Save photo record to database
            var servicePhoto = await _checklistService.AddPhotoAsync(
                checklistId, 
                photoType, 
                $"/uploads/service-photos/{fileName}", 
                fileName, 
                description, 
                User.Identity?.Name ?? "Unknown"
            );

            return Json(new { 
                success = true, 
                message = "Photo uploaded successfully.",
                photoId = servicePhoto.Id,
                photoPath = servicePhoto.FilePath
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading photo for checklist {ChecklistId}", checklistId);
            return Json(new { success = false, message = "Error uploading photo." });
        }
    }
}
