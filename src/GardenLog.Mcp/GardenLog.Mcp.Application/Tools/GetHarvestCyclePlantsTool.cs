using System.ComponentModel;
using GardenLog.Mcp.Application.Tools.Models;
using GardenLog.Mcp.Infrastructure.ApiClients;
using ModelContextProtocol.Server;
using PlantHarvest.Contract.ViewModels;

namespace GardenLog.Mcp.Application.Tools;

[McpServerToolType]
public class GetHarvestCyclePlantsTool
{
    private readonly IPlantHarvestApiClient _plantHarvestApiClient;
    private readonly ILogger<GetHarvestCyclePlantsTool> _logger;

    public GetHarvestCyclePlantsTool(
        IPlantHarvestApiClient plantHarvestApiClient,
        ILogger<GetHarvestCyclePlantsTool> logger)
    {
        _plantHarvestApiClient = plantHarvestApiClient;
        _logger = logger;
    }

    [McpServerTool(Name = "get_harvest_cycle_plants", UseStructuredContent = true)]
    [Description("Get all distinct plant schedules (grouped by PlantId and GrowthInstructionId) in a harvest cycle for the authenticated user. Includes calendar/schedule information.")]
    public async Task<IReadOnlyCollection<HarvestCyclePlantToolResult>> ExecuteAsync(
        [Description("Harvest cycle ID (required)")] string harvestCycleId,
        [Description("Include full plant varieties and their individual harvest cycle records")] bool includePlantVarieties = true,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(harvestCycleId))
        {
            throw new ArgumentException("harvestCycleId is required.");
        }

        _logger.LogInformation(
            "get_harvest_cycle_plants called: harvestCycleId={HarvestCycleId}, includePlantVarieties={IncludePlantVarieties}",
            harvestCycleId,
            includePlantVarieties);

        var plantHarvestCycles = await _plantHarvestApiClient.GetPlantHarvestCycles(harvestCycleId);

        var plants = plantHarvestCycles
            .GroupBy(x => $"{x.PlantId?.ToLowerInvariant()}|{x.PlantGrowthInstructionId?.ToLowerInvariant()}")
            .Select(g =>
            {
                var firstCycle = g.First();
                var plantId = firstCycle.PlantId;
                var growthInstructionId = firstCycle.PlantGrowthInstructionId;

                var plantName = g
                    .Select(x => x.PlantName)
                    .FirstOrDefault(x => !string.IsNullOrWhiteSpace(x))
                    ?? plantId;

                var growthInstructionName = g
                    .Select(x => x.PlantGrowthInstructionName)
                    .FirstOrDefault(x => !string.IsNullOrWhiteSpace(x));

                var cycles = g
                    .OrderBy(x => x.PlantVarietyName, StringComparer.OrdinalIgnoreCase)
                    .ToList();

                var varietyNames = g
                    .Where(x => !string.IsNullOrWhiteSpace(x.PlantVarietyName))
                    .Select(x => x.PlantVarietyName!.Trim())
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .OrderBy(x => x, StringComparer.OrdinalIgnoreCase)
                    .ToList();

                var allSchedules = g
                    .SelectMany(x => x.PlantCalendar)
                    .OrderBy(s => s.StartDate)
                    .ToList();

                return new HarvestCyclePlantToolResult
                {
                    PlantId = plantId,
                    PlantName = plantName,
                    PlantGrowthInstructionId = growthInstructionId,
                    PlantGrowthInstructionName = growthInstructionName,
                    PlantHarvestCycleCount = cycles.Count,
                    VarietyCount = varietyNames.Count,
                    VarietyNames = varietyNames,
                    Schedules = allSchedules,
                    PlantVarieties = includePlantVarieties ? cycles : Array.Empty<PlantHarvestCycleViewModel>()
                };
            })
            .OrderBy(x => x.PlantName, StringComparer.OrdinalIgnoreCase)
            .ThenBy(x => x.PlantGrowthInstructionName, StringComparer.OrdinalIgnoreCase)
            .ToList();

        return plants;
    }
}
