using GardenLog.SharedKernel;

namespace PlantHarvest.Contract.Base;

public abstract record WorkLogBase()
{
    public string Log { get; set; } = string.Empty;
    public List<RelatedEntity> RelatedEntities { get; set; } = new();
    public DateTime EnteredDateTime { get; set; }
    public DateTime EventDateTime { get; set; }
    public WorkLogReasonEnum Reason { get; set; }
}
