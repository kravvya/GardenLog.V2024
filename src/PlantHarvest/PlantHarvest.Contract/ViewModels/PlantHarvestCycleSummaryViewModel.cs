namespace PlantHarvest.Contract.ViewModels;

public record PlantHarvestCycleSummaryViewModel
{
    public string PlantId { get; set; } = string.Empty;
    public string PlantName { get; set; } = string.Empty;
    public string? PlantGrowthInstructionId { get; set; }
    public string? PlantGrowthInstructionName { get; set; }
    public string HarvestCycleId { get; set; } = string.Empty;
    public string HarvestCycleName { get; set; } = string.Empty;
    public DateTime? EarliestSeedingDate { get; set; }
    public DateTime? EarliestTransplantDate { get; set; }
    public DateTime? EarliestHarvestDate { get; set; }
    public DateTime? LatestHarvestDate { get; set; }
    public List<string> FeedbackNotes { get; set; } = new();
}