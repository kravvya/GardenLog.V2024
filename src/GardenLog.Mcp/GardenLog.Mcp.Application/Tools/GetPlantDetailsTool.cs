using System.ComponentModel;
using ModelContextProtocol.Server;
using GardenLog.Mcp.Infrastructure.ApiClients;
using GardenLog.Mcp.Application.Tools.Models;

namespace GardenLog.Mcp.Application.Tools;

/// <summary>
/// MCP Tool that retrieves plant catalog information from PlantCatalog API
/// This is an API-wrapper tool that demonstrates token forwarding
/// </summary>
[McpServerToolType]
public class GetPlantDetailsTool
{
    private readonly IPlantCatalogApiClient _plantCatalogClient;
    private readonly ILogger<GetPlantDetailsTool> _logger;

    public GetPlantDetailsTool(
        IPlantCatalogApiClient plantCatalogClient,
        ILogger<GetPlantDetailsTool> logger)
    {
        _plantCatalogClient = plantCatalogClient;
        _logger = logger;
    }

    [McpServerTool(Name = "get_plant_details", UseStructuredContent = true)]
    [Description("Get plant catalog information including grow instructions. Use when user asks about a specific plant's details, growing requirements, or planting instructions. Requires exact plant ID.")]
    public async Task<PlantDetailsToolResult?> ExecuteAsync(
        [Description("Plant ID (required)")] string plantId,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("get_plant_details called: plantId={PlantId}", plantId);

        if (string.IsNullOrEmpty(plantId))
        {
            throw new ArgumentException("plantId is required", nameof(plantId));
        }

        PlantCatalogPlantDetails? details;
        try
        {
            details = await _plantCatalogClient.GetPlantDetails(plantId);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Plant details request failed because grow instructions are missing: {PlantId}", plantId);
            throw new InvalidOperationException(
                $"No grow instructions are available for plantId '{plantId}'. This tool requires grow instructions to be present. Try a different plantId or add grow instructions in Plant Catalog first.");
        }

        if (details == null)
        {
            _logger.LogWarning("Plant not found: {PlantId}", plantId);
            return null;
        }

        _logger.LogInformation(
            "Successfully retrieved plant: {PlantName}. Grow instructions returned: {GrowInstructionCount}",
            details.Plant.Name,
            details.GrowInstructions.Count);

        return new PlantDetailsToolResult
        {
            Plant = details.Plant,
            GrowInstructions = details.GrowInstructions
        };
    }
}
