namespace GrowConditions.Contract.Base;

public abstract record WeatherForecastBase
{
    public List<DailyForecast> DailyForecasts { get; set; } = [];
}


public record DailyForecast
{
    public int Sequence { get; set; }
    public required string Name { get; set; }
    public bool IsDaytime { get; set; }   
    public WeatherCondition? Conditions { get; set; }
    public WeatherForecast? Forecast { get; set; }
    public WindForecast? Wind { get; set; }
}

public record WindForecast
{
    public required string Speed { get; set; }
    public required string WindDirection { get; set; }
}

public record WeatherForecast
{
    public decimal Temp { get; set; }
    public decimal? Humidity { get; set; }
    public int? ChanceOfPrecipitation { get; set; }
}

