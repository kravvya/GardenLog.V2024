namespace PlantHarvest.Contract.Commands;

#region Work Log
public record CreateWorkLogCommand : WorkLogBase
{ }

public record UpdateWorkLogCommand : WorkLogBase
{
    public string WorkLogId { get; init; } = string.Empty;
}

public class CreateWorkLogCommandValidator : WorkLogValidator<CreateWorkLogCommand>
{
    public CreateWorkLogCommandValidator()
    {
    }
}

public class UpdateWorkLogCommandValidator : WorkLogValidator<UpdateWorkLogCommand>
{
    public UpdateWorkLogCommandValidator()
    {
        RuleFor(command => command.WorkLogId).NotEmpty().Length(3, 50);
    }
}
#endregion