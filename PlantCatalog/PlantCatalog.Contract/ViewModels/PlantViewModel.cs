using PlantCatalog.Contract.Validators;

namespace PlantCatalog.Contract.ViewModels;

public record PlantViewModel: PlantBase
{
    public string PlantId { get; set; } = string.Empty;
    public int VarietyCount { get; set; }
    public int GrowInstructionsCount { get; set; }
}

public class PlantViewModelValidator : PlantValidator<PlantViewModel>
{
    public PlantViewModelValidator()
    {
    }
}