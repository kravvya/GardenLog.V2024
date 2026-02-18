using System.ComponentModel;
using GardenLog.Mcp.Infrastructure.ApiClients;
using ModelContextProtocol.Server;
using PlantCatalog.Contract.ViewModels;

namespace GardenLog.Mcp.Application.Tools;

[McpServerToolType]
public class SearchPlantsTool
{
    private readonly IPlantCatalogApiClient _plantCatalogApiClient;
    private readonly ILogger<SearchPlantsTool> _logger;

    public SearchPlantsTool(
        IPlantCatalogApiClient plantCatalogApiClient,
        ILogger<SearchPlantsTool> logger)
    {
        _plantCatalogApiClient = plantCatalogApiClient;
        _logger = logger;
    }

    [McpServerTool(Name = "search_plants", UseStructuredContent = true)]
    [Description("Search plants by name and return plant IDs. Uses exact name lookup first, then falls back to contains matching from plant names.")]
    public async Task<IReadOnlyCollection<PlantNameOnlyViewModel>> ExecuteAsync(
        [Description("Plant name text to search for")] string plantName,
        [Description("Maximum number of results to return (default 10, max 50)")] int limit = 10,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(plantName))
        {
            throw new ArgumentException("plantName is required.");
        }

        int boundedLimit = limit <= 0 ? 10 : Math.Min(limit, 50);
        string query = plantName.Trim();

        _logger.LogInformation("search_plants called: plantName={PlantName}, limit={Limit}", query, boundedLimit);

        var exactId = await _plantCatalogApiClient.GetPlantIdByName(query);
        var plantNames = await _plantCatalogApiClient.GetAllPlantNames();

        if (!string.IsNullOrWhiteSpace(exactId))
        {
            var exact = plantNames.FirstOrDefault(p => p.PlantId == exactId)
                ?? plantNames.FirstOrDefault(p => string.Equals(p.Name, query, StringComparison.OrdinalIgnoreCase));

            if (exact != null)
            {
                return new List<PlantNameOnlyViewModel> { exact };
            }

            return new List<PlantNameOnlyViewModel>
            {
                new()
                {
                    PlantId = exactId,
                    Name = query,
                    Color = string.Empty
                }
            };
        }

        var matches = plantNames
            .Where(p => p.Name.Contains(query, StringComparison.OrdinalIgnoreCase))
            .OrderByDescending(p => p.Name.StartsWith(query, StringComparison.OrdinalIgnoreCase))
            .ThenBy(p => p.Name)
            .Take(boundedLimit)
            .ToList();

        return matches;
    }
}
