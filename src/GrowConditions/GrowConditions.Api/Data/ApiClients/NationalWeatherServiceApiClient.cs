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

    public NationalWeatherServiceApiClient(HttpClient httpClient, IConfiguration confguration, ILogger<OpenWeatherApiClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;

        var nationalWeatherServiceUrl = confguration["Services:NationalWeatherService.Api"];

        if (nationalWeatherServiceUrl == null)
        {
            _logger.LogCritical("Did not get national weather api. This is a show stopper.");
            throw new ArgumentNullException(nameof(confguration));
        }

        _logger.LogInformation("National Weather Service URL @ {nationalWeatherServiceUrl}", nationalWeatherServiceUrl);
        _httpClient.BaseAddress = new Uri(nationalWeatherServiceUrl);
        _httpClient.DefaultRequestHeaders.Add("User-Agent", "slavgl.com");
    }

    public async Task<WeatherForecastViewModel?> GetWeatherForecast(WeatherstationViewModel weatherStation)
    {
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
                Wind = new WindForecast()
                {
                    Speed = period.WindSpeed,
                    WindDirection = period.WindDirection
                }
            });
        }

        return weather;
    }

    public async Task<WeatherstationViewModel?> GetWeatherStation(decimal latitude, decimal longitude)
    {


        var url = $"/points/{latitude},{longitude}";
        HttpResponseMessage response = await _httpClient.GetAsync(url);
        
        if (response.IsSuccessStatusCode == false)
        {
            _logger.LogCritical("Did not get National Weather Center point data message");
            return null;
        }

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
        weatherStation.ForecastOffice = pointData.Properties.GridId;
        weatherStation.GridX = pointData.Properties.GridX;
        weatherStation.GridY = pointData.Properties.GridY;
        weatherStation.Timezone = pointData.Properties.TimeZone;

        return weatherStation;

    }
}


