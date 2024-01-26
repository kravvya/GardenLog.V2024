namespace PlantHarvest.Contract.Commands;


#region Harvest Cycle
public record CreateHarvestCycleCommand : HarvestCycleBase
{ }

public record UpdateHarvestCycleCommand : HarvestCycleBase
{
    public string HarvestCycleId { get; init; } = string.Empty;
}

public class CreateHarvestCycleCommandValidator : HarvestCycleValidator<CreateHarvestCycleCommand>
{
    public CreateHarvestCycleCommandValidator()
    {
    }
}

public class UpdateHarvestCycleCommandValidator : HarvestCycleValidator<UpdateHarvestCycleCommand>
{
    public UpdateHarvestCycleCommandValidator()
    {
        RuleFor(command => command.HarvestCycleId).NotEmpty().Length(3, 50);
    }
}
#endregion

#region Plant Harvest Cycle

public record CreatePlantHarvestCycleCommand : PlantHarvestCycleBase
{ }

public record UpdatePlantHarvestCycleCommand : PlantHarvestCycleBase
{
    public string PlantHarvestCycleId { get; init; } = string.Empty;
}

public class CreatePlantHarvestCycleCommandValidator : PlantHarvestCycleValidator<CreatePlantHarvestCycleCommand>
{
    public CreatePlantHarvestCycleCommandValidator()
    {
    }
}

public class UpdatePlantHarvestCycleCommandValidator : PlantHarvestCycleValidator<UpdatePlantHarvestCycleCommand>
{
    public UpdatePlantHarvestCycleCommandValidator()
    {
        RuleFor(command => command.PlantHarvestCycleId).NotEmpty().Length(3, 50);
    }
}
#endregion

#region Plant Schedule
public record CreatePlantScheduleCommand : PlantScheduleBase
{

}

public class CreatePlantScheduleCommandValidator : PlantScheduleValidator<CreatePlantScheduleCommand>
{
    public CreatePlantScheduleCommandValidator()
    {
    }
}

public record UpdatePlantScheduleCommand : PlantScheduleBase
{
    public string PlantScheduleId { get; init; } = string.Empty;
}

public class UpdatePlantScheduleCommandValidator : PlantScheduleValidator<UpdatePlantScheduleCommand>
{
    public UpdatePlantScheduleCommandValidator()
    {
        RuleFor(command => command.PlantScheduleId).NotEmpty().Length(3, 50);
    }
}
#endregion

#region Garden Layout
public record CreateGardenBedPlantHarvestCycleCommand : GardenBedPlantHarvestCycleBase
{

}

public class CreateGardenBedPlantHarvestCycleCommandValidator : GardenBedPlantHarvestCycleValidator<CreateGardenBedPlantHarvestCycleCommand>
{
    public CreateGardenBedPlantHarvestCycleCommandValidator()
    {
    }
}

public record UpdateGardenBedPlantHarvestCycleCommand : GardenBedPlantHarvestCycleBase
{
    public string GardenBedPlantHarvestCycleId { get; init; } = string.Empty;
}

public class UpdateGardenBedPlantHarvestCycleCommandValidator : GardenBedPlantHarvestCycleValidator<UpdateGardenBedPlantHarvestCycleCommand>
{
    public UpdateGardenBedPlantHarvestCycleCommandValidator()
    {
        RuleFor(command => command.GardenBedPlantHarvestCycleId).NotEmpty().Length(3, 50);
    }
}
#endregion