namespace PlantHarvest.Contract.ViewModels;

public record PlantScheduleViewModel :PlantScheduleBase
{
    public string PlantScheduleId { get; set; } = string.Empty;
}

public class PlantScheduleViewModelValidator : PlantScheduleValidator<PlantScheduleViewModel>
{
    public PlantScheduleViewModelValidator()
    {
    }
}