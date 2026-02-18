namespace GardenLog.Mcp.Application.Tools.Models;

using PlantHarvest.Contract.ViewModels;

public class HarvestCyclePlantToolResult
{
    public string PlantId { get; set; } = string.Empty;
    public string PlantName { get; set; } = string.Empty;
    public int PlantHarvestCycleCount { get; set; }
    public int VarietyCount { get; set; }
    public IReadOnlyCollection<string> VarietyNames { get; set; } = Array.Empty<string>();
    public IReadOnlyCollection<PlantHarvestCycleViewModel> PlantHarvestCycles { get; set; } = Array.Empty<PlantHarvestCycleViewModel>();
}
