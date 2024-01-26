namespace PlantHarvest.Contract.Base;


public record PlantTaskBase
{
    public string Title { get; set; } = string.Empty;
    public WorkLogReasonEnum Type { get; set; }
    public DateTime CreatedDateTime { get; set; }
    public DateTime TargetDateStart { get; set; }
    public DateTime TargetDateEnd { get; set; }
    public DateTime? CompletedDateTime { get; set; }
    public string HarvestCycleId { get; set; } = string.Empty;
    public string PlantHarvestCycleId { get; set; } = string.Empty;
    public string PlantName { get; set; } = string.Empty;
    public string PlantScheduleId { get; set; } = string.Empty;
    public string Notes { get; set; } = string.Empty;
    public bool IsSystemGenerated { get; set; }
}
