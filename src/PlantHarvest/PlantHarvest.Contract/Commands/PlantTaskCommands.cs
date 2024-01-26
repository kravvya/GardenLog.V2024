namespace PlantHarvest.Contract.Commands;

#region Plant Task
public record CreatePlantTaskCommand : PlantTaskBase
{ }

public record UpdatePlantTaskCommand : PlantTaskBase
{
    public string PlantTaskId { get; init; } = string.Empty;
}

public class CreatePlantTaskCommandValidator : PlantTaskValidator<CreatePlantTaskCommand>
{
    public CreatePlantTaskCommandValidator()
    {
    }
}

public class UpdatePlantTaskCommandValidator : PlantTaskValidator<UpdatePlantTaskCommand>
{
    public UpdatePlantTaskCommandValidator()
    {
        RuleFor(command => command.PlantTaskId).NotEmpty().Length(3, 50);
    }
}
#endregion