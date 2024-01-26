namespace PlantHarvest.Contract.Validators;

public class PlantTaskValidator<T> : AbstractValidator<T>
    where T : PlantTaskBase
{
    public PlantTaskValidator()
    {
        RuleFor(command => command.HarvestCycleId).NotEmpty().Length(3, 100);
        RuleFor(command => command.Title).NotEmpty().Length(3, 1000);
        RuleFor(command => command.PlantHarvestCycleId).NotEmpty().Length(3, 100); ;
        RuleFor(command => command.PlantName).NotEmpty().Length(3, 100); ;
        RuleFor(command => command.CreatedDateTime).NotEmpty();
        RuleFor(command => command.Type).Must(r => r != WorkLogReasonEnum.Unspecified).WithMessage("Please select type of work");
        RuleFor(command => command.TargetDateEnd).GreaterThanOrEqualTo(command => command.TargetDateStart);
    }
}
