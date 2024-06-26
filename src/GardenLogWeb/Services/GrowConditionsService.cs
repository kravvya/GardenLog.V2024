using GardenLogWeb.Models.Forecast;
using GrowConditions.Contract;
using System.Net.Http;
using System.Reflection;

namespace GardenLogWeb.Services;

public interface IGrowConditionsService
{
    Task<WeatherForecastModel?> GetWeatherForecast(string gardenId);
}

public class GrowConditionsService : IGrowConditionsService
{
    private const string KEY = "Forecast";
    private readonly ILogger<GrowConditionsService> _logger;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ICacheService _cacheService;
    private readonly IGardenLogToastService _toastService;
    private readonly int _cacheDuration;

    public GrowConditionsService(ILogger<GrowConditionsService> logger, IHttpClientFactory clientFactory, ICacheService cacheService, IGardenLogToastService toastService, IConfiguration configuration)
    {
        _logger = logger;
        _httpClientFactory = clientFactory;
        _cacheService = cacheService;
        _toastService = toastService;
        if (!int.TryParse(configuration[GlobalConstants.GLOBAL_CACHE_DURATION], out _cacheDuration)) _cacheDuration = 60;
    }

    public async Task<WeatherForecastModel?> GetWeatherForecast(string gardenId)
    {
        if (!_cacheService.TryGetValue<WeatherForecastModel>(KEY, out WeatherForecastModel? forecast))
        {
            _logger.LogInformation("Weather forecast not in cache");

            forecast = await GetNewWeatherForecast(gardenId);

            if (forecast != null)
            {
                // Save data in cache.
                _cacheService.Set(KEY, forecast, DateTime.Now.AddMinutes(_cacheDuration));
            }
        }

        else
        {
            _logger.LogInformation($"Weather forecast found in cache. ");
        }

        return forecast;
    }

    #region "Private Functions"

    private async Task<WeatherForecastModel?> GetNewWeatherForecast(string gardenId)
    {
        var httpClient = _httpClientFactory.CreateClient(GlobalConstants.GROWCONDITIONS_API);

        var url = WeatherRoutes.GetForecast.Replace("{gardenId}", gardenId);

        var response = await httpClient.ApiGetAsync<WeatherForecastModel>(url, _logger);

        if (!response.IsSuccess)
        {
            _toastService.ShowToast("Unable to get Weather Forecast", GardenLogToastLevel.Error);
            return null;
        }

        return response.Response!;
    }

    #endregion
}

