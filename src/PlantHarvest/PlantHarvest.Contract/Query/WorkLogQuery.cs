namespace PlantHarvest.Contract.Query;

public record WorkLogSearch()
{
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public WorkLogReasonEnum? Reason { get; set; }
    public int? Limit { get; set; }
}