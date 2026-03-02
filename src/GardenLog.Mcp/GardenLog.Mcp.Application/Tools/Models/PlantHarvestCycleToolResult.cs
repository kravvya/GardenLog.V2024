namespace GardenLog.Mcp.Application.Tools.Models;

/// <summary>
/// Simplified plant harvest cycle result - excludes layout coordinates and dimensions.
/// Use for historical analysis and searching past growing seasons.
/// </summary>
public class PlantHarvestCycleToolResult
{
    public string PlantHarvestCycleId { get; set; } = string.Empty;
    public string HarvestCycleId { get; set; } = string.Empty;
    public string PlantId { get; set; } = string.Empty;
    public string PlantName { get; set; } = string.Empty;
    public string PlantVarietyId { get; set; } = string.Empty;
    public string PlantVarietyName { get; set; } = string.Empty;
    public string? PlantGrowthInstructionId { get; set; }
    public string? PlantGrowthInstructionName { get; set; }
    public string? Notes { get; set; }
    public int? GerminationRate { get; set; }
    
    // Calendar schedule
    public IReadOnlyCollection<ScheduledTask> PlantCalendar { get; set; } = Array.Empty<ScheduledTask>();
    
    // Garden bed placements (simplified - just bed ID, name, and plant count)
    public IReadOnlyCollection<GardenBedPlacement> GardenBeds { get; set; } = Array.Empty<GardenBedPlacement>();
}
