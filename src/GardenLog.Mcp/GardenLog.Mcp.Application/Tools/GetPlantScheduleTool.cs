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
    private readonly ILogger<GetPlantScheduleTool> _logger;

    public GetPlantScheduleTool(
        IPlantHarvestApiClient plantHarvestApiClient,
        IPlantCatalogApiClient plantCatalogApiClient,
        ILogger<GetPlantScheduleTool> logger)
    {
        _plantHarvestApiClient = plantHarvestApiClient;
        _plantCatalogApiClient = plantCatalogApiClient;
        _logger = logger;
    }

    [McpServerTool(Name = "get_plant_schedule", UseStructuredContent = true)]
    [Description("Get planned schedule(s) for a plant grouped by grow instruction. For plants with multiple schedules (e.g., spring and fall), all are returned so AI can pick the relevant one by date. Multiple varieties with same grow instruction are merged into one schedule. For actual dates, call get_worklog_history separately.")]
    public async Task<PlantScheduleToolResult?> ExecuteAsync(
        [Description("Plant name (finds current harvest cycle automatically)")] string? plantName = null,
        [Description("Plant harvest cycle ID (if already known)")] string? plantHarvestCycleId = null,
        [Description("Harvest cycle ID (optional, used with plantName to search specific cycle)")] string? harvestCycleId = null,
        CancellationToken cancellationToken = default)
    {
        string? resolvedPlantHarvestCycleId = plantHarvestCycleId;
        string? resolvedHarvestCycleId = harvestCycleId;
        string? resolvedPlantId = null;
        IReadOnlyCollection<PlantHarvest.Contract.ViewModels.PlantHarvestCycleViewModel> cycles = Array.Empty<PlantHarvest.Contract.ViewModels.PlantHarvestCycleViewModel>();

        // If plantName provided, get all cycles for that plant
        if (!string.IsNullOrWhiteSpace(plantName) && string.IsNullOrWhiteSpace(plantHarvestCycleId))
        {
            _logger.LogInformation("get_plant_schedule: Resolving plant name '{PlantName}' to plant harvest cycles", plantName);

            // Get plantId
            resolvedPlantId = await _plantCatalogApiClient.GetPlantIdByName(plantName);
            if (string.IsNullOrWhiteSpace(resolvedPlantId))
            {
                throw new ArgumentException($"Plant '{plantName}' not found.");
            }

            // Get current harvest cycle if not provided
            if (string.IsNullOrWhiteSpace(resolvedHarvestCycleId))
            {
                var harvestCycles = await _plantHarvestApiClient.GetHarvestCycles();
                var currentCycle = harvestCycles
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

            // Find ALL plant harvest cycles for this plant in the cycle
            var plantQuery = new PlantHarvest.Contract.Query.PlantHarvestCycleSearch
            {
                PlantId = resolvedPlantId,
                HarvestCycleId = resolvedHarvestCycleId
            };
            cycles = await _plantHarvestApiClient.SearchPlantHarvestCycles(plantQuery);

            if (!cycles.Any())
            {
                return null;
            }
        }
        // If plantHarvestCycleId provided, get just that one
        else if (!string.IsNullOrWhiteSpace(plantHarvestCycleId))
        {
            resolvedPlantHarvestCycleId = plantHarvestCycleId;

            _logger.LogInformation("get_plant_schedule: Getting schedule for plantHarvestCycleId={PlantHarvestCycleId}", resolvedPlantHarvestCycleId);

            // Search for this specific cycle
            var cycleQuery = new PlantHarvest.Contract.Query.PlantHarvestCycleSearch
            {
                PlantId = resolvedPlantId ?? string.Empty,
                HarvestCycleId = resolvedHarvestCycleId
            };
            var results = await _plantHarvestApiClient.SearchPlantHarvestCycles(cycleQuery);
            var cycle = results.FirstOrDefault(c => c.PlantHarvestCycleId == resolvedPlantHarvestCycleId);
            
            if (cycle == null)
            {
                return null;
            }
            
            cycles = new[] { cycle };
            resolvedPlantId = cycle.PlantId;
            resolvedHarvestCycleId = cycle.HarvestCycleId;
        }
        else
        {
            throw new ArgumentException("Either plantName or plantHarvestCycleId must be provided.");
        }

        if (!cycles.Any())
        {
            return null;
        }

        // Build result with schedules grouped by grow instruction
        var scheduleGroups = cycles
            .GroupBy(c => c.PlantGrowthInstructionId ?? string.Empty)
            .Select(g =>
            {
                var first = g.First();

                return new PlantScheduleGroup
                {
                    PlantHarvestCycleId = first.PlantHarvestCycleId,
                    PlantGrowthInstructionId = first.PlantGrowthInstructionId,
                    PlantGrowthInstructionName = first.PlantGrowthInstructionName,
                    Notes = string.Join(" | ", g.Where(c => !string.IsNullOrWhiteSpace(c.Notes)).Select(c => c.Notes).Distinct()),
                    PlannedTasks = first.PlantCalendar.Select(s => new ScheduledTask
                    {
                        TaskType = s.TaskType.ToString(),
                        StartDate = s.StartDate,
                        EndDate = s.EndDate,
                        IsSystemGenerated = s.IsSystemGenerated
                    }).ToList()
                };
            })
            .ToList();

        var result = new PlantScheduleToolResult
        {
            PlantId = resolvedPlantId ?? string.Empty,
            PlantName = cycles.First().PlantName ?? string.Empty,
            HarvestCycleId = resolvedHarvestCycleId ?? string.Empty,
            Schedules = scheduleGroups
        };

        return result;
    }
}
