namespace UserManagement.Contract.Command;

public record UpdateGardenBedCommand: GardenBedBase
{
    public string GardenBedId { get; init; } = string.Empty;
}

public class UpdateGardenBedCommandValidator : GardenBedValidator<UpdateGardenBedCommand>
{
    public UpdateGardenBedCommandValidator()
    {
        RuleFor(command => command.GardenBedId).NotEmpty().Length(3, 50);
    }
}
