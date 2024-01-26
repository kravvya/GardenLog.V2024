namespace PlantHarvest.Contract.Query;

public record  PlantTaskSearch()
{
    public string PlantHarvestCycleId { get; set; } = string.Empty;
    public WorkLogReasonEnum? Reason { get; set; }
    public bool IncludeResolvedTasks { get; set; } = false;
    public int? DueInNumberOfDays { get; set; }
    public bool IsPastDue { get; set; } = false;
}
