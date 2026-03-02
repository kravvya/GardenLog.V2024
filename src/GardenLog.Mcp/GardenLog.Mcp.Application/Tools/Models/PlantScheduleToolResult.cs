namespace GardenLog.Mcp.Application.Tools.Models;

/// <summary>
/// Plant schedule with planned dates for a specific plant harvest cycle.
/// For actual dates from worklogs, use get_worklog_history tool separately.
/// </summary>
public class PlantScheduleToolResult
{
    // Identity
    public string PlantHarvestCycleId { get; set; } = string.Empty;
    public string PlantId { get; set; } = string.Empty;
    public string PlantName { get; set; } = string.Empty;
    public string PlantVarietyId { get; set; } = string.Empty;
    public string PlantVarietyName { get; set; } = string.Empty;
    public string HarvestCycleId { get; set; } = string.Empty;
    public string? Notes { get; set; }
    
    // Planned schedule (from calendar)
    public IReadOnlyCollection<ScheduledTask> PlannedTasks { get; set; } = Array.Empty<ScheduledTask>();
    
    // Garden bed placements (reusing existing model)
    public IReadOnlyCollection<GardenBedPlacement> Beds { get; set; } = Array.Empty<GardenBedPlacement>();
}

/// <summary>
/// Scheduled task from plant calendar.
/// </summary>
public class ScheduledTask
{
    public string TaskType { get; set; } = string.Empty;  // SowIndoors, SowOutside, Transplant, Harvest
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public bool IsSystemGenerated { get; set; }
}
