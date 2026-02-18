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
    [Description("Get all distinct plants in a harvest cycle for the authenticated user. Can include full PlantHarvestCycle records with calendar dates.")]
    public async Task<IReadOnlyCollection<HarvestCyclePlantToolResult>> ExecuteAsync(
        [Description("Harvest cycle ID (required)")] string harvestCycleId,
        [Description("Include full plant harvest cycle records (including plantCalendar dates)")] bool includePlantHarvestCycles = true,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(harvestCycleId))
        {
            throw new ArgumentException("harvestCycleId is required.");
        }

        _logger.LogInformation(
            "get_harvest_cycle_plants called: harvestCycleId={HarvestCycleId}, includePlantHarvestCycles={IncludePlantHarvestCycles}",
            harvestCycleId,
            includePlantHarvestCycles);

        var plantHarvestCycles = await _plantHarvestApiClient.GetPlantHarvestCycles(harvestCycleId);

        var plants = plantHarvestCycles
            .GroupBy(x => x.PlantId, StringComparer.OrdinalIgnoreCase)
            .Select(g =>
            {
                var plantName = g
                    .Select(x => x.PlantName)
                    .FirstOrDefault(x => !string.IsNullOrWhiteSpace(x))
                    ?? g.Key;

                var cycles = g
                    .OrderBy(x => x.PlantVarietyName, StringComparer.OrdinalIgnoreCase)
                    .ToList();

                var varietyNames = g
                    .Where(x => !string.IsNullOrWhiteSpace(x.PlantVarietyName))
                    .Select(x => x.PlantVarietyName!.Trim())
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .OrderBy(x => x, StringComparer.OrdinalIgnoreCase)
                    .ToList();

                return new HarvestCyclePlantToolResult
                {
                    PlantId = g.Key,
                    PlantName = plantName,
                    PlantHarvestCycleCount = cycles.Count,
                    VarietyCount = varietyNames.Count,
                    VarietyNames = varietyNames,
                    PlantHarvestCycles = includePlantHarvestCycles ? cycles : Array.Empty<PlantHarvestCycleViewModel>()
                };
            })
            .OrderBy(x => x.PlantName, StringComparer.OrdinalIgnoreCase)
            .ToList();

        return plants;
    }
}
