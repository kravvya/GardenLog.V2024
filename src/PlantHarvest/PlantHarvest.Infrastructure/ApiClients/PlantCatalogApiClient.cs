using Microsoft.Extensions.Caching.Memory;
using System.Collections.ObjectModel;

namespace PlantHarvest.Infrastructure.ApiClients;

public interface IPlantCatalogApiClient
{
    Task<PlantViewModel?> GetPlant(string plantId);
    Task<PlantGrowInstructionViewModel?> GetPlantGrowInstruction(string plantId, string growInstructionId);
    Task<PlantVarietyViewModel?> GetPlantVariety(string plantId, string plantVarietyId);
}

public class PlantCatalogApiClient : IPlantCatalogApiClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<PlantCatalogApiClient> _logger;
    private readonly IMemoryCache _cache;

    private const string GROW_CACHE_KEY = "Plant:{0}Grow:{1}";
    private const string PLANT_CACHE_KEY = "Plant:{0}";
    private const int CACHE_DURATION = 60;

    public PlantCatalogApiClient(HttpClient httpClient, IConfiguration confguration, ILogger<PlantCatalogApiClient> logger, IMemoryCache cache)
    {
        _httpClient = httpClient;
        _logger = logger;
        _cache = cache;
       var plantUrl = confguration["Services:PlantCatalog.Api"];

        if(plantUrl == null)
        {
            _logger.LogCritical("Unable to get PlantCatalog Api");
            throw new ArgumentNullException("Unable to get PlantCatalog Api", nameof(confguration));
        }
        _logger.LogInformation($"Plant URL @ {plantUrl}");

        _httpClient.BaseAddress = new Uri(plantUrl);
    }

    public async Task<PlantGrowInstructionViewModel?> GetPlantGrowInstruction(string plantId, string growInstructionId)
    {
        string key = string.Format(GROW_CACHE_KEY, plantId, growInstructionId);

        if (!_cache.TryGetValue(key, out PlantGrowInstructionViewModel? growInstruction))
        {
            string route = Routes.GetPlantGrowInstruction.Replace("{plantId}", plantId).Replace("{id}", growInstructionId);

            var response = await _httpClient.ApiGetAsync<PlantGrowInstructionViewModel>(route);

            if (!response.IsSuccess)
            {
                _logger.LogError($"Unable to get Plant Grow Instruction for plantId: {plantId} and growId {growInstructionId}");
                return null;
            }

            growInstruction = response.Response;
            _cache.Set(key, growInstruction, new MemoryCacheEntryOptions()
            {
                SlidingExpiration = TimeSpan.FromMinutes(CACHE_DURATION)
            });
        }

        return growInstruction;
    }

    public async Task<PlantVarietyViewModel?> GetPlantVariety(string plantId, string plantVarietyId)
    {
        PlantVarietyViewModel? _variety;

        string route = Routes.GetPlantVariety.Replace("{plantId}", plantId).Replace("{id}", plantVarietyId);

        var response = await _httpClient.ApiGetAsync<PlantVarietyViewModel>(route);

        if (!response.IsSuccess)
        {
            _logger.LogError($"Unable to get Plant Grow Instruction for plantId: {plantId} and growId {plantVarietyId}");
            return null;
        }

        _variety = response.Response;

        return _variety;
    }

    public async Task<PlantViewModel?> GetPlant(string plantId)
    {
        string key = string.Format(PLANT_CACHE_KEY, plantId);

        if (!_cache.TryGetValue(key, out PlantViewModel? _plant))
        {
            string route = Routes.GetPlantById.Replace("{id}", plantId);

            var response = await _httpClient.ApiGetAsync<PlantViewModel>(route);

            if (!response.IsSuccess)
            {
                _logger.LogError($"Unable to get Plant : {plantId}");
                return null;
            }

            _plant = response.Response;
            _cache.Set(key, _plant, new MemoryCacheEntryOptions()
            {
                SlidingExpiration = TimeSpan.FromMinutes(CACHE_DURATION)
            });
        }

        return _plant;
    }
}
