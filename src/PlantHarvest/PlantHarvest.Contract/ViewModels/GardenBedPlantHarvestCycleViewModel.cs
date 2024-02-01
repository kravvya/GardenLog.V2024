namespace PlantHarvest.Contract.ViewModels;

public record GardenBedPlantHarvestCycleViewModel :GardenBedPlantHarvestCycleBase
{
    public string GardenBedPlantHarvestCycleId { get; set; } = string.Empty;
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
}

public class GardenBedPlantHarvestCycleViewModelValidator : GardenBedPlantHarvestCycleValidator<GardenBedPlantHarvestCycleViewModel>
{
    public GardenBedPlantHarvestCycleViewModelValidator()
    {
    }
}