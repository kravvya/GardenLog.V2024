using GardenLog.SharedKernel;
using GardenLog.SharedKernel.Enum;

namespace GardenLogWeb.Services;

public interface IWorkLogService
{
    Task<List<WorkLogModel>> GetWorkLogs(RelatedEntityTypEnum entityType, string entityId, bool forceRefresh);
    Task<ApiObjectResponse<string>> CreateWorkLog(WorkLogModel workModel);
    Task<ApiResponse> UpdateWorkLog(WorkLogModel workModel);
    Task<ApiResponse> DeleteWorkLog(string id, IList<RelatedEntity> relatedEntities);
}

public class WorkLogService : IWorkLogService
{
    private readonly ILogger<WorkLogService> _logger;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ICacheService _cacheService;
    private readonly IGardenLogToastService _toastService;
    private readonly IImageService _imageService;
    private readonly int _cacheDuration;
    private const string WORK_LOG_KEY = "WorkLogs_{0}_{1}";

    public WorkLogService(ILogger<WorkLogService> logger, IHttpClientFactory clientFactory, ICacheService cacheService, IGardenLogToastService toastService, IImageService imageService, IConfiguration configuration)
    {
        _logger = logger;
        _httpClientFactory = clientFactory;
        _cacheService = cacheService;
        _toastService = toastService;
        _imageService = imageService;
        if (!int.TryParse(configuration[GlobalConstants.GLOBAL_CACHE_DURATION], out _cacheDuration)) _cacheDuration = 10;
    }

    #region Public Work Log Functions

    public async Task<List<WorkLogModel>> GetWorkLogs(RelatedEntityTypEnum entityType, string entityId, bool forceRefresh)
    {
        string key = string.Format(WORK_LOG_KEY, entityType, entityId);

        if (forceRefresh || !_cacheService.TryGetValue<List<WorkLogModel>>(key, out List<WorkLogModel>? workLogs))
        {
            _logger.LogInformation("Work logs not in cache or forceRefresh");

            workLogs = await GetWorkLogs(entityType, entityId);

            // Save data in cache.
            _cacheService.Set(key, workLogs, DateTime.Now.AddMinutes(_cacheDuration));
        }

        else
        {
            _logger.LogInformation($"Harvests are in cache. Found {workLogs!.Count}");
        }

        return workLogs;
    }

    public async Task<ApiObjectResponse<string>> CreateWorkLog(WorkLogModel workLog)
    {
        var httpClient = _httpClientFactory.CreateClient(GlobalConstants.PLANTHARVEST_API);

        var response = await httpClient.ApiPostAsync(HarvestRoutes.CreateWorkLog, workLog);

        if (response.ValidationProblems != null)
        {
            _toastService.ShowToast($"Unable to create a Work Notes. Please resolve validatione errors and try again.", GardenLogToastLevel.Error);
        }
        else if (!response.IsSuccess)
        {
            _toastService.ShowToast($"Received an invalid response from Work notes post: {response.ErrorMessage}", GardenLogToastLevel.Error);
        }
        else
        {
            workLog.WorkLogId = response.Response!;

            AddOrUpdateToWorkLogList(workLog);

            _toastService.ShowToast($"Working notes saved", GardenLogToastLevel.Success);
        }

        return response;
    }

    public async Task<ApiResponse> UpdateWorkLog(WorkLogModel workLog)
    {
        var httpClient = _httpClientFactory.CreateClient(GlobalConstants.PLANTHARVEST_API);

        var response = await httpClient.ApiPutAsync(HarvestRoutes.UpdateWorkLog.Replace("{id}", workLog.WorkLogId), workLog);

        if (response.ValidationProblems != null)
        {
            _toastService.ShowToast($"Unable to update Work Notes. Please resolve validatione errors and try again.", GardenLogToastLevel.Error);
        }
        else if (!response.IsSuccess)
        {
            _toastService.ShowToast($"Received an invalid response: {response.ErrorMessage}", GardenLogToastLevel.Error);
        }
        else
        {
            AddOrUpdateToWorkLogList(workLog);

            _toastService.ShowToast($"Working notes successfully saved.", GardenLogToastLevel.Success);
        }

        return response;
    }

    public async Task<ApiResponse> DeleteWorkLog(string id, IList<RelatedEntity> relatedEntities)
    {
        var httpClient = _httpClientFactory.CreateClient(GlobalConstants.PLANTHARVEST_API);

        var response = await httpClient.ApiDeleteAsync(HarvestRoutes.DeleteWorkLog.Replace("{id}", id));

        if (response.ValidationProblems != null)
        {
            _toastService.ShowToast($"Unable to delete a Working notes. Please resolve validatione errors and try again.", GardenLogToastLevel.Error);
        }
        else if (!response.IsSuccess)
        {
            _toastService.ShowToast($"Received an invalid response: {response.ErrorMessage}", GardenLogToastLevel.Error);
        }
        else
        {
            RemoveFromWorkLogList(id, relatedEntities);

            _toastService.ShowToast($"Working notes deleted.", GardenLogToastLevel.Success);
        }
        return response;

    }

    #endregion


    #region Private Work Log Functions
    private async Task<List<WorkLogModel>> GetWorkLogs(RelatedEntityTypEnum entityType, string entityId)
    {
        var httpClient = _httpClientFactory.CreateClient(GlobalConstants.PLANTHARVEST_API);

        var url = HarvestRoutes.GetWorkLogs.Replace("{entityType}", entityType.ToString()).Replace("{entityId}", entityId);

        var response = await httpClient.ApiGetAsync<List<WorkLogModel>>(url, _logger);

        if (!response.IsSuccess)
        {
            _toastService.ShowToast("Unable to get Work Notes", GardenLogToastLevel.Error);
            return new List<WorkLogModel>();
        }

        return response.Response!;
    }

    private void AddOrUpdateToWorkLogList(WorkLogModel workLog)
    {
        foreach (var relatedEntity in workLog.RelatedEntities)
        {
            string key = string.Format(WORK_LOG_KEY, relatedEntity.EntityType, relatedEntity.EntityId);

            if (_cacheService.TryGetValue<List<WorkLogModel>>(key, out List<WorkLogModel>? workLogs))
            {
                var index = workLogs!.FindIndex(p => p.WorkLogId == workLog.WorkLogId);
                if (index > -1)
                {
                    workLogs[index] = workLog;
                    return;
                }
            }
            else
            {
                workLogs = new List<WorkLogModel>();
                _cacheService.Set(key, workLogs, DateTime.Now.AddMinutes(_cacheDuration));
            }
            workLogs.Add(workLog);
        }
    }

    private void RemoveFromWorkLogList(string workLogId, IList<RelatedEntity> relatedEntities)
    {
        foreach (var relatedEntity in relatedEntities)
        {
            string key = string.Format(WORK_LOG_KEY, relatedEntity.EntityType, relatedEntity.EntityId);
            if (_cacheService.TryGetValue<List<WorkLogModel>>(WORK_LOG_KEY, out var workLogs))
            {
                var index = workLogs!.FindIndex(p => p.WorkLogId == workLogId);
                if (index > -1)
                {
                    workLogs.RemoveAt(index);
                }
            }
        }
    }
    #endregion
}
