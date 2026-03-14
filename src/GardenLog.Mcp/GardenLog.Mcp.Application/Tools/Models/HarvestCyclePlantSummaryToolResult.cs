namespace GardenLog.Mcp.Application.Tools.Models;

/// <summary>
/// Simplified harvest cycle plant summary - just the plant list with optional variety and bed details.
/// Use this to answer "What am I growing?" - excludes schedules, instructions, and layout coordinates.
/// </summary>
public class HarvestCyclePlantSummaryToolResult
{
    public string PlantId { get; set; } = string.Empty;
    public string PlantName { get; set; } = string.Empty;
    public IReadOnlyCollection<PlantVarietySummary> Varieties { get; set; } = Array.Empty<PlantVarietySummary>();
}

/// <summary>
/// Plant variety with grow instructions and optional garden bed placement details.
/// </summary>
public class PlantVarietySummary
{
    public string PlantHarvestCycleId { get; set; } = string.Empty;
    public string PlantVarietyId { get; set; } = string.Empty;
    public string PlantVarietyName { get; set; } = string.Empty;
    public string? PlantGrowthInstructionId { get; set; }
    public string? PlantGrowthInstructionName { get; set; }
    public IReadOnlyCollection<GardenBedPlacement> Beds { get; set; } = Array.Empty<GardenBedPlacement>();
}

/// <summary>
/// Garden bed placement info - which bed and how many plants.
/// </summary>
public class GardenBedPlacement
{
    public string GardenBedId { get; set; } = string.Empty;
    public string GardenBedName { get; set; } = string.Empty;
    public int NumberOfPlants { get; set; }
}
