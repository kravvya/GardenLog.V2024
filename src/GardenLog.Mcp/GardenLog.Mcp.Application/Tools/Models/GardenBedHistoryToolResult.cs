namespace GardenLog.Mcp.Application.Tools.Models;

/// <summary>
/// Simplified garden bed history record optimized for AI crop rotation analysis.
/// Excludes layout details (x, y, pattern) and internal IDs that aren't useful for planning.
/// </summary>
public class GardenBedHistoryToolResult
{
    /// <summary>
    /// Harvest cycle ID - useful for linking back to cycle details
    /// </summary>
    public string HarvestCycleId { get; set; } = string.Empty;
    
    /// <summary>
    /// Plant ID - needed for follow-up queries to get plant details
    /// </summary>
    public string PlantId { get; set; } = string.Empty;
    
    /// <summary>
    /// Plant name for readability
    /// </summary>
    public string PlantName { get; set; } = string.Empty;
    
    /// <summary>
    /// When plant was added to this bed
    /// </summary>
    public DateTime? StartDate { get; set; }
    
    /// <summary>
    /// When plant was removed from this bed
    /// </summary>
    public DateTime? EndDate { get; set; }
    
    /// <summary>
    /// How many plants were in this bed
    /// </summary>
    public int NumberOfPlants { get; set; }
}
