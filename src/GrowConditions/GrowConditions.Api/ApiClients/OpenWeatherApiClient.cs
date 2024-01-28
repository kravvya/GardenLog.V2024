

namespace GrowConditions.Api.ApiClients;

public interface IOpenWeatherApiClient
{
    Task<WeatherUpdate?> GetWeatherUpdate(GardenViewModel garden);
}

public class OpenWeatherApiClient : IOpenWeatherApiClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<OpenWeatherApiClient> _logger;
    private readonly IMapper _mapper;
    private readonly IConfigurationService _configurationService;
   
    public OpenWeatherApiClient(HttpClient httpClient, IConfiguration confguration, ILogger<OpenWeatherApiClient> logger, IMapper mapper, IConfigurationService configurationService)
    {
        _httpClient = httpClient;
        _logger = logger;
        _mapper = mapper;
        _configurationService = configurationService;

        var openWeatherUrl = confguration["Services:OpenWeather.Api"];

        if (openWeatherUrl == null)
        {
            _logger.LogCritical("Did not get open weather api. This is a show stopper.");
            throw new ArgumentNullException(nameof(confguration));
        }

        _logger.LogInformation("Open Weather URL @ {openWeatherUrl}", openWeatherUrl);
        _httpClient.BaseAddress = new Uri(openWeatherUrl);
    }

    public async Task<WeatherUpdate?> GetWeatherUpdate(GardenViewModel garden)
    {
        string appId = _configurationService.GetOpenWeartherApplicationId();

        HttpResponseMessage response = await _httpClient.GetAsync($"?APPID={appId}&lat={garden.Latitude}&lon={garden.Longitude}&mode=json&units=imperial");
        response.EnsureSuccessStatusCode();

        var jsonString = await response.Content.ReadAsStringAsync();
        //continue to use Newtonsoft for OpenWeather service. 
        var openWeather = Newtonsoft.Json.JsonConvert.DeserializeObject<OpenWeather>(jsonString);

        if(openWeather == null || openWeather.Sys == null || openWeather.Main == null)
        {
            _logger.LogCritical("Did not get Open Weather update message");
            return null;
        }

        _logger.LogInformation("Fetch weather got weather: {temp}F", openWeather.Main.Temp);

        WeatherUpdate weather = _mapper.Map<WeatherUpdate>(openWeather);

        weather.Id = Guid.NewGuid().ToString();
        weather.UpdatedDateLocal = openWeather.Dt.AddSeconds(openWeather.SecondsFromUtc);
        weather.UpdatedDateUtc = openWeather.Dt;
        weather.Daylight = new()
        {
            SunriseLocal = openWeather.Sys.Sunrise.AddSeconds(openWeather.SecondsFromUtc),
            SunsetLocal = openWeather.Sys.Sunset.AddSeconds(openWeather.SecondsFromUtc),
            SunriseUtc = openWeather.Sys.Sunrise,
            SunsetUtc = openWeather.Sys.Sunset
        };
        weather.UserProfileId = garden.UserProfileId;
        weather.GardenId = garden.GardenId;
        weather.GardenName = garden.Name;
        weather.Location = new()
        {
            CityId = openWeather.CityId,
            CityName = openWeather.CityName,
            Coord = openWeather.Coord,
            Country = openWeather.Sys.Country
        };


        return weather;
    }
}
