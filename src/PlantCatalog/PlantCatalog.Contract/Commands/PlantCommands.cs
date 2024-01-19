using PlantCatalog.Contract.Validators;

namespace PlantCatalog.Contract.Commands;

#region Plant Commands
public record CreatePlantCommand : PlantBase
{ }

public record UpdatePlantCommand : PlantBase
{
    public string PlantId { get; init; } = string.Empty;
}

public class CreatePlantCommandValidator : PlantValidator<CreatePlantCommand>
{
    public CreatePlantCommandValidator()
    {
    }
}

public class UpdatePlantCommandValidator : PlantValidator<UpdatePlantCommand>
{
    public UpdatePlantCommandValidator()
    {
        RuleFor(command => command.PlantId).NotEmpty().Length(3, 50);
    }
}
#endregion

#region Grow Instruction Commands
public record CreatePlantGrowInstructionCommand : PlantGrowInstructionBase
{ }

public record UpdatePlantGrowInstructionCommand : PlantGrowInstructionBase
{
    public string PlantGrowInstructionId { get; init; } = string.Empty;
}

#endregion

#region Plant Variety Grow Instruction Commands
public record CreatePlantVarietyCommand : PlantVarietyBase
{ }

public record UpdatePlantVarietyCommand : PlantVarietyBase
{
    public string PlantVarietyId { get; init; } = string.Empty;
}

#endregion