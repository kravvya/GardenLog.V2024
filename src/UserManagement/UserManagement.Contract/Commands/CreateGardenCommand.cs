namespace UserManagement.Contract.Command;


public record CreateGardenCommand: GardenBase
{
}


public class CreateGardenCommandValidator : GardenValidator<CreateGardenCommand>
{
    public CreateGardenCommandValidator()
    {
    }
}
