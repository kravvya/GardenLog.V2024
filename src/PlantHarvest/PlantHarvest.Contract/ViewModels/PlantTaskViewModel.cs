namespace PlantHarvest.Contract.ViewModels;

public record PlantTaskViewModel: PlantTaskBase
{
    public string PlantTaskId { get; set; } = string.Empty;
}


public class PlantTaskViewModelValidator : PlantTaskValidator<PlantTaskViewModel>
{
    public PlantTaskViewModelValidator()
    {
    }
}