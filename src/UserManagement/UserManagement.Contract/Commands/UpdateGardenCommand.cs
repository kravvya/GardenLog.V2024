namespace UserManagement.Contract.Command;

public record UpdateGardenCommand: GardenBase
{
    public string GardenId { get; init; } = string.Empty;
}

public class UpdateGardenCommandValidator : GardenValidator<UpdateGardenCommand>
{
    public UpdateGardenCommandValidator()
    {
        RuleFor(command => command.GardenId).NotEmpty().Length(3, 50);
    }
}
