using System.ComponentModel;
using GardenLog.Mcp.Application.Services;
using GardenLog.Mcp.Infrastructure.ApiClients;
using ModelContextProtocol.Server;
using PlantHarvest.Contract.Query;
using PlantHarvest.Contract.ViewModels;

namespace GardenLog.Mcp.Application.Tools;

[McpServerToolType]
public class SearchPlantHarvestSummariesTool
{
    private readonly IPlantHarvestApiClient _plantHarvestApiClient;
    private readonly IUserContextAccessor _userContextAccessor;
    private readonly ILogger<SearchPlantHarvestSummariesTool> _logger;

    public SearchPlantHarvestSummariesTool(
        IPlantHarvestApiClient plantHarvestApiClient,
        IUserContextAccessor userContextAccessor,
        ILogger<SearchPlantHarvestSummariesTool> logger)
    {
        _plantHarvestApiClient = plantHarvestApiClient;
        _userContextAccessor = userContextAccessor;
        _logger = logger;
    }

    [McpServerTool(Name = "search_plant_harvest_summaries", UseStructuredContent = true)]
    [Description("Search grouped plant harvest summaries for the authenticated user. All filters are optional. Returns one row per plant, grow condition, and harvest cycle with aggregated dates and distinct non-empty feedback notes from PlantHarvestCycle notes.")]
    public async Task<IReadOnlyCollection<PlantHarvestCycleSummaryViewModel>> ExecuteAsync(
        [Description("Optional plant ID filter")] string? plantId = null,
        [Description("Optional plant name text filter")] string? plantName = null,
        [Description("Optional harvest cycle ID filter")] string? harvestCycleId = null,
        [Description("Optional harvest cycle name text filter")] string? harvestCycleName = null,
        CancellationToken cancellationToken = default)
    {
        string? userProfileId = _userContextAccessor.GetUserId();

        if (string.IsNullOrWhiteSpace(userProfileId))
        {
            throw new UnauthorizedAccessException("User context not found.");
        }

        _logger.LogInformation(
            "search_plant_harvest_summaries called: user={UserProfileId}, plantId={PlantId}, plantName={PlantName}, harvestCycleId={HarvestCycleId}, harvestCycleName={HarvestCycleName}",
            userProfileId,
            plantId,
            plantName,
            harvestCycleId,
            harvestCycleName);

        var query = new PlantHarvestCycleSummarySearch
        {
            PlantId = string.IsNullOrWhiteSpace(plantId) ? null : plantId.Trim(),
            PlantName = string.IsNullOrWhiteSpace(plantName) ? null : plantName.Trim(),
            HarvestCycleId = string.IsNullOrWhiteSpace(harvestCycleId) ? null : harvestCycleId.Trim(),
            HarvestCycleName = string.IsNullOrWhiteSpace(harvestCycleName) ? null : harvestCycleName.Trim()
        };

        return await _plantHarvestApiClient.SearchPlantHarvestCycleSummaries(query);
    }
}