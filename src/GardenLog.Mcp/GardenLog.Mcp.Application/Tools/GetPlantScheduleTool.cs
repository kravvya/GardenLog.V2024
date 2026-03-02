using System.ComponentModel;
using GardenLog.Mcp.Application.Tools.Models;
using GardenLog.Mcp.Infrastructure.ApiClients;
using ModelContextProtocol.Server;

namespace GardenLog.Mcp.Application.Tools;

[McpServerToolType]
public class GetPlantScheduleTool
{
    private readonly IPlantHarvestApiClient _plantHarvestApiClient;
    private readonly IPlantCatalogApiClient _plantCatalogApiClient;
    private readonly IUserManagementApiClient _userManagementApiClient;
    private readonly ILogger<GetPlantScheduleTool> _logger;

    public GetPlantScheduleTool(
        IPlantHarvestApiClient plantHarvestApiClient,
        IPlantCatalogApiClient plantCatalogApiClient,
        IUserManagementApiClient userManagementApiClient,
        ILogger<GetPlantScheduleTool> logger)
    {
        _plantHarvestApiClient = plantHarvestApiClient;
        _plantCatalogApiClient = plantCatalogApiClient;
        _userManagementApiClient = userManagementApiClient;
        _logger = logger;
    }

    [McpServerTool(Name = "get_plant_schedule", UseStructuredContent = true)]
    [Description("Get planned schedule for a plant. Provide plantName for convenience (finds current cycle), or provide plantHarvestCycleId if already known. For actual dates, call get_worklog_history separately.")]
    public async Task<PlantScheduleToolResult?> ExecuteAsync(
        [Description("Plant name (finds current harvest cycle automatically)")] string? plantName = null,
        [Description("Plant harvest cycle ID (if already known)")] string? plantHarvestCycleId = null,
        [Description("Harvest cycle ID (optional, used with plantName to search specific cycle)")] string? harvestCycleId = null,
        CancellationToken cancellationToken = default)
    {
        string? resolvedPlantHarvestCycleId = plantHarvestCycleId;
        string? resolvedHarvestCycleId = harvestCycleId;

        // If plantName provided, resolve to plantHarvestCycleId
        if (!string.IsNullOrWhiteSpace(plantName) && string.IsNullOrWhiteSpace(plantHarvestCycleId))
        {
            _logger.LogInformation("get_plant_schedule: Resolving plant name '{PlantName}' to plantHarvestCycleId", plantName);

            // Get plantId
            var plantId = await _plantCatalogApiClient.GetPlantIdByName(plantName);
            if (string.IsNullOrWhiteSpace(plantId))
            {
                throw new ArgumentException($"Plant '{plantName}' not found.");
            }

            // Get current harvest cycle if not provided
            if (string.IsNullOrWhiteSpace(resolvedHarvestCycleId))
            {
                var cycles = await _plantHarvestApiClient.GetHarvestCycles();
                var currentCycle = cycles
                    .Where(h => h.StartDate <= DateTime.UtcNow)
                    .Where(h => !h.EndDate.HasValue)
                    .OrderByDescending(h => h.StartDate)
                    .FirstOrDefault();
                if (currentCycle == null)
                {
                    throw new InvalidOperationException("No active harvest cycle found.");
                }
                resolvedHarvestCycleId = currentCycle.HarvestCycleId;
            }

            // Find plant harvest cycles
            var plantQuery = new PlantHarvest.Contract.Query.PlantHarvestCycleSearch
            {
                PlantId = plantId,
                HarvestCycleId = resolvedHarvestCycleId
            };
            var plantHarvestCycles = await _plantHarvestApiClient.SearchPlantHarvestCycles(plantQuery);

            if (!plantHarvestCycles.Any())
            {
                return null;
            }

            // Take first match (could be multiple varieties)
            resolvedPlantHarvestCycleId = plantHarvestCycles.First().PlantHarvestCycleId;
        }

        if (string.IsNullOrWhiteSpace(resolvedPlantHarvestCycleId))
        {
            throw new ArgumentException("Either plantName or plantHarvestCycleId must be provided.");
        }

        _logger.LogInformation("get_plant_schedule: Getting schedule for plantHarvestCycleId={PlantHarvestCycleId}", resolvedPlantHarvestCycleId);

        // Get the plant harvest cycle details
        var cycleQuery = new PlantHarvest.Contract.Query.PlantHarvestCycleSearch
        {
            PlantId = string.Empty  // Will be filtered below
        };
        var allResults = await _plantHarvestApiClient.SearchPlantHarvestCycles(cycleQuery);
        var cycle = allResults.FirstOrDefault(c => c.PlantHarvestCycleId == resolvedPlantHarvestCycleId);

        if (cycle == null)
        {
            return null;
        }

        // Get bed names
        Dictionary<string, string> bedNames = new();
        if (cycle.GardenBedLayout.Any())
        {
            var gardenId = cycle.GardenBedLayout.First().GardenId;
            if (!string.IsNullOrWhiteSpace(gardenId))
            {
                var beds = await _userManagementApiClient.GetGardenBeds(gardenId);
                bedNames = beds.ToDictionary(b => b.GardenBedId, b => b.Name);
            }
        }

        var result = new PlantScheduleToolResult
        {
            PlantHarvestCycleId = cycle.PlantHarvestCycleId,
            PlantId = cycle.PlantId ?? string.Empty,
            PlantName = cycle.PlantName ?? string.Empty,
            PlantVarietyId = cycle.PlantVarietyId ?? string.Empty,
            PlantVarietyName = cycle.PlantVarietyName ?? string.Empty,
            HarvestCycleId = cycle.HarvestCycleId,
            Notes = cycle.Notes,
            PlannedTasks = cycle.PlantCalendar.Select(s => new ScheduledTask
            {
                TaskType = s.TaskType.ToString(),
                StartDate = s.StartDate,
                EndDate = s.EndDate,
                IsSystemGenerated = s.IsSystemGenerated
            }).ToList(),
            Beds = cycle.GardenBedLayout.Select(gbl => new GardenBedPlacement
            {
                GardenBedId = gbl.GardenBedId,
                GardenBedName = bedNames.GetValueOrDefault(gbl.GardenBedId, gbl.GardenBedId),
                NumberOfPlants = gbl.NumberOfPlants
            }).ToList()
        };

        return result;
    }
}
