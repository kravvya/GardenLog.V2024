using System.ComponentModel;
using GardenLog.Mcp.Application.Services;
using GardenLog.Mcp.Infrastructure.ApiClients;
using ModelContextProtocol.Server;
using PlantHarvest.Contract.Query;
using PlantHarvest.Contract.ViewModels;

namespace GardenLog.Mcp.Application.Tools;

[McpServerToolType]
public class GetPlantHarvestCyclesTool
{
    private readonly IPlantHarvestApiClient _plantHarvestApiClient;
    private readonly IUserContextAccessor _userContextAccessor;
    private readonly ILogger<GetPlantHarvestCyclesTool> _logger;

    public GetPlantHarvestCyclesTool(
        IPlantHarvestApiClient plantHarvestApiClient,
        IUserContextAccessor userContextAccessor,
        ILogger<GetPlantHarvestCyclesTool> logger)
    {
        _plantHarvestApiClient = plantHarvestApiClient;
        _userContextAccessor = userContextAccessor;
        _logger = logger;
    }

    [McpServerTool(Name = "get_plant_harvest_cycles", UseStructuredContent = true)]
    [Description("Search PlantHarvest cycles for the authenticated user with optional filters such as plant, harvest cycle, date range, and minimum germination rate.")]
    public async Task<IReadOnlyCollection<PlantHarvestCycleViewModel>> ExecuteAsync(
        [Description("Optional Plant ID filter")] string? plantId = null,
        [Description("Optional Harvest Cycle ID filter")] string? harvestCycleId = null,
        [Description("Optional start date filter (inclusive)")] DateTime? startDate = null,
        [Description("Optional end date filter (inclusive)")] DateTime? endDate = null,
        [Description("Optional minimum germination rate filter (0-100)")] int? minGerminationRate = null,
        [Description("Maximum number of records to return (default 100, max 500)")] int limit = 100,
        CancellationToken cancellationToken = default)
    {
        string? userProfileId = _userContextAccessor.GetUserId();

        if (string.IsNullOrWhiteSpace(userProfileId))
        {
            throw new UnauthorizedAccessException("User context not found.");
        }

        if (startDate.HasValue && endDate.HasValue && startDate > endDate)
        {
            throw new ArgumentException("startDate must be less than or equal to endDate.");
        }

        if (minGerminationRate is < 0 or > 100)
        {
            throw new ArgumentException("minGerminationRate must be between 0 and 100.");
        }

        int boundedLimit = limit <= 0 ? 100 : Math.Min(limit, 500);

        _logger.LogInformation(
            "get_plant_harvest_cycles called: user={UserProfileId}, plantId={PlantId}, harvestCycleId={HarvestCycleId}, start={StartDate}, end={EndDate}, minGerminationRate={MinGerminationRate}, limit={Limit}",
            userProfileId,
            plantId,
            harvestCycleId,
            startDate,
            endDate,
            minGerminationRate,
            boundedLimit);

        var query = new PlantHarvestCycleSearch
        {
            PlantId = plantId,
            HarvestCycleId = harvestCycleId,
            StartDate = startDate,
            EndDate = endDate,
            MinGerminationRate = minGerminationRate,
            Limit = boundedLimit
        };

        return await _plantHarvestApiClient.SearchPlantHarvestCycles(query);
    }
}