namespace GrowConditions.Contract;

public static class WeatherRoutes
{
    public const string WeatherBase = "/v1/api/weather";

    public const string GetLastWeatherUpdate = WeatherBase + "/garden/{gardenId}/last";
    public const string GetHistoryOfWeatherUpdates = WeatherBase + "/garden/{gardenId}/history/{numberOfDays}";
    public const string Run = WeatherBase + "/run";
}
