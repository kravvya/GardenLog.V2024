using FluentValidation;
using UserManagement.Contract.Base;

namespace UserManagement.Contract.Validators;

public class GardenValidator<T> :AbstractValidator<T>
    where T : GardenBase
{
    public GardenValidator()
    {
        RuleFor(command => command.Name).NotEmpty().Length(3, 50);
        RuleFor(command => command.City).NotEmpty().Length(3, 100);
        RuleFor(command => command.StateCode).NotEmpty().MaximumLength(2);
        RuleFor(command => command.Latitude).NotEmpty().NotEqual(0);
        RuleFor(command => command.Longitude).NotEmpty().NotEqual(0);
        RuleFor(command => command.Notes).NotEmpty().Length(3, 1000);
    }
}


public class GardenBedValidator<T> : AbstractValidator<T>
    where T : GardenBedBase
{
    public GardenBedValidator()
    {
        RuleFor(command => command.GardenId).NotEmpty().Length(3, 50);
        RuleFor(command => command.Name).NotEmpty().Length(3, 50);
        RuleFor(command => command.Length).NotEmpty().NotEqual(0);
        RuleFor(command => command.Width).NotEmpty().NotEqual(0);
        //RuleFor(command => command.X).NotEmpty();
        //RuleFor(command => command.Y).NotEmpty();
        RuleFor(command => command.Notes).NotEmpty().Length(3, 1000);
    }
}
