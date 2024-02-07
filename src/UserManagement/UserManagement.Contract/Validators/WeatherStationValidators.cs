namespace UserManagement.Contract.Validators;

public class WeatherStationValidator<T> :AbstractValidator<T>
    where T : WeatherstationBase
{
    public WeatherStationValidator()
    {
        RuleFor(command => command.ForecastOffice).NotEmpty().Length(3, 50);
        RuleFor(command => command.GridX).NotEmpty().NotEqual(0);
        RuleFor(command => command.GridY).NotEmpty().NotEqual(0);
        RuleFor(command => command.Timezone).NotEmpty().Length(10, 100);
    }
}
