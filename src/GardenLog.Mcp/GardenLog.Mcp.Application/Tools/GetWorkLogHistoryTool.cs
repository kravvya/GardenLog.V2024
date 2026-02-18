using System.ComponentModel;
using GardenLog.Mcp.Application.Services;
using GardenLog.Mcp.Infrastructure.ApiClients;
using ModelContextProtocol.Server;
using PlantHarvest.Contract.Enum;
using PlantHarvest.Contract.Query;
using PlantHarvest.Contract.ViewModels;

namespace GardenLog.Mcp.Application.Tools;

[McpServerToolType]
public class GetWorkLogHistoryTool
{
    private readonly IPlantHarvestApiClient _plantHarvestApiClient;
    private readonly IUserContextAccessor _userContextAccessor;
    private readonly ILogger<GetWorkLogHistoryTool> _logger;

    public GetWorkLogHistoryTool(
        IPlantHarvestApiClient plantHarvestApiClient,
        IUserContextAccessor userContextAccessor,
        ILogger<GetWorkLogHistoryTool> logger)
    {
        _plantHarvestApiClient = plantHarvestApiClient;
        _userContextAccessor = userContextAccessor;
        _logger = logger;
    }

    [McpServerTool(Name = "get_worklog_history", UseStructuredContent = true)]
    [Description("Get historical WorkLog entries for the authenticated user using PlantHarvest search API. plantId is required. Results are sorted by EventDateTime descending.")]
    public async Task<IReadOnlyCollection<WorkLogViewModel>> ExecuteAsync(
        [Description("Plant ID filter (required)")] string plantId,
        [Description("Optional start date filter (inclusive)")] DateTime? startDate = null,
        [Description("Optional end date filter (inclusive)")] DateTime? endDate = null,
        [Description("Optional work log reason filter")] WorkLogReasonEnum? reason = null,
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

        int boundedLimit = limit <= 0 ? 100 : Math.Min(limit, 500);

        _logger.LogInformation(
            "get_worklog_history called: user={UserProfileId}, plantId={PlantId}, start={StartDate}, end={EndDate}, reason={Reason}, limit={Limit}",
            userProfileId,
            plantId,
            startDate,
            endDate,
            reason,
            boundedLimit);

        var query = new WorkLogSearch
        {
            PlantId = plantId,
            StartDate = startDate,
            EndDate = endDate,
            Reason = reason,
            Limit = boundedLimit
        };

        return await _plantHarvestApiClient.SearchWorkLogs(query);
    }
}
