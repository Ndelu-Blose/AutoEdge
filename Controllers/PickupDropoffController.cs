using AutoEdge.Data;
using AutoEdge.Models.Entities;
using AutoEdge.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AutoEdge.Controllers
{
    [Authorize]
    public class PickupDropoffController : Controller
    {
        private readonly IPickupDropoffService _pickupService;
        private readonly ApplicationDbContext _db;
        private readonly ILogger<PickupDropoffController> _logger;

        public PickupDropoffController(IPickupDropoffService pickupService, ApplicationDbContext db, ILogger<PickupDropoffController> logger)
        {
            _pickupService = pickupService;
            _db = db;
            _logger = logger;
        }

        [HttpGet]
        [Authorize(Roles = "ServiceManager,Administrator")]
        public async Task<IActionResult> Index(string? status = null, string? date = null)
        {
            var query = _db.VehiclePickups
                .Include(p => p.ServiceBooking)
                .Include(p => p.Driver)
                .AsQueryable();

            if (!string.IsNullOrEmpty(status))
            {
                query = query.Where(p => p.PickupStatus == status);
            }

            if (!string.IsNullOrEmpty(date) && DateTime.TryParse(date, out var filterDate))
            {
                query = query.Where(p => p.PickupDate.Date == filterDate.Date);
            }

            var pickups = await query
                .OrderByDescending(p => p.PickupDate)
                .ToListAsync();

            ViewBag.StatusFilter = status;
            ViewBag.DateFilter = date;
            ViewBag.StatusOptions = new[] { "Scheduled", "EnRoute", "Collected", "InService", "Completed", "Delivered", "Failed" };

            return View(pickups);
        }

        [HttpGet]
        public async Task<IActionResult> Create(int? bookingId = null)
        {
            if (bookingId.HasValue)
            {
                var booking = await _db.ServiceBookings
                    .FirstOrDefaultAsync(b => b.Id == bookingId.Value);
                
                if (booking == null)
                {
                    return NotFound();
                }

                ViewBag.Booking = booking;
            }

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreatePickupRequest request)
        {
            if (!ModelState.IsValid)
            {
                return View(request);
            }

            try
            {
                // Validate serviceable zone
                var isServiceable = await _pickupService.CheckServiceableZoneAsync(request.PickupLocation);
                if (!isServiceable)
                {
                    ModelState.AddModelError("PickupLocation", "This address is outside our serviceable area. Please contact us for alternative arrangements.");
                    return View(request);
                }

                // Check driver availability
                var availableDrivers = await _pickupService.GetAvailableDriversAsync(request.PickupDate, request.PickupTime);
                if (!availableDrivers.Any())
                {
                    ModelState.AddModelError("PickupDate", "No drivers are available at the requested time. Please select a different time slot.");
                    return View(request);
                }

                var pickup = await _pickupService.CreatePickupRequestAsync(request);
                
                TempData["SuccessMessage"] = "Pickup request created successfully! A driver will be assigned shortly.";
                return RedirectToAction("Details", new { id = pickup.VehiclePickupId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating pickup request");
                ModelState.AddModelError("", "An error occurred while creating the pickup request. Please try again.");
                return View(request);
            }
        }

        [HttpGet]
        [Authorize(Roles = "ServiceManager,Administrator")]
        public async Task<IActionResult> AssignDriver(int id)
        {
            var pickup = await _db.VehiclePickups
                .Include(p => p.ServiceBooking)
                .FirstOrDefaultAsync(p => p.VehiclePickupId == id);

            if (pickup == null)
            {
                return NotFound();
            }

            var availableDrivers = await _pickupService.GetAvailableDriversAsync(pickup.PickupDate, pickup.PickupTime);
            
            ViewBag.AvailableDrivers = availableDrivers;
            return View(pickup);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "ServiceManager,Administrator")]
        public async Task<IActionResult> AssignDriver(int id, string driverId)
        {
            try
            {
                var success = await _pickupService.AssignDriverAsync(id, driverId);
                
                if (success)
                {
                    TempData["SuccessMessage"] = "Driver assigned successfully!";
                    return RedirectToAction("Index");
                }
                else
                {
                    TempData["ErrorMessage"] = "Failed to assign driver. Please try again.";
                    return RedirectToAction("AssignDriver", new { id });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error assigning driver to pickup {PickupId}", id);
                TempData["ErrorMessage"] = "An error occurred while assigning the driver.";
                return RedirectToAction("AssignDriver", new { id });
            }
        }

        [HttpPost]
        [Authorize(Roles = "Driver")]
        public async Task<IActionResult> UpdateStatus(int id, string status, string? notes = null)
        {
            try
            {
                var success = await _pickupService.UpdatePickupStatusAsync(id, status, notes);
                
                if (success)
                {
                    return Json(new { success = true, message = "Status updated successfully" });
                }
                else
                {
                    return Json(new { success = false, message = "Failed to update status" });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating pickup status for {PickupId}", id);
                return Json(new { success = false, message = "An error occurred while updating status" });
            }
        }

        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var pickup = await _db.VehiclePickups
                .Include(p => p.ServiceBooking)
                .Include(p => p.Driver)
                .FirstOrDefaultAsync(p => p.VehiclePickupId == id);

            if (pickup == null)
            {
                return NotFound();
            }

            return View(pickup);
        }

        [HttpGet]
        [Authorize(Roles = "Driver")]
        public async Task<IActionResult> DriverApp()
        {
            var userId = User.Identity?.Name;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var driver = await _db.Drivers
                .FirstOrDefaultAsync(d => d.UserId == userId);

            if (driver == null)
            {
                return NotFound("Driver profile not found");
            }

            var assignedPickups = await _db.VehiclePickups
                .Include(p => p.ServiceBooking)
                .Where(p => p.DriverId == driver.Id && 
                           (p.PickupStatus == "Scheduled" || p.PickupStatus == "EnRoute" || p.PickupStatus == "Collected" || p.PickupStatus == "InService"))
                .OrderBy(p => p.PickupDate)
                .ToListAsync();

            return View(assignedPickups);
        }

        [HttpGet]
        [Authorize(Roles = "Driver")]
        public async Task<IActionResult> PhotoUpload(int id)
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            var driver = await _db.Drivers.FirstOrDefaultAsync(d => d.UserId == userId);

            if (driver == null)
            {
                return NotFound("Driver profile not found");
            }

            var pickup = await _db.VehiclePickups
                .Include(p => p.ServiceBooking)
                .FirstOrDefaultAsync(p => p.VehiclePickupId == id && p.DriverId == driver.Id);

            if (pickup == null)
            {
                return NotFound();
            }

            return View(pickup);
        }

        [HttpPost]
        [Authorize(Roles = "Driver")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UploadPhotos(int id, List<IFormFile> photos, string photoType)
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            var driver = await _db.Drivers.FirstOrDefaultAsync(d => d.UserId == userId);

            if (driver == null)
            {
                return NotFound("Driver profile not found");
            }

            var pickup = await _db.VehiclePickups
                .FirstOrDefaultAsync(p => p.VehiclePickupId == id && p.DriverId == driver.Id);

            if (pickup == null)
            {
                return NotFound();
            }

            if (photos == null || !photos.Any())
            {
                ModelState.AddModelError("", "Please select at least one photo");
                return View("PhotoUpload", pickup);
            }

            var photoUrls = new List<string>();
            var uploadsPath = Path.Combine("wwwroot", "uploads", "pickup-photos");

            if (!Directory.Exists(uploadsPath))
            {
                Directory.CreateDirectory(uploadsPath);
            }

            foreach (var photo in photos)
            {
                if (photo.Length > 0)
                {
                    var fileName = $"{pickup.VehiclePickupId}_{Guid.NewGuid()}{Path.GetExtension(photo.FileName)}";
                    var filePath = Path.Combine(uploadsPath, fileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await photo.CopyToAsync(stream);
                    }

                    photoUrls.Add($"/uploads/pickup-photos/{fileName}");
                }
            }

            // Update pickup with photo URLs
            var existingPhotos = string.IsNullOrEmpty(pickup.PickupPhotos) 
                ? new List<string>() 
                : System.Text.Json.JsonSerializer.Deserialize<List<string>>(pickup.PickupPhotos) ?? new List<string>();

            existingPhotos.AddRange(photoUrls);
            pickup.PickupPhotos = System.Text.Json.JsonSerializer.Serialize(existingPhotos);
            pickup.UpdatedAt = DateTime.UtcNow;

            await _db.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Successfully uploaded {photoUrls.Count} photos";
            return RedirectToAction("PhotoUpload", new { id });
        }

        [HttpGet]
        public async Task<IActionResult> CustomerPickups()
        {
            var userId = User.Identity?.Name;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var pickups = await _pickupService.GetCustomerPickupsAsync(userId);
            return View(pickups);
        }

        [HttpPost]
        public async Task<IActionResult> ScheduleReturn(int pickupId, DateTime returnDate, TimeSpan returnTime)
        {
            try
            {
                var success = await _pickupService.ScheduleReturnAsync(pickupId, returnDate, returnTime);
                
                if (success)
                {
                    return Json(new { success = true, message = "Return delivery scheduled successfully" });
                }
                else
                {
                    return Json(new { success = false, message = "Failed to schedule return delivery" });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error scheduling return for pickup {PickupId}", pickupId);
                return Json(new { success = false, message = "An error occurred while scheduling return" });
            }
        }
    }
}
