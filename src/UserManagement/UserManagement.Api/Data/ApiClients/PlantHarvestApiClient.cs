using GardenLog.SharedInfrastructure.Extensions;
using PlantHarvest.Contract;
using PlantHarvest.Contract.Query;

namespace UserManagement.Api.Data.ApiClients;

public interface IPlantHarvestApiClient
{
    Task<string?> GetTasks(string userProfileId, bool pastDueOnly);
}

public class PlantHarvestApiClient : IPlantHarvestApiClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<PlantHarvestApiClient> _logger;


    public PlantHarvestApiClient(HttpClient httpClient, IConfiguration confguration, ILogger<PlantHarvestApiClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;

        var harvestUrl = confguration["Services:PlantHarvest.Api"];

        if (harvestUrl == null)
        {
            _logger.LogCritical("Unable to get PlantHarvest Api");
            throw new ArgumentNullException(nameof(confguration), "Unable to get PlantHarvest Api");
        }
        _logger.LogInformation("PlantHarvest URL @ {harvestUrl}", harvestUrl);

        _httpClient.BaseAddress = new Uri(harvestUrl);
    }

    public async Task<string?> GetTasks(string userProfileId, bool pastDueOnly)
    {

        string route = HarvestRoutes.SearchTasks.Replace("{format}", "html");

        var search = new PlantTaskSearch();

        if (pastDueOnly) search.IsPastDue = pastDueOnly;
        else search.DueInNumberOfDays = 6;

        using var requestContent = search.ToJsonStringContent();

        var headers = new List<KeyValuePair<string, string>>() { new("RequestUser", userProfileId) };

        var response = await _httpClient.ApiPostAsync(route, search, headers);

        if (!response.IsSuccess)
        {
            _logger.LogError("Unable to get Tasks for userProfileId: {userProfileId} ", userProfileId);
            return null;
        }

        var content = response.Response;

        return content;
    }
}