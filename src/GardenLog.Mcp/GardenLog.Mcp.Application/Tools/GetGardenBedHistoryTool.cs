using System.ComponentModel;
using GardenLog.Mcp.Infrastructure.ApiClients;
using ModelContextProtocol.Server;
using PlantHarvest.Contract.ViewModels;

namespace GardenLog.Mcp.Application.Tools;

[McpServerToolType]
public class GetGardenBedHistoryTool
{
    private readonly IPlantHarvestApiClient _plantHarvestApiClient;
    private readonly ILogger<GetGardenBedHistoryTool> _logger;

    public GetGardenBedHistoryTool(
        IPlantHarvestApiClient plantHarvestApiClient,
        ILogger<GetGardenBedHistoryTool> logger)
    {
        _plantHarvestApiClient = plantHarvestApiClient;
        _logger = logger;
    }

    [McpServerTool(Name = "get_garden_bed_history", UseStructuredContent = true)]
    [Description("Get historical garden bed occupancy from PlantHarvest for crop rotation analysis.")]
    public async Task<IReadOnlyCollection<GardenBedPlantHarvestCycleViewModel>> ExecuteAsync(
        [Description("Garden ID (required)")] string gardenId,
        [Description("Garden bed ID (required)")] string gardenBedId,
        [Description("Optional start date filter (inclusive)")] DateTime? startDate = null,
        [Description("Optional end date filter (inclusive)")] DateTime? endDate = null,
        [Description("Maximum number of records to return (default 100, max 500)")] int limit = 100,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(gardenId))
        {
            throw new ArgumentException("gardenId is required", nameof(gardenId));
        }

        if (string.IsNullOrWhiteSpace(gardenBedId))
        {
            throw new ArgumentException("gardenBedId is required", nameof(gardenBedId));
        }

        if (startDate.HasValue && endDate.HasValue && startDate > endDate)
        {
            throw new ArgumentException("startDate must be less than or equal to endDate.");
        }

        int boundedLimit = limit <= 0 ? 100 : Math.Min(limit, 500);

        _logger.LogInformation(
            "get_garden_bed_history called: gardenId={GardenId}, gardenBedId={GardenBedId}, start={StartDate}, end={EndDate}, limit={Limit}",
            gardenId,
            gardenBedId,
            startDate,
            endDate,
            boundedLimit);

        var history = await _plantHarvestApiClient.GetGardenBedUsageHistory(gardenId, gardenBedId);

        var filtered = history
            .Where(item =>
            {
                var occupancyStart = item.StartDate ?? DateTime.MinValue;
                var occupancyEnd = item.EndDate ?? DateTime.MaxValue;

                if (startDate.HasValue && occupancyEnd < startDate.Value)
                {
                    return false;
                }

                if (endDate.HasValue && occupancyStart > endDate.Value)
                {
                    return false;
                }

                return true;
            })
            .OrderByDescending(item => item.StartDate ?? DateTime.MinValue)
            .Take(boundedLimit)
            .ToList();

        return filtered;
    }
}