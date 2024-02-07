using Microsoft.Extensions.Caching.Memory;
using UserManagement.Contract;
using UserManagement.Contract.Command;

namespace GrowConditions.Api.Data.ApiClients;

public interface IUserManagementApiClient
{
    Task<List<GardenViewModel>?> GetAllGardens();
    Task<GardenViewModel?> GetGarden(string gardenId);
    Task<WeatherstationViewModel?> GetWeatherstation(string gardenId);
    Task SetWeatherstation(string gardenId, WeatherstationViewModel weatherstation);
}

public class UserManagementApiClient : IUserManagementApiClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<UserManagementApiClient> _logger;
    private readonly IMemoryCache _cache;

    private const string GARDEN_CACHE_KEY = "Gardens";
    private const string WEATHERSTATION_CACHE_KEY = "WeatherStation-{0}";
    private const int CACHE_DURATION = 1440;

    public UserManagementApiClient(HttpClient httpClient, IConfiguration confguration, ILogger<UserManagementApiClient> logger, IMemoryCache cache)
    {
        _httpClient = httpClient;
        _logger = logger;
        _cache = cache;
        var userManagementUrl = confguration["Services:UserManagement.Api"];

        if (userManagementUrl == null)
        {
            _logger.LogCritical("Did not get user management api. This is a show stopper.");
            throw new ArgumentNullException("Services:UserManagement.Api");
        }
        _logger.LogInformation($"User Mgmt URL @ {userManagementUrl}");

        _httpClient.BaseAddress = new Uri(userManagementUrl);
    }

    public async Task<List<GardenViewModel>?> GetAllGardens()
    {

        if (!_cache.TryGetValue(GARDEN_CACHE_KEY, out List<GardenViewModel>? gardens))
        {
            var response = await _httpClient.ApiGetAsync<List<GardenViewModel>>(GardenRoutes.GetAllGardens);

            if (!response.IsSuccess)
            {
                _logger.LogError($"Unable to get Gardens");
                return null;
            }

            gardens = response.Response;
            _cache.Set(GARDEN_CACHE_KEY, gardens, new MemoryCacheEntryOptions()
            {
                SlidingExpiration = TimeSpan.FromMinutes(CACHE_DURATION)
            });
        }
        return gardens;
    }

    public async Task<WeatherstationViewModel?> GetWeatherstation(string gardenId)
    {
        string cacheKey = string.Format(WEATHERSTATION_CACHE_KEY, gardenId);

        if (!_cache.TryGetValue(cacheKey, out WeatherstationViewModel? weatherstation))
        {
            var response = await _httpClient.ApiGetAsync<List<WeatherstationViewModel>>(GardenRoutes.GetWeatherstation.Replace("{gardenId}", gardenId));

            if (!response.IsSuccess)
            {
                _logger.LogError($"Weatherstation is not found. Will try to go and get it based on coordinates");
                return null;
            }

            weatherstation = response.Response!.FirstOrDefault();

            _cache.Set(cacheKey, weatherstation, new MemoryCacheEntryOptions()
            {
                SlidingExpiration = TimeSpan.FromMinutes(CACHE_DURATION)
            });
        }
        return weatherstation;
    }

    public async Task<GardenViewModel?> GetGarden(string gardenId)
    {
        var response = await _httpClient.ApiGetAsync<GardenViewModel>(GardenRoutes.GetGarden.Replace("{gardenId}", gardenId));

        if (!response.IsSuccess)
        {
            _logger.LogError("Unable to get Garden {gardenId}", gardenId);
            return null;
        }

        return response.Response;
    }

    public async Task SetWeatherstation(string gardenId, WeatherstationViewModel weatherstation)
    {

        CreateWeatherstationCommand command = new()
        {
            GardenId = gardenId,
            ForecastOffice = weatherstation.ForecastOffice,
            GridX = weatherstation.GridX,
            GridY = weatherstation.GridY,
            Timezone = weatherstation.Timezone

        };

        using var requestContent = command.ToJsonStringContent();

        string url = GardenRoutes.SetWeatherstation.Replace("{gardenId}", gardenId);

        var response = await _httpClient.ApiPostAsync(GardenRoutes.SetWeatherstation.Replace("{gardenId}", gardenId), requestContent);

        if (!response.IsSuccess)
        {
            _logger.LogError($"Weatherstation was not set.");
        }
    }
}
