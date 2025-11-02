using AutoEdge.Models.Entities;

namespace AutoEdge.Models.ViewModels;

public class MechanicsBoardVm
{
    public List<Mechanic> Mechanics { get; init; } = new();
    public Dictionary<int, List<ServiceJob>> JobsByMechanic { get; init; } = new();
}