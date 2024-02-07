namespace UserManagement.Contract.Command;


public record CreateWeatherstationCommand: WeatherstationBase
{
    public required string GardenId { get; set; }
}


public class CreateWeatherStationCommandValidator : WeatherStationValidator<CreateWeatherstationCommand>
{
    public CreateWeatherStationCommandValidator()
    {

    }
}
