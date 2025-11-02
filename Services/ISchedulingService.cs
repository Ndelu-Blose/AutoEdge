using AutoEdge.Models.Entities;

namespace AutoEdge.Services
{
    public interface ISchedulingService
    {
        Task<ServiceJob> ScheduleJobAsync(int bookingId, DateOnly date, TimeOnly start, int? mechanicId, CancellationToken ct = default);
    }
}