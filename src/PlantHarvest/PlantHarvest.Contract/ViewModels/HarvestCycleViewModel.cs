namespace PlantHarvest.Contract.ViewModels;

public record HarvestCycleViewModel:HarvestCycleBase
{
    public string HarvestCycleId { get; set; }=string.Empty;
}

public class HarvestCycleViewModelValidator : HarvestCycleValidator<HarvestCycleViewModel>
{
    public HarvestCycleViewModelValidator()
    {
    }
}