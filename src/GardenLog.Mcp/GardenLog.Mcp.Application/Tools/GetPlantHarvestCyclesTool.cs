using System.ComponentModel;
using GardenLog.Mcp.Application.Services;
using GardenLog.Mcp.Application.Tools.Models;
using GardenLog.Mcp.Infrastructure.ApiClients;
using ModelContextProtocol.Server;
using PlantHarvest.Contract.Query;

namespace GardenLog.Mcp.Application.Tools;

[McpServerToolType]
public class GetPlantHarvestCyclesTool
{
    private readonly IPlantHarvestApiClient _plantHarvestApiClient;
    private readonly IUserManagementApiClient _userManagementApiClient;
    private readonly IUserContextAccessor _userContextAccessor;
    private readonly ILogger<GetPlantHarvestCyclesTool> _logger;

    public GetPlantHarvestCyclesTool(
        IPlantHarvestApiClient plantHarvestApiClient,
        IUserManagementApiClient userManagementApiClient,
        IUserContextAccessor userContextAccessor,
        ILogger<GetPlantHarvestCyclesTool> logger)
    {
        _plantHarvestApiClient = plantHarvestApiClient;
        _userManagementApiClient = userManagementApiClient;
        _userContextAccessor = userContextAccessor;
        _logger = logger;
    }

    [McpServerTool(Name = "get_plant_harvest_cycles", UseStructuredContent = true)]
    [Description("Search PlantHarvest cycles for the authenticated user using PlantHarvest search API. plantId is required. Returns simplified data without layout coordinates.")]
    public async Task<IReadOnlyCollection<PlantHarvestCycleToolResult>> ExecuteAsync(
        [Description("Plant ID filter (required)")] string plantId,
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

        if (string.IsNullOrWhiteSpace(plantId))
        {
            throw new ArgumentException("plantId is required.");
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

        var cycles = await _plantHarvestApiClient.SearchPlantHarvestCycles(query);

        // Get unique garden IDs to fetch bed names
        var gardenIds = cycles
            .SelectMany(c => c.GardenBedLayout)
            .Select(gbl => gbl.GardenId)
            .Where(id => !string.IsNullOrWhiteSpace(id))
            .Distinct()
            .ToList();

        Dictionary<string, string> bedNames = new();
        foreach (var gardenId in gardenIds)
        {
            try
            {
                var beds = await _userManagementApiClient.GetGardenBeds(gardenId);
                foreach (var bed in beds)
                {
                    bedNames.TryAdd(bed.GardenBedId, bed.Name);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to fetch bed names for gardenId={GardenId}", gardenId);
            }
        }

        // Transform to simplified model
        var results = cycles.Select(c => new PlantHarvestCycleToolResult
        {
            PlantHarvestCycleId = c.PlantHarvestCycleId,
            HarvestCycleId = c.HarvestCycleId,
            PlantId = c.PlantId ?? string.Empty,
            PlantName = c.PlantName ?? string.Empty,
            PlantVarietyId = c.PlantVarietyId ?? string.Empty,
            PlantVarietyName = c.PlantVarietyName ?? string.Empty,
            PlantGrowthInstructionId = c.PlantGrowthInstructionId,
            PlantGrowthInstructionName = c.PlantGrowthInstructionName,
            Notes = c.Notes,
            GerminationRate = c.GerminationRate.HasValue ? (int?)Math.Round(c.GerminationRate.Value) : null,
            PlantCalendar = c.PlantCalendar.Select(s => new ScheduledTask
            {
                TaskType = s.TaskType.ToString(),
                StartDate = s.StartDate,
                EndDate = s.EndDate,
                IsSystemGenerated = s.IsSystemGenerated
            }).ToList(),
            GardenBeds = c.GardenBedLayout.Select(gbl => new GardenBedPlacement
            {
                GardenBedId = gbl.GardenBedId,
                GardenBedName = bedNames.GetValueOrDefault(gbl.GardenBedId, gbl.GardenBedId),
                NumberOfPlants = gbl.NumberOfPlants
            }).ToList()
        }).ToList();

        return results;
    }
}