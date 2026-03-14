namespace PlantHarvest.Contract.Query;

public record PlantHarvestCycleSearch()
{
    public string? PlantId { get; set; }
    public string? HarvestCycleId { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public int? MinGerminationRate { get; set; }
    public int? Limit { get; set; }
}

public record PlantHarvestCycleSummarySearch()
{
    public string? PlantId { get; set; }
    public string? PlantName { get; set; }
    public string? HarvestCycleId { get; set; }
    public string? HarvestCycleName { get; set; }
}