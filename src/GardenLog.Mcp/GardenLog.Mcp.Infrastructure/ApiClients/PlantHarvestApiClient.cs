using PlantHarvest.Contract.Query;
using PlantHarvest.Contract.ViewModels;

namespace GardenLog.Mcp.Infrastructure.ApiClients;

public interface IPlantHarvestApiClient
{
    Task<IReadOnlyCollection<WorkLogViewModel>> SearchWorkLogs(WorkLogSearch search);
    Task<IReadOnlyCollection<PlantHarvestCycleViewModel>> SearchPlantHarvestCycles(PlantHarvestCycleSearch search);
    Task<IReadOnlyCollection<HarvestCycleViewModel>> GetHarvestCycles();
    Task<IReadOnlyCollection<GardenBedPlantHarvestCycleViewModel>> GetGardenBedUsageHistory(string gardenId, string gardenBedId);
}

public class PlantHarvestApiClient : IPlantHarvestApiClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<PlantHarvestApiClient> _logger;

    public PlantHarvestApiClient(HttpClient httpClient, IConfiguration configuration, ILogger<PlantHarvestApiClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;

        var plantHarvestUrl = configuration["Services:PlantHarvest.Api"];

        if (string.IsNullOrWhiteSpace(plantHarvestUrl))
        {
            _logger.LogCritical("Unable to get PlantHarvest Api");
            throw new ArgumentNullException("Unable to get PlantHarvest Api", nameof(configuration));
        }

        _httpClient.BaseAddress = new Uri(plantHarvestUrl);
    }

    public async Task<IReadOnlyCollection<WorkLogViewModel>> SearchWorkLogs(WorkLogSearch search)
    {
        var queryParams = new List<string>();

        if (search.StartDate.HasValue)
        {
            queryParams.Add($"startDate={Uri.EscapeDataString(search.StartDate.Value.ToString("o"))}");
        }

        if (search.EndDate.HasValue)
        {
            queryParams.Add($"endDate={Uri.EscapeDataString(search.EndDate.Value.ToString("o"))}");
        }

        if (search.Reason.HasValue)
        {
            queryParams.Add($"reason={Uri.EscapeDataString(search.Reason.Value.ToString())}");
        }

        if (search.Limit.HasValue)
        {
            queryParams.Add($"limit={search.Limit.Value}");
        }

        var route = HarvestRoutes.SearchWorkLogs;
        if (queryParams.Count > 0)
        {
            route = $"{route}?{string.Join("&", queryParams)}";
        }

        var response = await _httpClient.ApiGetAsync<List<WorkLogViewModel>>(route);
        if (!response.IsSuccess || response.Response == null)
        {
            _logger.LogError("Unable to search WorkLogs");
            return Array.Empty<WorkLogViewModel>();
        }

        return response.Response;
    }

    public async Task<IReadOnlyCollection<PlantHarvestCycleViewModel>> SearchPlantHarvestCycles(PlantHarvestCycleSearch search)
    {
        var queryParams = new List<string>();

        if (!string.IsNullOrWhiteSpace(search.PlantId))
        {
            queryParams.Add($"plantId={Uri.EscapeDataString(search.PlantId)}");
        }

        if (!string.IsNullOrWhiteSpace(search.HarvestCycleId))
        {
            queryParams.Add($"harvestCycleId={Uri.EscapeDataString(search.HarvestCycleId)}");
        }

        if (search.StartDate.HasValue)
        {
            queryParams.Add($"startDate={Uri.EscapeDataString(search.StartDate.Value.ToString("o"))}");
        }

        if (search.EndDate.HasValue)
        {
            queryParams.Add($"endDate={Uri.EscapeDataString(search.EndDate.Value.ToString("o"))}");
        }

        if (search.MinGerminationRate.HasValue)
        {
            queryParams.Add($"minGerminationRate={search.MinGerminationRate.Value}");
        }

        if (search.Limit.HasValue)
        {
            queryParams.Add($"limit={search.Limit.Value}");
        }

        var route = HarvestRoutes.SearchPlantHarvestCycles;
        if (queryParams.Count > 0)
        {
            route = $"{route}?{string.Join("&", queryParams)}";
        }

        var response = await _httpClient.ApiGetAsync<List<PlantHarvestCycleViewModel>>(route);
        if (!response.IsSuccess || response.Response == null)
        {
            _logger.LogError("Unable to search PlantHarvestCycles");
            return Array.Empty<PlantHarvestCycleViewModel>();
        }

        return response.Response;
    }

    public async Task<IReadOnlyCollection<HarvestCycleViewModel>> GetHarvestCycles()
    {
        var response = await _httpClient.ApiGetAsync<List<HarvestCycleViewModel>>(HarvestRoutes.GetAllHarvestCycles);
        if (!response.IsSuccess || response.Response == null)
        {
            _logger.LogError("Unable to get HarvestCycles");
            return Array.Empty<HarvestCycleViewModel>();
        }

        return response.Response;
    }

    public async Task<IReadOnlyCollection<GardenBedPlantHarvestCycleViewModel>> GetGardenBedUsageHistory(string gardenId, string gardenBedId)
    {
        var route = HarvestRoutes.GetGardenBedUsageHistory
            .Replace("{gardenId}", Uri.EscapeDataString(gardenId))
            .Replace("{gardenBedId}", Uri.EscapeDataString(gardenBedId));

        var response = await _httpClient.ApiGetAsync<List<GardenBedPlantHarvestCycleViewModel>>(route);
        if (!response.IsSuccess || response.Response == null)
        {
            _logger.LogError("Unable to get garden bed usage history for garden {GardenId} and bed {GardenBedId}", gardenId, gardenBedId);
            return Array.Empty<GardenBedPlantHarvestCycleViewModel>();
        }

        return response.Response;
    }
}