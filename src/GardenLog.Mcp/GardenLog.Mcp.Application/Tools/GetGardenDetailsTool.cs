using System.ComponentModel;
using GardenLog.Mcp.Application.Tools.Models;
using GardenLog.Mcp.Infrastructure.ApiClients;
using ModelContextProtocol.Server;
using UserManagement.Contract.ViewModels;

namespace GardenLog.Mcp.Application.Tools;

[McpServerToolType]
public class GetGardenDetailsTool
{
    private readonly IUserManagementApiClient _userManagementApiClient;
    private readonly ILogger<GetGardenDetailsTool> _logger;

    public GetGardenDetailsTool(
        IUserManagementApiClient userManagementApiClient,
        ILogger<GetGardenDetailsTool> logger)
    {
        _userManagementApiClient = userManagementApiClient;
        _logger = logger;
    }

    [McpServerTool(Name = "get_garden_details", UseStructuredContent = true)]
    [Description("Get garden details for the authenticated user by garden ID or garden name, with optional garden bed details.")]
    public async Task<GardenDetailsToolResult?> ExecuteAsync(
        [Description("Garden ID (optional if gardenName is provided)")] string? gardenId = null,
        [Description("Garden name (optional if gardenId is provided)")] string? gardenName = null,
        [Description("Include garden bed details in response")] bool includeGardenBeds = true,
        CancellationToken cancellationToken = default)
    {
        GardenViewModel? garden = null;

        if (string.IsNullOrWhiteSpace(gardenId) && string.IsNullOrWhiteSpace(gardenName))
        {
            var gardens = await _userManagementApiClient.GetGardens();

            if (gardens.Count == 0)
            {
                throw new ArgumentException("No gardens found for the authenticated user.");
            }

            if (gardens.Count == 1)
            {
                garden = gardens.First();
                _logger.LogInformation("get_garden_details resolved single garden: gardenId={GardenId}", garden.GardenId);
            }
            else
            {
                var available = string.Join(", ", gardens.Select(g => $"{g.Name} ({g.GardenId})").Take(5));
                throw new ArgumentException(
                    $"Multiple gardens found. Provide gardenId or gardenName. Available gardens: {available}");
            }
        }

        if (garden == null && !string.IsNullOrWhiteSpace(gardenId))
        {
            _logger.LogInformation("get_garden_details called with gardenId={GardenId}", gardenId);
            garden = await _userManagementApiClient.GetGarden(gardenId);
        }
        else if (garden == null && !string.IsNullOrWhiteSpace(gardenName))
        {
            _logger.LogInformation("get_garden_details called with gardenName={GardenName}", gardenName);
            garden = await _userManagementApiClient.GetGardenByName(gardenName);
        }

        if (garden == null)
        {
            return null;
        }

        IReadOnlyCollection<GardenBedViewModel> beds = Array.Empty<GardenBedViewModel>();

        if (includeGardenBeds)
        {
            beds = await _userManagementApiClient.GetGardenBeds(garden.GardenId);
        }

        return new GardenDetailsToolResult
        {
            Garden = garden,
            GardenBeds = beds
        };
    }
}