namespace GardenLog.Mcp.Application.Tools.Models;

/// <summary>
/// Plant schedules with planned dates. Returns schedules grouped by grow instruction.
/// For plants with multiple grow instructions (e.g., Radishes in spring and fall), all schedules are returned.
/// For plants with multiple varieties using the same grow instruction (e.g., 10 Pepper varieties all "Start Indoors"), one schedule is returned.
/// AI should filter by current date to select the appropriate schedule.
/// For actual dates from worklogs, use get_worklog_history tool separately.
/// </summary>
public class PlantScheduleToolResult
{
    public string PlantId { get; set; } = string.Empty;
    public string PlantName { get; set; } = string.Empty;
    public string HarvestCycleId { get; set; } = string.Empty;
    
    // All schedules for this plant (one per grow instruction + variety combo)
    public IReadOnlyCollection<PlantScheduleGroup> Schedules { get; set; } = Array.Empty<PlantScheduleGroup>();
}

/// <summary>
/// A specific schedule for a plant with a particular grow instruction.
/// </summary>
public class PlantScheduleGroup
{
    public string PlantHarvestCycleId { get; set; } = string.Empty;
    public string? PlantGrowthInstructionId { get; set; }
    public string? PlantGrowthInstructionName { get; set; }
    public string? Notes { get; set; }
    
    // Planned schedule (from calendar)
    public IReadOnlyCollection<ScheduledTask> PlannedTasks { get; set; } = Array.Empty<ScheduledTask>();
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
