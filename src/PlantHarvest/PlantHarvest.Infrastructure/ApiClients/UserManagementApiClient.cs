using Microsoft.Extensions.Caching.Memory;
using UserManagement.Contract;
using UserManagement.Contract.ViewModels;

namespace PlantHarvest.Infrastructure.ApiClients;

public interface IUserManagementApiClient
{
    Task<GardenViewModel?> GetGarden(string gardenId);
}

public class UserManagementApiClient : IUserManagementApiClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<UserManagementApiClient> _logger;
    private readonly IMemoryCache _cache;

    private const string GARDEN_CACHE_KEY = "Garden:{0}";
    private const int CACHE_DURATION = 60;

    public UserManagementApiClient(HttpClient httpClient, IConfiguration confguration, ILogger<UserManagementApiClient> logger, IMemoryCache cache)
    {
        _httpClient = httpClient;
        _logger = logger;
        _cache = cache;
        var plantUrl = confguration["Services:UserManagement.Api"];

        if(plantUrl == null )
        {
            _logger.LogCritical("Unable to get User Management Api");
            throw new ArgumentException("Unable to get User Management Api", nameof(confguration));
        }
        _logger.LogInformation("User Mgmt URL @ {plantUrl}", plantUrl);

        _httpClient.BaseAddress = new Uri(plantUrl);
    }

    public async Task<GardenViewModel?> GetGarden(string gardenId)
    {
        string key = string.Format(GARDEN_CACHE_KEY, gardenId);

        if (!_cache.TryGetValue(key, out GardenViewModel? garden))
        {
            string route = GardenRoutes.GetGarden.Replace("{gardenId}", gardenId);

            var response = await _httpClient.ApiGetAsync<GardenViewModel>(route);

            if (!response.IsSuccess)
            {
                _logger.LogError("Unable to get Garden deatil for gardenId: {gardenId}", gardenId);
                return null;
            }

            garden = response.Response;
            _cache.Set(key, garden, new MemoryCacheEntryOptions()
            {
                SlidingExpiration = TimeSpan.FromMinutes(CACHE_DURATION)
            });
        }
        return garden;
    }

}
