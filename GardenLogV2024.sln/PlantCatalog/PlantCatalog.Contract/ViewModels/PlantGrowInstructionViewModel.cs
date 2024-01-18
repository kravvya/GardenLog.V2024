using PlantCatalog.Contract.Validators;

namespace PlantCatalog.Contract.ViewModels;

public record PlantGrowInstructionViewModel: PlantGrowInstructionBase
{
    public string PlantGrowInstructionId { get; set; } = string.Empty;
}

public class PlantGrowInstructionViewModelValidator : PlantGrowInstructionValidator<PlantGrowInstructionViewModel>
{
    public PlantGrowInstructionViewModelValidator()
    {
    }
}