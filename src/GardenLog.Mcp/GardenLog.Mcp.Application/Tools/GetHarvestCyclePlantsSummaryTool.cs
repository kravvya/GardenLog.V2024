using System.ComponentModel;
using GardenLog.Mcp.Application.Tools.Models;
using GardenLog.Mcp.Infrastructure.ApiClients;
using ModelContextProtocol.Server;

namespace GardenLog.Mcp.Application.Tools;

[McpServerToolType]
public class GetHarvestCyclePlantsSummaryTool
{
    private readonly IPlantHarvestApiClient _plantHarvestApiClient;
    private readonly IUserManagementApiClient _userManagementApiClient;
    private readonly ILogger<GetHarvestCyclePlantsSummaryTool> _logger;

    public GetHarvestCyclePlantsSummaryTool(
        IPlantHarvestApiClient plantHarvestApiClient,
        IUserManagementApiClient userManagementApiClient,
        ILogger<GetHarvestCyclePlantsSummaryTool> logger)
    {
        _plantHarvestApiClient = plantHarvestApiClient;
        _userManagementApiClient = userManagementApiClient;
        _logger = logger;
    }

    [McpServerTool(Name = "get_harvest_cycle_plants_summary", UseStructuredContent = true)]
    [Description("Get a simple list of plants in a harvest cycle with optional variety and garden bed details. Lightweight alternative to get_harvest_cycle_plants - excludes schedules, instructions, and layout coordinates.")]
    public async Task<IReadOnlyCollection<HarvestCyclePlantSummaryToolResult>> ExecuteAsync(
        [Description("Harvest cycle ID (required)")] string harvestCycleId,
        [Description("Include variety details per plant")] bool includeVarieties = false,
        [Description("Include garden bed placement per variety (requires includeVarieties=true)")] bool includeBeds = false,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(harvestCycleId))
        {
            throw new ArgumentException("harvestCycleId is required.");
        }

        _logger.LogInformation(
            "get_harvest_cycle_plants_summary called: harvestCycleId={HarvestCycleId}, includeVarieties={IncludeVarieties}, includeBeds={IncludeBeds}",
            harvestCycleId,
            includeVarieties,
            includeBeds);

        var plantHarvestCycles = await _plantHarvestApiClient.GetPlantHarvestCycles(harvestCycleId);

        // Get garden bed names if needed
        Dictionary<string, string> bedNames = new();
        if (includeBeds && includeVarieties)
        {
            var allBedIds = plantHarvestCycles
                .SelectMany(phc => phc.GardenBedLayout)
                .Select(gbl => gbl.GardenBedId)
                .Distinct()
                .ToList();

            if (allBedIds.Any())
            {
                var gardenId = plantHarvestCycles.FirstOrDefault()?.GardenBedLayout.FirstOrDefault()?.GardenId;
                if (!string.IsNullOrWhiteSpace(gardenId))
                {
                    var beds = await _userManagementApiClient.GetGardenBeds(gardenId);
                    bedNames = beds.ToDictionary(b => b.GardenBedId, b => b.Name);
                }
            }
        }

        var plants = plantHarvestCycles
            .GroupBy(x => x.PlantId ?? string.Empty)
            .Select(g =>
            {
                var firstCycle = g.First();
                var plantId = firstCycle.PlantId ?? string.Empty;
                var plantName = firstCycle.PlantName ?? plantId;

                IReadOnlyCollection<PlantVarietySummary> varieties = includeVarieties
                    ? g.Select(phc =>
                    {
                        IReadOnlyCollection<GardenBedPlacement> beds = includeBeds
                            ? phc.GardenBedLayout.Select(gbl => new GardenBedPlacement
                            {
                                GardenBedId = gbl.GardenBedId,
                                GardenBedName = bedNames.GetValueOrDefault(gbl.GardenBedId, gbl.GardenBedId),
                                NumberOfPlants = gbl.NumberOfPlants
                            }).ToList()
                            : Array.Empty<GardenBedPlacement>();

                        return new PlantVarietySummary
                        {
                            PlantHarvestCycleId = phc.PlantHarvestCycleId,
                            PlantVarietyId = phc.PlantVarietyId ?? string.Empty,
                            PlantVarietyName = phc.PlantVarietyName ?? string.Empty,
                            PlantGrowthInstructionId = phc.PlantGrowthInstructionId,
                            PlantGrowthInstructionName = phc.PlantGrowthInstructionName,
                            Beds = beds
                        };
                    }).ToList()
                    : Array.Empty<PlantVarietySummary>();

                return new HarvestCyclePlantSummaryToolResult
                {
                    PlantId = plantId,
                    PlantName = plantName,
                    Varieties = varieties
                };
            })
            .OrderBy(x => x.PlantName, StringComparer.OrdinalIgnoreCase)
            .ToList();

        return plants;
    }
}
