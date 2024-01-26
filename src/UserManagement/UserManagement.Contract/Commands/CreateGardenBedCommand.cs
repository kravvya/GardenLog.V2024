namespace UserManagement.Contract.Command;


public record CreateGardenBedCommand: GardenBedBase
{
}


public class CreateGardenBedCommandValidator : GardenBedValidator<CreateGardenBedCommand>
{
    public CreateGardenBedCommandValidator()
    {
    }
}
