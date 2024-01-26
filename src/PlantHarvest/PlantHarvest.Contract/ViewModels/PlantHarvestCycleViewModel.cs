namespace PlantHarvest.Contract.ViewModels;

public record PlantHarvestCycleViewModel:PlantHarvestCycleBase
{
    public string PlantHarvestCycleId { get; set; }=string.Empty;

    public List<PlantScheduleViewModel> PlantCalendar { get; set; } = new();
    public List<GardenBedPlantHarvestCycleViewModel> GardenBedLayout { get; set; } = new();
}

public class PlantHarvestCycleViewModelValidator : PlantHarvestCycleValidator<PlantHarvestCycleViewModel>
{
    public PlantHarvestCycleViewModelValidator()
    {
    }
}