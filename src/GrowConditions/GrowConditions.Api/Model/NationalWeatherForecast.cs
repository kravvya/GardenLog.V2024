namespace GrowConditions.Api.Model;


public record NationalWeatherForecast
{
    public required ForecastProperties Properties { get; set; }
}


public record ForecastProperties
{
    public required string Units { get; set; }
    public DateTime UpdateTime { get; set; }
    public required List<Period> Periods { get; set; }
}

public record Period
{
    public int Number { get; set; }
    public required string Name { get; set; }
    public bool IsDaytime { get; set; }
    public int Temperature { get; set; }
    public required string TemperatureUnit { get; set; }
    public required string TemperatureTrend { get; set; }
    public required ProbabilityOfPrecipitation ProbabilityOfPrecipitation { get; set; }
    public required Dewpoint Dewpoint { get; set; }
    public required RelativeHumidity RelativeHumidity { get; set; }
    public required string WindSpeed { get; set; }
    public required string WindDirection { get; set; }
    public required string Icon { get; set; }
    public required string ShortForecast { get; set; }
    public required string DetailedForecast { get; set; }
}



public record Dewpoint
{
    public required string UnitCode { get; set; }
    public double Value { get; set; }
}

public record ProbabilityOfPrecipitation
{
    public required string UnitCode { get; set; }
    public int Value { get; set; }
}

public record RelativeHumidity
{
    public required string UnitCode { get; set; }
    public int Value { get; set; }
}


