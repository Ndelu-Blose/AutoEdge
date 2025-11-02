using AutoEdge.Models.Entities;

namespace AutoEdge.Models.ViewModels;

public record MechanicKpis(int TodayCount, int WeekCount, int CompletedMonth, int InProgressNow);

public class MechanicsDashboardVm
{
    public Mechanic Mechanic { get; init; } = null!;
    public MechanicKpis Kpis { get; init; } = null!;
    public List<ServiceJob> Today { get; init; } = new();
    public List<ServiceJob> Upcoming { get; init; } = new();
} 