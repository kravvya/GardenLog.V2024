namespace PlantHarvest.Contract.Base;

public abstract record PlantScheduleBase
{
    public string HarvestCycleId { get; set; } = string.Empty;
    public string PlantHarvestCycleId { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public WorkLogReasonEnum TaskType { get; set; }
    public string Notes { get; set; } = string.Empty;
    public bool IsSystemGenerated { get; set; }
    
}
