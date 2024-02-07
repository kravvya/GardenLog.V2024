using GrowConditions.Contract.Base;
using GrowConditions.Contract.ViewModels;

namespace GrowConditions.Api.Data.ApiClients;

public interface INationalWeatherServiceApiClient
{
    Task<WeatherForecastViewModel?> GetWeatherForecast(WeatherstationViewModel weatherStation);
    Task<WeatherstationViewModel?> GetWeatherStation(decimal latitude, decimal longitude);
}

public class NationalWeatherServiceApiClient : INationalWeatherServiceApiClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<OpenWeatherApiClient> _logger;
    private readonly IMapper _mapper;
    private readonly IConfigurationService _configurationService;

    public NationalWeatherServiceApiClient(HttpClient httpClient, IConfiguration confguration, ILogger<OpenWeatherApiClient> logger, IMapper mapper, IConfigurationService configurationService)
    {
        _httpClient = httpClient;
        _logger = logger;
        _mapper = mapper;
        _configurationService = configurationService;

        var nationalWeatherServiceUrl = confguration["NationalWeatherService.Api"];

        if (nationalWeatherServiceUrl == null)
        {
            _logger.LogCritical("Did not get national weather api. This is a show stopper.");
            throw new ArgumentNullException(nameof(confguration));
        }

        _logger.LogInformation("National Weather Service URL @ {nationalWeatherServiceUrl}", nationalWeatherServiceUrl);
        _httpClient.BaseAddress = new Uri(nationalWeatherServiceUrl);
        _httpClient.DefaultRequestHeaders.Add("User-Agent", "(under development website");
    }

    public async Task<WeatherForecastViewModel?> GetWeatherForecast(WeatherstationViewModel weatherStation)
    {
        string appId = _configurationService.GetOpenWeartherApplicationId();

        HttpResponseMessage response = await _httpClient.GetAsync($"gridpoints/{weatherStation.ForecastOffice}/{weatherStation.GridX},{weatherStation.GridY}/forecast");
        response.EnsureSuccessStatusCode();

        var jsonString = await response.Content.ReadAsStringAsync();
        //continue to use Newtonsoft for OpenWeather service. 
        var nationalForecast = Newtonsoft.Json.JsonConvert.DeserializeObject<NationalWeatherForecast>(jsonString);

        if (nationalForecast == null || nationalForecast.Properties == null)
        {
            _logger.LogCritical("Did not get Forecast");
            return null;
        }

        _logger.LogInformation("Fetch weather got weather: {temp}F", nationalForecast.Properties.Periods[0].Temperature);

        WeatherForecastViewModel weather = new();

        foreach (var period in nationalForecast.Properties.Periods)
        {
            weather.DailyForecasts.Add(new DailyForecast()
            {
                IsDaytime = period.IsDaytime,
                Name = period.Name,
                Sequence = period.Number,
                Conditions = new WeatherCondition()
                {
                    Description = period.DetailedForecast,
                    Icon = period.Icon,
                    Main = period.ShortForecast
                },
                Forecast = new WeatherForecast()
                {
                    Temp = period.Temperature,
                    Humidity = period.RelativeHumidity.Value,
                    ChanceOfPrecipitation = period.ProbabilityOfPrecipitation.Value,
                },
            });
        }

        return weather;
    }

    public async Task<WeatherstationViewModel?> GetWeatherStation(decimal latitude, decimal longitude)
    {
        HttpResponseMessage response = await _httpClient.GetAsync($"/points?{latitude},{longitude}");
        response.EnsureSuccessStatusCode();

        var jsonString = await response.Content.ReadAsStringAsync();
        //continue to use Newtonsoft for OpenWeather service. 
        var pointData = Newtonsoft.Json.JsonConvert.DeserializeObject<NationalWeatherPointData>(jsonString);

        if (pointData == null || pointData.Properties == null)
        {
            _logger.LogCritical("Did not get National Weather Center point data message");
            return null;
        }

        _logger.LogInformation("Fetch weatherstation: {offide}", pointData.Properties.ForecastOffice);

        WeatherstationViewModel weatherStation = new();
        weatherStation.WeatherstationId = pointData.Id;
        weatherStation.ForecastOffice = pointData.Properties.ForecastOffice;
        weatherStation.GridX = pointData.Properties.GridX;
        weatherStation.GridY = pointData.Properties.GridY;
        weatherStation.Timezone = pointData.Properties.TimeZone;

        return weatherStation;
    }
}


