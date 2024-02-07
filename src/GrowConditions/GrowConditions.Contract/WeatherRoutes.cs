namespace GrowConditions.Contract;

public static class WeatherRoutes
{
    public const string WeatherBase = "/v1/api/weather";

    public const string GetLastWeatherUpdate = WeatherBase + "/garden/{gardenId}/last";
    public const string GetHistoryOfWeatherUpdates = WeatherBase + "/garden/{gardenId}/history/{numberOfDays}";
    public const string Run = WeatherBase + "/run";

    public const string GetForecast = WeatherBase + "/garden/{gardenId}/forecast";
    public const string GetWeatherStation= WeatherBase + "/garden/{gardenId}/station/";
    public const string GetWeatherStationByCoordinaes = WeatherBase + "/station?lat={lat}&lon={lon}";
}
