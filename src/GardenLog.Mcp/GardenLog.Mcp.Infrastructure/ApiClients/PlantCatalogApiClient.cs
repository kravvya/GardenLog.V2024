using Microsoft.Extensions.Caching.Memory;
using PlantCatalog.Contract.ViewModels;

namespace GardenLog.Mcp.Infrastructure.ApiClients;

public record PlantCatalogPlantDetails
{
    public PlantViewModel Plant { get; init; } = new();
    public IReadOnlyCollection<PlantGrowInstructionViewModel> GrowInstructions { get; init; } = Array.Empty<PlantGrowInstructionViewModel>();
}

public interface IPlantCatalogApiClient
{
    Task<PlantCatalogPlantDetails?> GetPlantDetails(string plantId);
    Task<string?> GetPlantIdByName(string plantName);
    Task<IReadOnlyCollection<PlantNameOnlyViewModel>> GetAllPlantNames();
}

public class PlantCatalogApiClient : IPlantCatalogApiClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<PlantCatalogApiClient> _logger;
    private readonly IMemoryCache _cache;

    private const string PLANT_DETAILS_CACHE_KEY = "Plant:Details:{0}";
    private const int CACHE_DURATION = 60;

    public PlantCatalogApiClient(HttpClient httpClient, IConfiguration configuration, ILogger<PlantCatalogApiClient> logger, IMemoryCache cache)
    {
        _httpClient = httpClient;
        _logger = logger;
        _cache = cache;
        var plantUrl = configuration["Services:PlantCatalog.Api"];

        if (plantUrl == null)
        {
            _logger.LogCritical("Unable to get PlantCatalog Api");
            throw new ArgumentNullException("Unable to get PlantCatalog Api", nameof(configuration));
        }
        _logger.LogInformation("Plant URL @ {plantUrl}", plantUrl);

        _httpClient.BaseAddress = new Uri(plantUrl);
    }

    public async Task<PlantCatalogPlantDetails?> GetPlantDetails(string plantId)
    {
        string key = string.Format(PLANT_DETAILS_CACHE_KEY, plantId);

        if (!_cache.TryGetValue(key, out PlantCatalogPlantDetails? details))
        {
            string plantRoute = Routes.GetPlantById.Replace("{id}", plantId);
            string growInstructionsRoute = Routes.GetPlantGrowInstructions.Replace("{plantId}", plantId);

            var plantTask = _httpClient.ApiGetAsync<PlantViewModel>(plantRoute);
            var growInstructionsTask = _httpClient.ApiGetAsync<List<PlantGrowInstructionViewModel>>(growInstructionsRoute);

            await Task.WhenAll(plantTask, growInstructionsTask);

            var plantResponse = await plantTask;
            var growInstructionsResponse = await growInstructionsTask;

            if (!plantResponse.IsSuccess || plantResponse.Response is null)
            {
                _logger.LogError("Unable to get Plant : {plantId}", plantId);
                return null;
            }

            if (!growInstructionsResponse.IsSuccess || growInstructionsResponse.Response is null)
            {
                _logger.LogError("Unable to get Grow Instructions for plant : {plantId}", plantId);
                throw new InvalidOperationException($"No grow instructions were returned for plantId '{plantId}'.");
            }

            details = new PlantCatalogPlantDetails
            {
                Plant = plantResponse.Response,
                GrowInstructions = growInstructionsResponse.Response
            };

            _cache.Set(key, details, new MemoryCacheEntryOptions()
            {
                SlidingExpiration = TimeSpan.FromMinutes(CACHE_DURATION)
            });
        }

        return details;
    }

    public async Task<string?> GetPlantIdByName(string plantName)
    {
        if (string.IsNullOrWhiteSpace(plantName))
        {
            return null;
        }

        var route = Routes.GetIdByPlantName.Replace("{name}", Uri.EscapeDataString(plantName));
        var response = await _httpClient.ApiGetAsync<string>(route);

        if (!response.IsSuccess || string.IsNullOrWhiteSpace(response.Response))
        {
            return null;
        }

        return response.Response;
    }

    public async Task<IReadOnlyCollection<PlantNameOnlyViewModel>> GetAllPlantNames()
    {
        var response = await _httpClient.ApiGetAsync<List<PlantNameOnlyViewModel>>(Routes.GetAllPlantNames);

        if (!response.IsSuccess || response.Response == null)
        {
            _logger.LogError("Unable to get plant names");
            return Array.Empty<PlantNameOnlyViewModel>();
        }

        return response.Response;
    }
}
