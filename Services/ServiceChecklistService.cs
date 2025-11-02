using AutoEdge.Data;
using AutoEdge.Models.Entities;
using AutoEdge.Services;
using Microsoft.EntityFrameworkCore;

namespace AutoEdge.Services
{
    public class ServiceChecklistService : IServiceChecklistService
    {
        private readonly ApplicationDbContext _db;
        private readonly IServiceNotificationService _notificationService;
        private readonly ILogger<ServiceChecklistService> _logger;

        public ServiceChecklistService(ApplicationDbContext db, IServiceNotificationService notificationService, ILogger<ServiceChecklistService> logger)
        {
            _db = db;
            _notificationService = notificationService;
            _logger = logger;
        }

        public async Task<ServiceChecklist> CreateChecklistAsync(int serviceJobId, int mechanicId, string serviceType, CancellationToken ct = default)
        {
            try
            {
                var checklist = new ServiceChecklist
                {
                    ServiceJobId = serviceJobId,
                    MechanicId = mechanicId,
                    ServiceType = serviceType,
                    StartedAt = DateTime.UtcNow,
                    IsCompleted = false,
                    CreatedAtUtc = DateTime.UtcNow,
                    CreatedBy = "system"
                };

                _db.ServiceChecklists.Add(checklist);
                await _db.SaveChangesAsync(ct);

                // Add default checklist items based on service type
                await AddDefaultChecklistItemsAsync(checklist.Id, serviceType, ct);

                // Update the service booking to indicate service has started
                var serviceBooking = await _db.ServiceBookings
                    .FirstOrDefaultAsync(sb => sb.ServiceJob != null && sb.ServiceJob.Id == serviceJobId, ct);
                
                if (serviceBooking != null)
                {
                    // Update booking status to indicate service has started
                    serviceBooking.Status = ServiceBookingStatus.Confirmed; // Keep as confirmed but service is now active
                    serviceBooking.IsServiceStarted = true;
                    serviceBooking.ServiceStartedDate = DateTime.UtcNow;
                    _db.ServiceBookings.Update(serviceBooking);
                    await _db.SaveChangesAsync(ct);
                    
                    _logger.LogInformation("Service started for booking {BookingId}", serviceBooking.Id);
                }

                _logger.LogInformation("Created service checklist {ChecklistId} for service job {ServiceJobId}", checklist.Id, serviceJobId);
                return checklist;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating service checklist for service job {ServiceJobId}", serviceJobId);
                throw;
            }
        }

        public async Task<ServiceChecklist?> GetChecklistAsync(int checklistId, CancellationToken ct = default)
        {
            return await _db.ServiceChecklists
                .Include(c => c.ServiceJob)
                .ThenInclude(j => j.ServiceBooking)
                .Include(c => c.Mechanic)
                .Include(c => c.Items)
                .Include(c => c.Photos)
                .FirstOrDefaultAsync(c => c.Id == checklistId, ct);
        }

        public async Task<ServiceChecklist?> GetChecklistByServiceJobAsync(int serviceJobId, CancellationToken ct = default)
        {
            return await _db.ServiceChecklists
                .Include(c => c.ServiceJob)
                .ThenInclude(j => j.ServiceBooking)
                .Include(c => c.Mechanic)
                .Include(c => c.Items)
                .Include(c => c.Photos)
                .FirstOrDefaultAsync(c => c.ServiceJobId == serviceJobId, ct);
        }

        public async Task<List<ServiceChecklistItem>> GetChecklistItemsAsync(int checklistId, CancellationToken ct = default)
        {
            return await _db.ServiceChecklistItems
                .Where(i => i.ServiceChecklistId == checklistId)
                .OrderBy(i => i.Id)
                .ToListAsync(ct);
        }

        public async Task<ServiceChecklistItem> AddChecklistItemAsync(int checklistId, string taskName, string? description, decimal? estimatedCost = null, int? estimatedDurationMinutes = null, CancellationToken ct = default)
        {
            try
            {
                var item = new ServiceChecklistItem
                {
                    ServiceChecklistId = checklistId,
                    TaskName = taskName,
                    Description = description,
                    EstimatedCost = estimatedCost,
                    EstimatedDurationMinutes = estimatedDurationMinutes,
                    IsCompleted = false,
                    CreatedAtUtc = DateTime.UtcNow
                };

                _db.ServiceChecklistItems.Add(item);
                await _db.SaveChangesAsync(ct);

                // Update checklist totals
                await UpdateChecklistTotalsAsync(checklistId, ct);

                _logger.LogInformation("Added checklist item {ItemId} to checklist {ChecklistId}", item.Id, checklistId);
                return item;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding checklist item to checklist {ChecklistId}", checklistId);
                throw;
            }
        }

        public async Task<bool> CompleteChecklistItemAsync(int itemId, string? notes, decimal? actualCost = null, int? actualDurationMinutes = null, CancellationToken ct = default)
        {
            try
            {
                var item = await _db.ServiceChecklistItems.FindAsync(itemId);
                if (item == null)
                {
                    _logger.LogWarning("Checklist item {ItemId} not found", itemId);
                    return false;
                }

                item.IsCompleted = true;
                item.CompletedAt = DateTime.UtcNow;
                item.Notes = notes;
                item.ActualCost = actualCost ?? item.EstimatedCost;
                item.ActualDurationMinutes = actualDurationMinutes ?? item.EstimatedDurationMinutes;

                await _db.SaveChangesAsync(ct);

                // Update checklist totals
                await UpdateChecklistTotalsAsync(item.ServiceChecklistId, ct);

                _logger.LogInformation("Completed checklist item {ItemId}", itemId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error completing checklist item {ItemId}", itemId);
                return false;
            }
        }

        public async Task<bool> CompleteChecklistAsync(int checklistId, string? notes, CancellationToken ct = default)
        {
            try
            {
                var checklist = await _db.ServiceChecklists.FindAsync(checklistId);
                if (checklist == null)
                {
                    _logger.LogWarning("Service checklist {ChecklistId} not found", checklistId);
                    return false;
                }

                checklist.IsCompleted = true;
                checklist.CompletedAt = DateTime.UtcNow;
                checklist.Notes = notes;

                // Also mark the service job as completed
                var serviceJob = await _db.ServiceJobs.FindAsync(checklist.ServiceJobId);
                if (serviceJob != null)
                {
                    serviceJob.IsCompleted = true;
                }

                await _db.SaveChangesAsync(ct);

                // Trigger customer notification workflow
                var booking = await _db.ServiceBookings
                    .FirstOrDefaultAsync(b => b.ServiceJob != null && b.ServiceJob.Id == checklist.ServiceJobId, ct);
                
                if (booking != null)
                {
                    // Update booking status to completed
                    booking.Status = ServiceBookingStatus.Completed;
                    booking.ServiceCompletedDate = DateTime.UtcNow;
                    await _db.SaveChangesAsync(ct);

                    // Trigger notification service to send completion notification and generate invoice
                    try
                    {
                        await _notificationService.SendServiceCompletionNotificationAsync(booking.Id);
                        _logger.LogInformation("Service completion notification sent for booking {BookingId}", booking.Id);
                    }
                    catch (Exception notificationEx)
                    {
                        _logger.LogError(notificationEx, "Failed to send service completion notification for booking {BookingId}", booking.Id);
                        // Don't fail the checklist completion if notification fails
                    }
                }

                _logger.LogInformation("Completed service checklist {ChecklistId}", checklistId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error completing service checklist {ChecklistId}", checklistId);
                return false;
            }
        }

        public async Task<ServicePhoto> AddPhotoAsync(int checklistId, string photoType, string filePath, string fileName, string? description, string takenBy, CancellationToken ct = default)
        {
            try
            {
                var photo = new ServicePhoto
                {
                    ServiceChecklistId = checklistId,
                    PhotoType = photoType,
                    FilePath = filePath,
                    FileName = fileName,
                    Description = description,
                    TakenAt = DateTime.UtcNow,
                    TakenBy = takenBy,
                    CreatedAtUtc = DateTime.UtcNow
                };

                _db.ServicePhotos.Add(photo);
                await _db.SaveChangesAsync(ct);

                _logger.LogInformation("Added photo {PhotoId} to checklist {ChecklistId}", photo.Id, checklistId);
                return photo;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding photo to checklist {ChecklistId}", checklistId);
                throw;
            }
        }

        public async Task<List<ServicePhoto>> GetPhotosAsync(int checklistId, CancellationToken ct = default)
        {
            return await _db.ServicePhotos
                .Where(p => p.ServiceChecklistId == checklistId)
                .OrderBy(p => p.TakenAt)
                .ToListAsync(ct);
        }

        public async Task<List<ServiceChecklist>> GetChecklistsByMechanicAsync(int mechanicId, CancellationToken ct = default)
        {
            return await _db.ServiceChecklists
                .Include(c => c.ServiceJob)
                .ThenInclude(j => j.ServiceBooking)
                .Include(c => c.Items)
                .Where(c => c.MechanicId == mechanicId)
                .OrderByDescending(c => c.StartedAt)
                .ToListAsync(ct);
        }

        public async Task<List<ServiceChecklist>> GetActiveChecklistsAsync(CancellationToken ct = default)
        {
            return await _db.ServiceChecklists
                .Include(c => c.ServiceJob)
                .ThenInclude(j => j.ServiceBooking)
                .Include(c => c.Mechanic)
                .Include(c => c.Items)
                .Where(c => !c.IsCompleted)
                .OrderByDescending(c => c.StartedAt)
                .ToListAsync(ct);
        }

        public async Task<List<ServiceChecklist>> GetCompletedChecklistsAsync(CancellationToken ct = default)
        {
            return await _db.ServiceChecklists
                .Include(c => c.ServiceJob)
                .ThenInclude(j => j.ServiceBooking)
                .Include(c => c.Mechanic)
                .Include(c => c.Items)
                .Include(c => c.Photos)
                .Where(c => c.IsCompleted)
                .OrderByDescending(c => c.CompletedAt)
                .ToListAsync(ct);
        }

        private async Task AddDefaultChecklistItemsAsync(int checklistId, string serviceType, CancellationToken ct = default)
        {
            var defaultItems = GetDefaultChecklistItems(serviceType);
            
            foreach (var item in defaultItems)
            {
                var checklistItem = new ServiceChecklistItem
                {
                    ServiceChecklistId = checklistId,
                    TaskName = item.TaskName,
                    Description = item.Description,
                    EstimatedCost = item.EstimatedCost,
                    EstimatedDurationMinutes = item.EstimatedDuration,
                    IsCompleted = false,
                    CreatedAtUtc = DateTime.UtcNow
                };

                _db.ServiceChecklistItems.Add(checklistItem);
            }

            await _db.SaveChangesAsync(ct);
            
            // Update checklist totals after adding items
            await UpdateChecklistTotalsAsync(checklistId, ct);
        }

        private async Task UpdateChecklistTotalsAsync(int checklistId, CancellationToken ct = default)
        {
            try
            {
                var checklist = await _db.ServiceChecklists.FindAsync(checklistId);
                if (checklist == null) return;

                var items = await _db.ServiceChecklistItems
                    .Where(i => i.ServiceChecklistId == checklistId)
                    .ToListAsync(ct);

                // Update counts
                checklist.TotalItemsCount = items.Count;
                checklist.CompletedItemsCount = items.Count(i => i.IsCompleted);

                // Update costs
                checklist.TotalEstimatedCost = items.Sum(i => i.EstimatedCost ?? 0);
                checklist.TotalActualCost = items.Where(i => i.IsCompleted).Sum(i => i.ActualCost ?? 0);

                // Update durations
                checklist.TotalEstimatedDurationMinutes = items.Sum(i => i.EstimatedDurationMinutes ?? 0);
                checklist.TotalActualDurationMinutes = items.Where(i => i.IsCompleted).Sum(i => i.ActualDurationMinutes ?? 0);

                _db.ServiceChecklists.Update(checklist);
                await _db.SaveChangesAsync(ct);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating checklist totals for checklist {ChecklistId}", checklistId);
            }
        }

        private List<(string TaskName, string? Description, decimal? EstimatedCost, int? EstimatedDuration)> GetDefaultChecklistItems(string serviceType)
        {
            return serviceType.ToLower() switch
            {
                "maintenance" => new List<(string, string?, decimal?, int?)>
                {
                    ("Visual Inspection", "Check for visible damage, leaks, or wear", 250.00m, 15),
                    ("Oil Change", "Replace engine oil and oil filter", 450.00m, 30),
                    ("Air Filter Check", "Inspect and replace air filter if needed", 150.00m, 10),
                    ("Tire Inspection", "Check tire pressure, tread depth, and condition", 200.00m, 15),
                    ("Brake Inspection", "Check brake pads, rotors, and fluid levels", 350.00m, 25),
                    ("Battery Test", "Test battery voltage and connections", 150.00m, 10),
                    ("Fluid Levels Check", "Check all fluid levels (coolant, brake, power steering)", 200.00m, 15),
                    ("Lighting Check", "Test all lights (headlights, taillights, indicators)", 150.00m, 10),
                    ("Final Test Drive", "Perform test drive to verify all systems", 100.00m, 15)
                },
                "repairs" => new List<(string, string?, decimal?, int?)>
                {
                    ("Diagnostic Scan", "Run diagnostic scan to identify issues", 750.00m, 30),
                    ("Visual Inspection", "Thorough visual inspection of affected area", 300.00m, 20),
                    ("Component Removal", "Remove necessary components for repair", 500.00m, 45),
                    ("Parts Replacement", "Replace faulty or worn parts", 1000.00m, 60),
                    ("System Testing", "Test repaired systems for proper function", 400.00m, 30),
                    ("Quality Check", "Verify repair quality and safety", 250.00m, 20),
                    ("Cleanup", "Clean work area and vehicle", 150.00m, 15),
                    ("Final Inspection", "Final inspection before completion", 200.00m, 15),
                    ("Test Drive", "Test drive to verify repair success", 150.00m, 20)
                },
                "inspection" => new List<(string, string?, decimal?, int?)>
                {
                    ("Safety Inspection", "Check all safety systems and components", 500.00m, 30),
                    ("Emissions Test", "Test vehicle emissions compliance", 400.00m, 20),
                    ("Roadworthiness Check", "Verify vehicle is roadworthy", 350.00m, 25),
                    ("Documentation", "Complete inspection documentation", 200.00m, 15),
                    ("Report Generation", "Generate inspection report", 150.00m, 10)
                },
                _ => new List<(string, string?, decimal?, int?)>
                {
                    ("Initial Assessment", "Assess vehicle condition and requirements", 300.00m, 20),
                    ("Service Performance", "Perform required service work", 600.00m, 45),
                    ("Quality Check", "Verify service quality and completion", 250.00m, 20),
                    ("Final Inspection", "Final inspection before delivery", 200.00m, 15)
                }
            };
        }
    }
}
