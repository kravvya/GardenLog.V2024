namespace GardenLog.Mcp.Application.Tools.Models;

using PlantHarvest.Contract.ViewModels;

public class HarvestCyclePlantToolResult
{
    public string PlantId { get; set; } = string.Empty;
    public string PlantName { get; set; } = string.Empty;
    public string? PlantGrowthInstructionId { get; set; }
    public string? PlantGrowthInstructionName { get; set; }
    public int PlantHarvestCycleCount { get; set; }
    public int VarietyCount { get; set; }
    public IReadOnlyCollection<string> VarietyNames { get; set; } = Array.Empty<string>();
    public IReadOnlyCollection<PlantScheduleViewModel> Schedules { get; set; } = Array.Empty<PlantScheduleViewModel>();
    public IReadOnlyCollection<PlantHarvestCycleViewModel> PlantVarieties { get; set; } = Array.Empty<PlantHarvestCycleViewModel>();
}
