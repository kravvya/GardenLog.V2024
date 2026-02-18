using System.ComponentModel;
using GardenLog.Mcp.Infrastructure.ApiClients;
using ModelContextProtocol.Server;
using PlantHarvest.Contract.ViewModels;

namespace GardenLog.Mcp.Application.Tools;

[McpServerToolType]
public class SearchHarvestCyclesTool
{
    private readonly IPlantHarvestApiClient _plantHarvestApiClient;
    private readonly ILogger<SearchHarvestCyclesTool> _logger;

    public SearchHarvestCyclesTool(
        IPlantHarvestApiClient plantHarvestApiClient,
        ILogger<SearchHarvestCyclesTool> logger)
    {
        _plantHarvestApiClient = plantHarvestApiClient;
        _logger = logger;
    }

    [McpServerTool(Name = "search_harvest_cycles", UseStructuredContent = true)]
    [Description("Search harvest cycles using optional year, garden, name, and date filters.")]
    public async Task<IReadOnlyCollection<HarvestCycleViewModel>> ExecuteAsync(
        [Description("Optional harvest cycle name text filter")] string? harvestCycleName = null,
        [Description("Optional garden ID filter")] string? gardenId = null,
        [Description("Optional year filter applied to StartDate")] int? year = null,
        [Description("Optional start date filter (inclusive)")] DateTime? startDate = null,
        [Description("Optional end date filter (inclusive)")] DateTime? endDate = null,
        [Description("Maximum number of records to return (default 100, max 500)")] int limit = 100,
        CancellationToken cancellationToken = default)
    {
        if (startDate.HasValue && endDate.HasValue && startDate > endDate)
        {
            throw new ArgumentException("startDate must be less than or equal to endDate.");
        }

        if (year is < 1900 or > 3000)
        {
            throw new ArgumentException("year must be between 1900 and 3000.");
        }

        int boundedLimit = limit <= 0 ? 100 : Math.Min(limit, 500);

        _logger.LogInformation(
            "search_harvest_cycles called: name={HarvestCycleName}, gardenId={GardenId}, year={Year}, start={StartDate}, end={EndDate}, limit={Limit}",
            harvestCycleName,
            gardenId,
            year,
            startDate,
            endDate,
            boundedLimit);

        var harvestCycles = await _plantHarvestApiClient.GetHarvestCycles();

        var filtered = harvestCycles
            .Where(h => string.IsNullOrWhiteSpace(gardenId) || string.Equals(h.GardenId, gardenId, StringComparison.OrdinalIgnoreCase))
            .Where(h => string.IsNullOrWhiteSpace(harvestCycleName) || h.HarvestCycleName.Contains(harvestCycleName, StringComparison.OrdinalIgnoreCase))
            .Where(h => !year.HasValue || h.StartDate.Year == year.Value)
            .Where(h => !startDate.HasValue || h.StartDate >= startDate.Value)
            .Where(h => !endDate.HasValue || (h.EndDate ?? h.StartDate) <= endDate.Value)
            .OrderByDescending(h => h.StartDate)
            .Take(boundedLimit)
            .ToList();

        return filtered;
    }
}