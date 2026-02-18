using UserManagement.Contract.ViewModels;

namespace GardenLog.Mcp.Application.Tools.Models;

public record GardenDetailsToolResult
{
    public GardenViewModel Garden { get; init; } = new();
    public IReadOnlyCollection<GardenBedViewModel> GardenBeds { get; init; } = Array.Empty<GardenBedViewModel>();
}