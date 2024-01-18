using PlantCatalog.Contract.Commands;

namespace PlantCatalog.Contract.Validators;

//https://docs.fluentvalidation.net/en/latest/aspnet.html
public class PlantValidator<T> : AbstractValidator<T>
    where T : PlantBase
{
    public PlantValidator()
    {
        RuleFor(command => command.Name).NotEmpty().Length(3, 50);
        RuleFor(command => command.Description).NotEmpty().MaximumLength(1000);
        RuleFor(command => command.Color).NotEmpty().MaximumLength(50);
        RuleFor(command => command.GardenTip).NotEmpty().MaximumLength(1000);
        RuleFor(command => command.Type).NotEmpty().WithMessage("Plant type has to be selected");
    }
}

#region Plant Grow Instruction Validators
public class PlantGrowInstructionValidator<T> : AbstractValidator<T>
    where T : PlantGrowInstructionBase
{
    public PlantGrowInstructionValidator()
    {
        RuleFor(command => command.Name).NotEmpty().Length(3, 50);
        RuleFor(command => command.PlantId).NotEmpty().Length(2, 50);
        //RuleFor(command => command.PlantingDepthInInches).NotEmpty().IsEnumName(typeof(PlantingDepthEnum));
        RuleFor(command => command.PlantingMethod).NotEmpty();
        //RuleFor(command => command.StartSeedAheadOfWeatherCondition).NotEmpty().IsEnumName(typeof(WeatherConditionEnum));
        RuleFor(command => command.HarvestSeason).NotEmpty();
        //RuleFor(command => command.TransplantAheadOfWeatherCondition).NotEmpty().IsEnumName(typeof(WeatherConditionEnum));
        //RuleFor(command => command.StartSeedInstructions).NotNull().MaximumLength(1000);
        RuleFor(command => command.GrowingInstructions).NotNull().MaximumLength(1000);
        RuleFor(command => command.HarvestInstructions).NotNull().MaximumLength(1000);

    }
}

public class CreatePlantGrowInstructionCommandValidator : PlantGrowInstructionValidator<CreatePlantGrowInstructionCommand>
{
    public CreatePlantGrowInstructionCommandValidator()
    {
    }
}

public class UpdatePlantGrowInstructionCommandValidator : PlantGrowInstructionValidator<UpdatePlantGrowInstructionCommand>
{
    public UpdatePlantGrowInstructionCommandValidator()
    {
        RuleFor(command => command.PlantGrowInstructionId).NotEmpty().Length(3, 50);
    }
}
#endregion

#region Plant Variety Validators
public class PlantVarietyValidator<T> : AbstractValidator<T>
    where T : PlantVarietyBase
{
    public PlantVarietyValidator()
    {
        RuleFor(command => command.Name).NotEmpty().Length(3, 50);
        RuleFor(command => command.PlantId).NotEmpty().Length(2, 50);
        RuleFor(command => command.Title).NotEmpty().Length(10, 100);
      
    }
}

public class CreatePlantVarietyCommandValidator : PlantVarietyValidator<CreatePlantVarietyCommand>
{
    public CreatePlantVarietyCommandValidator()
    {
    }
}

public class UpdatePlantVarietyCommandValidator : PlantVarietyValidator<UpdatePlantVarietyCommand>
{
    public UpdatePlantVarietyCommandValidator()
    {
        RuleFor(command => command.PlantVarietyId).NotEmpty().Length(3, 50);
    }
}
#endregion