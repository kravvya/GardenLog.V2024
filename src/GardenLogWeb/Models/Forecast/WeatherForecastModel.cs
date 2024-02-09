using GrowConditions.Contract.ViewModels;

namespace GardenLogWeb.Models.Forecast;

public record WeatherForecastModel : WeatherForecastViewModel
{
}


public record WeatherForecastDayModel
{
    public string Name { get; set; } = string.Empty;
    public WeatherForecastSummaryModel? Day { get; set; }
    public WeatherForecastSummaryModel? Night { get; set; }
    public required int Sequence { get; set; }
    public required string Class { get; set; } = string.Empty;
}

public record WeatherForecastSummaryModel
{
    public required string Main { get; set; }
    public required string Description { get; set; }
    public required decimal Temperature { get; set; }
    public decimal? Humidity { get; set; }
    public required string WindSpeed { get; set; }
    public int? ChanceOfRain { get; set; }
    public int? ChanceOfSnow { get; set; }
    public required string Icon { get; set; }

}