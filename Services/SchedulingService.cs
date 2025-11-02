using AutoEdge.Data;
using AutoEdge.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace AutoEdge.Services
{
    public class SchedulingService : ISchedulingService
    {
        private readonly ApplicationDbContext _db;

        public SchedulingService(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<ServiceJob> ScheduleJobAsync(int bookingId, DateOnly date, TimeOnly start, int? mechanicId, CancellationToken ct = default)
        {
            var booking = await _db.ServiceBookings.FirstOrDefaultAsync(b => b.Id == bookingId, ct)
                ?? throw new InvalidOperationException("Booking not found");

            // pick mechanic if not provided: first available
            Mechanic? mech = null;
            if (mechanicId.HasValue)
            {
                mech = await _db.Mechanics.FirstOrDefaultAsync(m => m.Id == mechanicId.Value, ct);
            }
            else
            {
                mech = await _db.Mechanics.Where(m => m.IsAvailable)
                    .OrderByDescending(m => m.Rating)
                    .FirstOrDefaultAsync(ct);
            }

            var job = new ServiceJob
            {
                ServiceBookingId = booking.Id,
                MechanicId = mech?.Id,
                ScheduledDate = date,
                ScheduledStart = start,
                DurationMin = booking.EstimatedDurationMin,
                IsCompleted = false
            };

            _db.ServiceJobs.Add(job);
            booking.Status = ServiceBookingStatus.Confirmed;

            await _db.SaveChangesAsync(ct);
            return job;
        }
    }
}