using System.ComponentModel;
using GardenLog.Mcp.Infrastructure.ApiClients;
using ModelContextProtocol.Server;
using PlantHarvest.Contract.ViewModels;

namespace GardenLog.Mcp.Application.Tools;

[McpServerToolType]
public class GetCurrentHarvestCycleTool
{
    private readonly IPlantHarvestApiClient _plantHarvestApiClient;
    private readonly ILogger<GetCurrentHarvestCycleTool> _logger;

    public GetCurrentHarvestCycleTool(
        IPlantHarvestApiClient plantHarvestApiClient,
        ILogger<GetCurrentHarvestCycleTool> logger)
    {
        _plantHarvestApiClient = plantHarvestApiClient;
        _logger = logger;
    }

    [McpServerTool(Name = "get_current_harvest_cycle", UseStructuredContent = true)]
    [Description("Get current harvest cycle for the authenticated user. Current means StartDate is on/before asOfDate and EndDate is not set.")]
    public async Task<HarvestCycleViewModel?> ExecuteAsync(
        [Description("Optional garden ID filter")] string? gardenId = null,
        [Description("As-of date for current cycle calculation (defaults to now UTC)")] DateTime? asOfDate = null,
        CancellationToken cancellationToken = default)
    {
        var effectiveDate = asOfDate ?? DateTime.UtcNow;

        _logger.LogInformation(
            "get_current_harvest_cycle called: gardenId={GardenId}, asOfDate={AsOfDate}",
            gardenId,
            effectiveDate);

        var harvestCycles = await _plantHarvestApiClient.GetHarvestCycles();

        var current = harvestCycles
            .Where(h => string.IsNullOrWhiteSpace(gardenId) || string.Equals(h.GardenId, gardenId, StringComparison.OrdinalIgnoreCase))
            .Where(h => h.StartDate <= effectiveDate)
            .Where(h => !h.EndDate.HasValue)
            .OrderByDescending(h => h.StartDate)
            .FirstOrDefault();

        return current;
    }
}
