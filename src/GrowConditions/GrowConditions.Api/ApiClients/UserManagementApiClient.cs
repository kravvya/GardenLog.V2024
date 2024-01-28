using Microsoft.Extensions.Caching.Memory;
using UserManagement.Contract;

namespace GrowConditions.Api.ApiClients;

public interface IUserManagementApiClient
{
    Task<List<GardenViewModel>?> GetAllGardens();
}

public class UserManagementApiClient : IUserManagementApiClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<UserManagementApiClient> _logger;
    private readonly IMemoryCache _cache;

    private const string GARDEN_CACHE_KEY = "Gardens";
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

}
