using AutoEdge.Data;
using AutoEdge.Models.Entities;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace AutoEdge.Services
{
    public class PickupDropoffService : IPickupDropoffService
    {
        private readonly ApplicationDbContext _db;
        private readonly IEmailService _emailService;
        private readonly ILogger<PickupDropoffService> _logger;

        public PickupDropoffService(ApplicationDbContext db, IEmailService emailService, ILogger<PickupDropoffService> logger)
        {
            _db = db;
            _emailService = emailService;
            _logger = logger;
        }

        public async Task<bool> CheckServiceableZoneAsync(string address)
        {
            try
            {
                // For now, implement basic validation
                // In production, integrate with geocoding service
                var serviceableZones = new[]
                {
                    "Downtown", "Midtown", "Uptown", "East Side", "West Side",
                    "North End", "South End", "Central District"
                };

                var addressLower = address.ToLower();
                return serviceableZones.Any(zone => addressLower.Contains(zone.ToLower()));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking serviceable zone for address: {Address}", address);
                return false;
            }
        }

        public async Task<List<DriverAssignment>> GetAvailableDriversAsync(DateTime date, TimeSpan time)
        {
            try
            {
                // Get the seeded driver's UserId (mkhizesigcino69@gmail.com)
                var seededDriverUser = await _db.Users
                    .Where(u => u.Email == "mkhizesigcino69@gmail.com")
                    .Select(u => u.Id)
                    .FirstOrDefaultAsync();
                
                var availableDrivers = await _db.Drivers
                    .Where(d => d.IsAvailable)
                    .Select(d => new DriverAssignment
                    {
                        DriverId = d.UserId,
                        DriverName = d.Name,
                        DriverPhone = d.Phone,
                        VehicleRegistration = d.VehicleRegistration,
                        Rating = (double)d.Rating,
                        IsAvailable = true
                    })
                    .ToListAsync();

                // Check for conflicts - convert DriverId (string) to int for comparison
                var conflictingPickups = await _db.VehiclePickups
                    .Where(p => p.PickupDate.Date == date.Date &&
                               (p.PickupStatus == "Scheduled" || p.PickupStatus == "EnRoute" || p.PickupStatus == "Collected" || p.PickupStatus == "InService") &&
                               p.PickupTime <= time.Add(TimeSpan.FromHours(2)) &&
                               p.PickupTime >= time.Subtract(TimeSpan.FromHours(2)) &&
                               p.DriverId.HasValue)
                    .Select(p => p.DriverId!.Value)
                    .ToListAsync();

                // Get the Driver.Id values to compare with conflicting pickups
                var driverIds = await _db.Drivers
                    .Where(d => d.IsAvailable)
                    .Select(d => new { d.Id, d.UserId })
                    .ToListAsync();

                // Filter out drivers with conflicts and prioritize seeded driver
                return availableDrivers
                    .Where(d => 
                    {
                        var driverInfo = driverIds.FirstOrDefault(di => di.UserId == d.DriverId);
                        if (driverInfo == null) return false;
                        return !conflictingPickups.Contains(driverInfo.Id);
                    })
                    .OrderByDescending(d => d.DriverId == seededDriverUser ? 1 : 0) // Prioritize seeded driver
                    .ThenByDescending(d => d.Rating) // Then by rating
                    .ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting available drivers for {Date} at {Time}", date, time);
                return new List<DriverAssignment>();
            }
        }

        public async Task<VehiclePickup> CreatePickupRequestAsync(CreatePickupRequest request)
        {
            try
            {
                // Automatically assign the best available driver
                var availableDrivers = await GetAvailableDriversAsync(request.PickupDate, request.PickupTime);
                var selectedDriver = availableDrivers.FirstOrDefault();
                
                int? driverId = null;
                if (selectedDriver != null)
                {
                    // Look up the actual Driver.Id from the UserId
                    var driver = await _db.Drivers.FirstOrDefaultAsync(d => d.UserId == selectedDriver.DriverId);
                    driverId = driver?.Id;
                    
                    if (driverId == null)
                    {
                        _logger.LogWarning("Driver with UserId {UserId} not found in Drivers table", selectedDriver.DriverId);
                        selectedDriver = null; // Reset to null if lookup failed
                    }
                }
                
                if (driverId == null)
                {
                    _logger.LogWarning("No available drivers found for pickup at {Date} {Time}", 
                        request.PickupDate, request.PickupTime);
                }

                var pickup = new VehiclePickup
                {
                    ServiceBookingId = request.ServiceBookingId ?? 0,
                    DriverId = driverId,
                    PickupLocation = request.PickupLocation,
                    PickupDate = request.PickupDate,
                    PickupTime = request.PickupTime,
                    PickupStatus = driverId != null ? "Assigned" : "Scheduled",
                    VehicleConditionPickup = JsonSerializer.Serialize(new
                    {
                        VehicleMake = request.VehicleMake,
                        VehicleModel = request.VehicleModel,
                        VehicleYear = request.VehicleYear,
                        LicensePlate = request.VehicleLicensePlate,
                        SpecialInstructions = request.SpecialInstructions
                    }),
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                _db.VehiclePickups.Add(pickup);
                await _db.SaveChangesAsync();

                // Send notifications
                if (selectedDriver != null)
                {
                    // Notify driver
                    await SendDriverNotificationAsync(pickup.VehiclePickupId, 
                        $"You have been automatically assigned to pickup {request.CustomerName}'s vehicle from {request.PickupLocation}");
                    
                    // Notify customer
                    await SendCustomerNotificationAsync(pickup.VehiclePickupId, 
                        $"Your pickup has been assigned to driver {selectedDriver.DriverName}");
                    
                    _logger.LogInformation("Automatically assigned driver {DriverName} (ID: {DriverId}) to pickup {PickupId}", 
                        selectedDriver.DriverName, selectedDriver.DriverId, pickup.VehiclePickupId);
                }
                else
                {
                    // No drivers available - notify service managers only
                    await SendManagerNotificationAsync(pickup);
                    _logger.LogInformation("No available drivers found. Pickup {PickupId} scheduled without driver assignment", 
                        pickup.VehiclePickupId);
                }

                return pickup;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating pickup request");
                throw;
            }
        }

        public async Task<bool> AssignDriverAsync(int pickupId, string driverId)
        {
            try
            {
                var pickup = await _db.VehiclePickups.FindAsync(pickupId);
                if (pickup == null)
                {
                    return false;
                }

                var driver = await _db.Drivers.FirstOrDefaultAsync(d => d.UserId == driverId);
                if (driver == null)
                {
                    return false;
                }

                pickup.DriverId = driver.Id;
                pickup.PickupStatus = "Assigned";
                pickup.UpdatedAt = DateTime.UtcNow;

                await _db.SaveChangesAsync();

                // Send notifications
                await SendDriverNotificationAsync(pickupId, "You have been assigned a new pickup request");
                await SendCustomerNotificationAsync(pickupId, "Your pickup has been assigned to a driver");

                _logger.LogInformation("Assigned driver {DriverId} to pickup {PickupId}", driverId, pickupId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error assigning driver {DriverId} to pickup {PickupId}", driverId, pickupId);
                return false;
            }
        }

        public async Task<bool> UpdatePickupStatusAsync(int pickupId, string status, string? notes = null)
        {
            try
            {
                var pickup = await _db.VehiclePickups.FindAsync(pickupId);
                if (pickup == null)
                {
                    return false;
                }

                var validStatuses = new[] { "EnRoute", "Collected", "InService", "Completed", "Delivered", "Failed" };
                if (!validStatuses.Contains(status))
                {
                    return false;
                }

                pickup.PickupStatus = status;
                pickup.UpdatedAt = DateTime.UtcNow;

                if (!string.IsNullOrEmpty(notes))
                {
                    pickup.VehicleConditionPickup = notes;
                }

                // Update timestamps based on status
                switch (status)
                {
                    case "Collected":
                        pickup.PickupDate = DateTime.UtcNow;
                        break;
                    case "Delivered":
                        pickup.DropoffDate = DateTime.UtcNow;
                        break;
                }

                await _db.SaveChangesAsync();

                // Send appropriate notifications
                await SendStatusNotificationAsync(pickupId, status);

                _logger.LogInformation("Updated pickup {PickupId} status to {Status}", pickupId, status);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating pickup {PickupId} status to {Status}", pickupId, status);
                return false;
            }
        }

        public async Task<bool> UploadConditionPhotosAsync(int pickupId, List<IFormFile> photos, string photoType)
        {
            try
            {
                var pickup = await _db.VehiclePickups.FindAsync(pickupId);
                if (pickup == null)
                {
                    return false;
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
                        var fileName = $"{pickupId}_{photoType}_{Guid.NewGuid()}{Path.GetExtension(photo.FileName)}";
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
                    : JsonSerializer.Deserialize<List<string>>(pickup.PickupPhotos) ?? new List<string>();

                existingPhotos.AddRange(photoUrls);
                pickup.PickupPhotos = JsonSerializer.Serialize(existingPhotos);
                pickup.UpdatedAt = DateTime.UtcNow;

                await _db.SaveChangesAsync();

                _logger.LogInformation("Uploaded {Count} photos for pickup {PickupId}", photoUrls.Count, pickupId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading photos for pickup {PickupId}", pickupId);
                return false;
            }
        }

        public async Task<bool> ScheduleReturnAsync(int pickupId, DateTime returnDate, TimeSpan returnTime)
        {
            try
            {
                var pickup = await _db.VehiclePickups.FindAsync(pickupId);
                if (pickup == null)
                {
                    return false;
                }

                pickup.DropoffDate = returnDate;
                pickup.DropoffTime = returnTime;
                pickup.PickupStatus = "ScheduledReturn";
                pickup.UpdatedAt = DateTime.UtcNow;

                await _db.SaveChangesAsync();

                // Notify driver and customer
                await SendDriverNotificationAsync(pickupId, $"Return delivery scheduled for {returnDate:yyyy-MM-dd} at {returnTime}");
                await SendCustomerNotificationAsync(pickupId, $"Your vehicle return has been scheduled for {returnDate:yyyy-MM-dd} at {returnTime}");

                _logger.LogInformation("Scheduled return for pickup {PickupId} on {Date} at {Time}", 
                    pickupId, returnDate, returnTime);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error scheduling return for pickup {PickupId}", pickupId);
                return false;
            }
        }

        public async Task<List<VehiclePickup>> GetCustomerPickupsAsync(string customerId)
        {
            try
            {
                return await _db.VehiclePickups
                    .Include(p => p.ServiceBooking)
                    .Include(p => p.Driver)
                    .Where(p => p.ServiceBooking.CustomerId == customerId)
                    .OrderByDescending(p => p.PickupDate)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting pickups for customer {CustomerId}", customerId);
                return new List<VehiclePickup>();
            }
        }

        public async Task<VehiclePickup?> GetPickupDetailsAsync(int pickupId)
        {
            try
            {
                return await _db.VehiclePickups
                    .Include(p => p.ServiceBooking)
                    .Include(p => p.Driver)
                    .FirstOrDefaultAsync(p => p.VehiclePickupId == pickupId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting pickup details for {PickupId}", pickupId);
                return null;
            }
        }

        public async Task<bool> CaptureCustomerSignatureAsync(int pickupId, string signatureData)
        {
            try
            {
                var pickup = await _db.VehiclePickups.FindAsync(pickupId);
                if (pickup == null)
                {
                    return false;
                }

                pickup.CustomerSignatures = signatureData;
                pickup.UpdatedAt = DateTime.UtcNow;

                await _db.SaveChangesAsync();

                _logger.LogInformation("Captured customer signature for pickup {PickupId}", pickupId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error capturing signature for pickup {PickupId}", pickupId);
                return false;
            }
        }

        public async Task<bool> SendDriverNotificationAsync(int pickupId, string message)
        {
            try
            {
                var pickup = await GetPickupDetailsAsync(pickupId);
                if (pickup?.Driver == null)
                {
                    return false;
                }

                // In production, integrate with SMS service
                _logger.LogInformation("Driver notification for pickup {PickupId}: {Message}", pickupId, message);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending driver notification for pickup {PickupId}", pickupId);
                return false;
            }
        }

        public async Task<bool> SendCustomerNotificationAsync(int pickupId, string message)
        {
            try
            {
                var pickup = await GetPickupDetailsAsync(pickupId);
                if (pickup?.ServiceBooking == null)
                {
                    return false;
                }

                // Send email notification
                await _emailService.SendEmailAsync(
                    pickup.ServiceBooking.CustomerEmail,
                    "Pickup Status Update",
                    $"Your vehicle pickup status has been updated: {message}"
                );

                _logger.LogInformation("Customer notification sent for pickup {PickupId}: {Message}", pickupId, message);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending customer notification for pickup {PickupId}", pickupId);
                return false;
            }
        }

        private async Task SendManagerNotificationAsync(VehiclePickup pickup)
        {
            try
            {
                // Send notification to service managers about new pickup request
                _logger.LogInformation("New pickup request {PickupId} requires driver assignment", pickup.VehiclePickupId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending manager notification for pickup {PickupId}", pickup.VehiclePickupId);
            }
        }

        private async Task SendStatusNotificationAsync(int pickupId, string status)
        {
            try
            {
                var message = status switch
                {
                    "EnRoute" => "Driver is on the way to collect your vehicle",
                    "Collected" => "Your vehicle has been collected and is being transported to our service center",
                    "InService" => "Your vehicle is now at our service center and work has begun",
                    "Completed" => "Service work on your vehicle has been completed",
                    "Delivered" => "Your vehicle has been delivered back to you",
                    "Failed" => "There was an issue with your pickup. Please contact us for assistance",
                    _ => $"Your pickup status has been updated to: {status}"
                };

                await SendCustomerNotificationAsync(pickupId, message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending status notification for pickup {PickupId}", pickupId);
            }
        }
    }
}
