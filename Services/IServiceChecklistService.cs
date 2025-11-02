using AutoEdge.Models.Entities;

namespace AutoEdge.Services
{
    public interface IServiceChecklistService
    {
        Task<ServiceChecklist> CreateChecklistAsync(int serviceJobId, int mechanicId, string serviceType, CancellationToken ct = default);
        Task<ServiceChecklist?> GetChecklistAsync(int checklistId, CancellationToken ct = default);
        Task<ServiceChecklist?> GetChecklistByServiceJobAsync(int serviceJobId, CancellationToken ct = default);
        Task<List<ServiceChecklistItem>> GetChecklistItemsAsync(int checklistId, CancellationToken ct = default);
        Task<ServiceChecklistItem> AddChecklistItemAsync(int checklistId, string taskName, string? description, decimal? estimatedCost = null, int? estimatedDurationMinutes = null, CancellationToken ct = default);
        Task<bool> CompleteChecklistItemAsync(int itemId, string? notes, decimal? actualCost = null, int? actualDurationMinutes = null, CancellationToken ct = default);
        Task<bool> CompleteChecklistAsync(int checklistId, string? notes, CancellationToken ct = default);
        Task<ServicePhoto> AddPhotoAsync(int checklistId, string photoType, string filePath, string fileName, string? description, string takenBy, CancellationToken ct = default);
        Task<List<ServicePhoto>> GetPhotosAsync(int checklistId, CancellationToken ct = default);
        Task<List<ServiceChecklist>> GetChecklistsByMechanicAsync(int mechanicId, CancellationToken ct = default);
        Task<List<ServiceChecklist>> GetActiveChecklistsAsync(CancellationToken ct = default);
        Task<List<ServiceChecklist>> GetCompletedChecklistsAsync(CancellationToken ct = default);
    }

    public class CreateChecklistItemRequest
    {
        public string TaskName { get; set; } = string.Empty;
        public string? Description { get; set; }
    }

    public class CompleteItemRequest
    {
        public int ItemId { get; set; }
        public string? Notes { get; set; }
    }

    public class AddPhotoRequest
    {
        public string PhotoType { get; set; } = string.Empty; // "Before", "During", "After"
        public string FilePath { get; set; } = string.Empty;
        public string FileName { get; set; } = string.Empty;
        public string? Description { get; set; }
    }
}
