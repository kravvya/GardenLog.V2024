using GardenLog.SharedInfrastructure.Extensions;
using GrowConditions.Contract;

namespace UserManagement.Api.Data.ApiClients;

public interface IGrowConditionsApiClient
{
    Task<WeatherstationViewModel?> GetWeatherStation(decimal latitude, decimal longitude);
}

public class GrowConditionsApiClient : IGrowConditionsApiClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<PlantHarvestApiClient> _logger;


    public GrowConditionsApiClient(HttpClient httpClient, IConfiguration confguration, ILogger<PlantHarvestApiClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;

        var growConditionsUrl = confguration["Services:GrowConditions.Api"];

        if (growConditionsUrl == null)
        {
            _logger.LogCritical("Unable to get Grow Conditions Api");
            throw new ArgumentNullException(nameof(confguration), "Unable to get Grow Conditions Api");
        }
        _logger.LogInformation("Grow Conditions URL @ {growConditionsUrl}", growConditionsUrl);

        _httpClient.BaseAddress = new Uri(growConditionsUrl);
    }

    public async Task<WeatherstationViewModel?> GetWeatherStation(decimal latitude, decimal longitude)
    {
        string route = WeatherRoutes.GetWeatherStationByCoordinaes.Replace("{lat}", latitude.ToString()).Replace("{lon}", longitude.ToString());
               
        var response = await _httpClient.ApiGetAsync<WeatherstationViewModel>(route);

        if (!response.IsSuccess)
        {
            _logger.LogError("Unable to get weather station: {lat}, {lon} ", latitude, longitude);
            return null;
        }

        var content = response.Response;

        return content;
    }
}