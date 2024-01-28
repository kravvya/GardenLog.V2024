using GrowConditions.Contract.Base;

namespace GrowConditions.Contract.ViewModels;

public record WeatherUpdateViewModel: WeatherUpdateBase
{
    public string WeatherId { get; set; } =string.Empty;
}
