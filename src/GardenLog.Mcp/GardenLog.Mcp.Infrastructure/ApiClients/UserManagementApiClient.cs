using UserManagement.Contract;
using UserManagement.Contract.ViewModels;

namespace GardenLog.Mcp.Infrastructure.ApiClients;

public interface IUserManagementApiClient
{
    Task<GardenViewModel?> GetGarden(string gardenId);
    Task<GardenViewModel?> GetGardenByName(string gardenName);
    Task<IReadOnlyCollection<GardenBedViewModel>> GetGardenBeds(string gardenId);
}

public class UserManagementApiClient : IUserManagementApiClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<UserManagementApiClient> _logger;

    public UserManagementApiClient(HttpClient httpClient, IConfiguration configuration, ILogger<UserManagementApiClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;

        var userManagementUrl = configuration["Services:UserManagement.Api"];

        if (string.IsNullOrWhiteSpace(userManagementUrl))
        {
            _logger.LogCritical("Unable to get UserManagement Api");
            throw new ArgumentNullException("Unable to get UserManagement Api", nameof(configuration));
        }

        _httpClient.BaseAddress = new Uri(userManagementUrl);
    }

    public async Task<GardenViewModel?> GetGarden(string gardenId)
    {
        var route = GardenRoutes.GetGarden.Replace("{gardenId}", Uri.EscapeDataString(gardenId));

        var response = await _httpClient.ApiGetAsync<GardenViewModel>(route);
        if (!response.IsSuccess || response.Response == null)
        {
            _logger.LogError("Unable to get garden {GardenId}", gardenId);
            return null;
        }

        return response.Response;
    }

    public async Task<GardenViewModel?> GetGardenByName(string gardenName)
    {
        var route = GardenRoutes.GetGardenByName.Replace("{gardenName}", Uri.EscapeDataString(gardenName));

        var response = await _httpClient.ApiGetAsync<GardenViewModel>(route);
        if (!response.IsSuccess || response.Response == null)
        {
            _logger.LogError("Unable to get garden by name {GardenName}", gardenName);
            return null;
        }

        return response.Response;
    }

    public async Task<IReadOnlyCollection<GardenBedViewModel>> GetGardenBeds(string gardenId)
    {
        var route = GardenRoutes.GetGardenBeds.Replace("{gardenId}", Uri.EscapeDataString(gardenId));

        var response = await _httpClient.ApiGetAsync<List<GardenBedViewModel>>(route);
        if (!response.IsSuccess || response.Response == null)
        {
            _logger.LogError("Unable to get garden beds for garden {GardenId}", gardenId);
            return Array.Empty<GardenBedViewModel>();
        }

        return response.Response;
    }
}