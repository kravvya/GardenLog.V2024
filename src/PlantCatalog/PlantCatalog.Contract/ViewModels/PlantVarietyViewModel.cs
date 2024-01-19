using PlantCatalog.Contract.Validators;

namespace PlantCatalog.Contract.ViewModels;

public record PlantVarietyViewModel: PlantVarietyBase
{
    public string PlantVarietyId { get; set; } = string.Empty;
}

public class PlantVarietyViewModelValidator : PlantVarietyValidator<PlantVarietyViewModel>
{
    public PlantVarietyViewModelValidator()
    {
    }
}