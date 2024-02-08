using GrowConditions.Contract;

namespace GrowConditions.IntegrationTest.Clients;

public class WeatherClient
{
    private readonly Uri _baseUrl;
    private readonly HttpClient _httpClient;

    public WeatherClient(Uri baseUrl, HttpClient httpClient)
    {
        _baseUrl = baseUrl;
        _httpClient = httpClient;
    }

    public async Task<HttpResponseMessage> GetWeatherstation(string gardenId, double lat, double lon)
    {
        var url = $"{this._baseUrl.OriginalString}{WeatherRoutes.GetWeatherStation}?lat={lat}&lon={lon}";

        return await this._httpClient.GetAsync(url.Replace("{gardedId}", gardenId));
    }

    public async Task<HttpResponseMessage> GetWeatherForecast(string gardenId)
    {
        var url = $"{this._baseUrl.OriginalString}{WeatherRoutes.GetForecast}";

        return await this._httpClient.GetAsync(url.Replace("{gardedId}", gardenId));
    }
}
