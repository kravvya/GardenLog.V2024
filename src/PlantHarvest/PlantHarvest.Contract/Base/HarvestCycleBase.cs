namespace PlantHarvest.Contract.Base;

public abstract record HarvestCycleBase
{
    public string HarvestCycleName { get; set; } = string.Empty;
    public string Notes { get; set; }=string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string UserProfileId { get; init; } = string.Empty;
    public string GardenId { get; set; } = string.Empty;
}
