using AutoEdge.Data;
using AutoEdge.Models.Entities;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace AutoEdge.Services
{
    public class BookingService : IBookingService
    {
        private readonly ApplicationDbContext _db;
        private readonly IQRCodeService _qrCodeService;
        private readonly IEmailService _emailService;
        private readonly IQRCodePdfService _qrCodePdfService;
        private readonly ILogger<BookingService> _logger;
        private readonly IPickupDropoffService _pickupService;

        public BookingService(ApplicationDbContext db, IQRCodeService qrCodeService, IEmailService emailService, IQRCodePdfService qrCodePdfService, ILogger<BookingService> logger, IPickupDropoffService pickupService)
        {
            _db = db;
            _qrCodeService = qrCodeService;
            _emailService = emailService;
            _qrCodePdfService = qrCodePdfService;
            _logger = logger;
            _pickupService = pickupService;
        }

        public async Task<ServiceBooking> CreateBookingAsync(CreateBookingRequest request, CancellationToken ct = default)
        {
            var reference = $"SV-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString()[..4].ToUpper()}";

            // Validate business hours (8 AM to 5 PM)
            if (request.PreferredStart < TimeOnly.FromTimeSpan(TimeSpan.FromHours(8)) ||
                request.PreferredStart >= TimeOnly.FromTimeSpan(TimeSpan.FromHours(17)))
            {
                throw new InvalidOperationException("Service bookings are only available during business hours (8:00 AM to 5:00 PM).");
            }

            // Validate future date only
            if (request.PreferredDate <= DateOnly.FromDateTime(DateTime.Today))
            {
                throw new InvalidOperationException("Service bookings must be scheduled for future dates only.");
            }

            // smarter duplicate check: block only overlapping times on same day
            // build the requested window using estimated duration
            var start = request.PreferredStart;
            var end = start.Add(TimeSpan.FromMinutes(
                request.ServiceType switch
                {
                    ServiceType.Maintenance => 90,
                    ServiceType.Repairs => 180,
                    ServiceType.Inspection => 60,
                    _ => 60
                }));

            var sameDayBookings = await _db.ServiceBookings
                .Where(b =>
                    b.Status != ServiceBookingStatus.Canceled &&
                    b.PreferredDate == request.PreferredDate &&
                    (
                        (!string.IsNullOrWhiteSpace(request.VIN) && b.VIN == request.VIN) ||
                        (string.IsNullOrWhiteSpace(request.VIN) && b.VIN == null &&
                         b.Make == request.Make && b.Model == request.Model && b.Year == request.Year)
                    ))
                .ToListAsync(ct);

            var exists = sameDayBookings.Any(b =>
                !(end <= b.PreferredStart ||
                  start >= b.PreferredStart.Add(TimeSpan.FromMinutes(b.EstimatedDurationMin))));

            if (exists)
                throw new InvalidOperationException("A booking already exists for this vehicle and date.");

            // Auto-assign available technician (prevent double booking)
            var availableMechanic = await FindAvailableMechanicAsync(request.PreferredDate, start, end, ct);
            if (availableMechanic == null)
            {
                throw new InvalidOperationException("No technicians are available at the requested time. Please choose a different time slot.");
            }

            var estDuration = request.ServiceType switch
            {
                ServiceType.Maintenance => 90,
                ServiceType.Repairs => 180,
                ServiceType.Inspection => 60,
                _ => 60
            };
            var estCost = request.ServiceType switch
            {
                ServiceType.Maintenance => 1500m,
                ServiceType.Repairs => 3000m,
                ServiceType.Inspection => 800m,
                _ => 1000m
            };

            var booking = new ServiceBooking
            {
                Reference = reference,
                ServiceType = request.ServiceType,
                CustomerId = request.CustomerId,
                CustomerName = request.CustomerName,
                CustomerEmail = request.CustomerEmail,
                CustomerPhone = request.CustomerPhone,
                Make = request.Make,
                Model = request.Model,
                Year = request.Year,
                VIN = request.VIN,
                Mileage = request.Mileage,
                PreferredDate = request.PreferredDate,
                PreferredStart = request.PreferredStart,
                EstimatedDurationMin = estDuration,
                EstimatedCost = estCost,
                DeliveryMethod = request.DeliveryMethod,
                Notes = request.Notes,
                Status = ServiceBookingStatus.Pending,
                CreatedAtUtc = DateTime.UtcNow,
                CreatedBy = "web"
            };

            _db.ServiceBookings.Add(booking);
            await _db.SaveChangesAsync(ct);

            // Auto-create service job with assigned mechanic
            var serviceJob = new ServiceJob
            {
                ServiceBookingId = booking.Id,
                MechanicId = availableMechanic.Id,
                ScheduledDate = request.PreferredDate,
                ScheduledStart = request.PreferredStart,
                DurationMin = estDuration,
                IsCompleted = false
            };

            _db.ServiceJobs.Add(serviceJob);
            booking.Status = ServiceBookingStatus.Confirmed;
            await _db.SaveChangesAsync(ct);

            // Generate and send QR code email
            try
            {
                await GenerateAndSendQRCodeEmailAsync(booking);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send QR code email for booking {BookingId}", booking.Id);
                // Don't fail the booking creation if email fails
            }

            // Automatically create pickup request if DeliveryMethod is Pickup
            if (request.DeliveryMethod == ServiceDeliveryMethod.Pickup && !string.IsNullOrWhiteSpace(request.PickupAddress))
            {
                try
                {
                    await CreateAutomaticPickupRequestAsync(booking, request);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to create pickup request for booking {BookingId}", booking.Id);
                    // Don't fail the booking creation if pickup request fails
                }
            }

            return booking;
        }

        public async Task<ServiceBooking?> ConfirmBookingAsync(int bookingId, CancellationToken ct = default)
        {
            var booking = await _db.ServiceBookings.FirstOrDefaultAsync(b => b.Id == bookingId, ct);
            if (booking == null) return null;
            booking.Status = ServiceBookingStatus.Confirmed;
            await _db.SaveChangesAsync(ct);
            return booking;
        }

        public async Task<ServiceBooking?> GetBookingAsync(int bookingId, CancellationToken ct = default)
        {
            return await _db.ServiceBookings.FirstOrDefaultAsync(b => b.Id == bookingId, ct);
        }

        public async Task<ServiceBooking?> GetBookingWithJobAsync(int bookingId, CancellationToken ct = default)
        {
            return await _db.ServiceBookings
                .Include(b => b.ServiceJob)
                .ThenInclude(j => j.Mechanic)
                .Include(b => b.VehiclePickups)
                .FirstOrDefaultAsync(b => b.Id == bookingId, ct);
        }

        private async Task<Mechanic?> FindAvailableMechanicAsync(DateOnly date, TimeOnly startTime, TimeOnly endTime, CancellationToken ct = default)
        {
            // Get all available mechanics
            var availableMechanics = await _db.Mechanics
                .Where(m => m.IsAvailable)
                .OrderByDescending(m => m.Rating)
                .ToListAsync(ct);

            foreach (var mechanic in availableMechanics)
            {
                // Check if mechanic has any conflicting bookings on the same date
                var conflictingJobs = await _db.ServiceJobs
                    .Where(j => j.MechanicId == mechanic.Id && 
                               j.ScheduledDate == date &&
                               j.IsCompleted == false)
                    .ToListAsync(ct);

                // Check for time conflicts
                var hasConflict = conflictingJobs.Any(job =>
                {
                    var jobStart = job.ScheduledStart;
                    var jobEnd = jobStart.Add(TimeSpan.FromMinutes(job.DurationMin));
                    
                    // Check if the new booking time overlaps with existing job
                    return !(endTime <= jobStart || startTime >= jobEnd);
                });

                if (!hasConflict)
                {
                    return mechanic;
                }
            }

            return null; // No available mechanic found
        }

        private async Task GenerateAndSendQRCodeEmailAsync(ServiceBooking booking)
        {
            try
            {
                // Generate QR code
                var qrCodeData = await _qrCodeService.GenerateQRCodeForServiceBookingAsync(booking.Id);
                var qrCodeImage = await _qrCodeService.GenerateQRCodeImageAsync(qrCodeData);

                // Generate PDF ticket
                var pdfTicket = await _qrCodePdfService.GenerateServiceBookingQRCodePdfAsync(booking, qrCodeImage);

                // Prepare email data
                var vehicleInfo = $"{booking.Make} {booking.Model} ({booking.Year})";
                var scheduledDateTime = booking.PreferredDate.ToDateTime(booking.PreferredStart);

                // Send email with QR code PDF ticket
                await _emailService.SendServiceBookingQRCodeEmailWithPdfAsync(
                    booking.CustomerEmail,
                    booking.CustomerName,
                    vehicleInfo,
                    booking.Reference,
                    scheduledDateTime,
                    booking.ServiceType.ToString(),
                    pdfTicket
                );

                _logger.LogInformation("QR code PDF ticket email sent successfully for booking {BookingId}", booking.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating and sending QR code PDF ticket email for booking {BookingId}", booking.Id);
                throw;
            }
        }

        public async Task<List<ServiceBooking>> GetAllBookingsAsync(CancellationToken ct = default)
        {
            return await _db.ServiceBookings
                .Include(b => b.ServiceJob)
                .ThenInclude(j => j.Mechanic)
                .OrderByDescending(b => b.CreatedAtUtc)
                .ToListAsync(ct);
        }

        public async Task<List<ServiceBooking>> GetBookingsByMechanicAsync(int mechanicId, CancellationToken ct = default)
        {
            return await _db.ServiceBookings
                .Include(b => b.ServiceJob)
                .Where(b => b.ServiceJob != null && b.ServiceJob.MechanicId == mechanicId)
                .OrderByDescending(b => b.CreatedAtUtc)
                .ToListAsync(ct);
        }

        public async Task<bool> ReassignBookingToMechanicAsync(int bookingId, int mechanicId, CancellationToken ct = default)
        {
            try
            {
                var booking = await _db.ServiceBookings
                    .Include(b => b.ServiceJob)
                    .FirstOrDefaultAsync(b => b.Id == bookingId, ct);

                if (booking?.ServiceJob == null)
                {
                    return false;
                }

                // Check if the new mechanic is available
                var mechanic = await _db.Mechanics.FirstOrDefaultAsync(m => m.Id == mechanicId && m.IsAvailable, ct);
                if (mechanic == null)
                {
                    return false;
                }

                // Check for time conflicts with the new mechanic
                var existingJobs = await _db.ServiceJobs
                    .Where(j => j.MechanicId == mechanicId && 
                               j.ScheduledDate == booking.ServiceJob.ScheduledDate &&
                               j.Id != booking.ServiceJob.Id)
                    .ToListAsync(ct);

                var newJobStart = booking.ServiceJob.ScheduledStart;
                var newJobEnd = newJobStart.Add(TimeSpan.FromMinutes(booking.ServiceJob.DurationMin));

                var hasConflict = existingJobs.Any(job =>
                {
                    var jobStart = job.ScheduledStart;
                    var jobEnd = jobStart.Add(TimeSpan.FromMinutes(job.DurationMin));
                    return !(newJobEnd <= jobStart || newJobStart >= jobEnd);
                });

                if (hasConflict)
                {
                    return false; // Mechanic is not available at this time
                }

                // Reassign the job
                booking.ServiceJob.MechanicId = mechanicId;
                await _db.SaveChangesAsync(ct);

                _logger.LogInformation("Booking {BookingId} reassigned to mechanic {MechanicId}", bookingId, mechanicId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error reassigning booking {BookingId} to mechanic {MechanicId}", bookingId, mechanicId);
                return false;
            }
        }

        private async Task CreateAutomaticPickupRequestAsync(ServiceBooking booking, CreateBookingRequest request)
        {
            try
            {
                _logger.LogInformation("Creating automatic pickup request for booking {BookingId}", booking.Id);

                var pickupRequest = new CreatePickupRequest
                {
                    ServiceBookingId = booking.Id,
                    CustomerId = booking.CustomerId ?? "",
                    CustomerName = booking.CustomerName,
                    CustomerPhone = booking.CustomerPhone ?? "",
                    PickupLocation = request.PickupAddress ?? "",
                    PickupDate = booking.PreferredDate.ToDateTime(TimeOnly.MinValue),
                    PickupTime = booking.PreferredStart.ToTimeSpan(),
                    VehicleMake = booking.Make,
                    VehicleModel = booking.Model,
                    VehicleYear = booking.Year,
                    VehicleLicensePlate = null, // Can be added later
                    SpecialInstructions = booking.Notes
                };

                await _pickupService.CreatePickupRequestAsync(pickupRequest);
                
                _logger.LogInformation("Successfully created pickup request for booking {BookingId}", booking.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating automatic pickup request for booking {BookingId}", booking.Id);
                throw;
            }
        }
        
    }
}