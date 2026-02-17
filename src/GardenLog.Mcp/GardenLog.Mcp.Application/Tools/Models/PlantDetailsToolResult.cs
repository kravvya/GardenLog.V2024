using PlantCatalog.Contract.ViewModels;

namespace GardenLog.Mcp.Application.Tools.Models;

public record PlantDetailsToolResult
{
    public PlantViewModel Plant { get; init; } = new();
    public IReadOnlyCollection<PlantGrowInstructionViewModel> GrowInstructions { get; init; } = Array.Empty<PlantGrowInstructionViewModel>();
}