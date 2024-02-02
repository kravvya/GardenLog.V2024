

namespace GardenLogWeb.Services;

public interface IPlantTaskService
{
    Task<ApiResponse> CompletePlantTask(PlantTaskModel task);
    Task<ApiObjectResponse<string>> CreatePlantTask(PlantTaskModel task);
    Task<List<PlantTaskModel>> GetActivePlantTasks(bool forceRefresh);
    Task<long> GetNumberOfCompletedTasks(string harvestCycleId);
    Task<List<PlantTaskModel>> GetPlantTasks(bool forceRefresh);
    Task<ApiResponse> UpdatePlantTask(PlantTaskModel task);
}

public class PlantTaskService : IPlantTaskService
{
    private readonly ILogger<PlantTaskService> _logger;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ICacheService _cacheService;
    private readonly IGardenLogToastService _toastService;
    private readonly int _cacheDuration;
    private const string PLANT_TASK_KEY = "PlantTasks";
    private const string PLANT_ACTIVE_TASK_KEY = "ActivePlantTasks";

    public PlantTaskService(ILogger<PlantTaskService> logger, IHttpClientFactory clientFactory, ICacheService cacheService, IGardenLogToastService toastService, IConfiguration configuration)
    {
        _logger = logger;
        _httpClientFactory = clientFactory;
        _cacheService = cacheService;
        _toastService = toastService;
        if (!int.TryParse(configuration[GlobalConstants.GLOBAL_CACHE_DURATION], out _cacheDuration)) _cacheDuration = 10;
    }

    #region Public Plant Task Functions

    public async Task<List<PlantTaskModel>> GetPlantTasks(bool forceRefresh)
    {

        if (forceRefresh || !_cacheService.TryGetValue<List<PlantTaskModel>>(PLANT_TASK_KEY, out List<PlantTaskModel>? tasks))
        {
            _logger.LogInformation("Tasks not in cache or forceRefresh");

            tasks = await GetPlantTasks();

            // Save data in cache.
            _cacheService.Set(PLANT_TASK_KEY, tasks, DateTime.Now.AddMinutes(_cacheDuration));
        }

        else
        {
            _logger.LogInformation($"Tasks are in cache. Found {tasks!.Count}");
        }

        return tasks;
    }

    public async Task<List<PlantTaskModel>> GetActivePlantTasks(bool forceRefresh)
    {

        if (forceRefresh || !_cacheService.TryGetValue<List<PlantTaskModel>>(PLANT_ACTIVE_TASK_KEY, out List<PlantTaskModel>? tasks))
        {
            _logger.LogInformation("Active tasks not in cache or forceRefresh");

            tasks = await GetActivePlantTasks();

            // Save data in cache.
            _cacheService.Set(PLANT_ACTIVE_TASK_KEY, tasks, DateTime.Now.AddMinutes(_cacheDuration));
        }

        else
        {
            _logger.LogInformation("Active tasks are in cache. Found {@count}", tasks!.Count);
        }

        return tasks;
    }

    public async Task<ApiObjectResponse<string>> CreatePlantTask(PlantTaskModel task)
    {
        var httpClient = _httpClientFactory.CreateClient(GlobalConstants.PLANTHARVEST_API);

        var response = await httpClient.ApiPostAsync(HarvestRoutes.CreateTask, task);

        if (response.ValidationProblems != null)
        {
            _toastService.ShowToast($"Unable to create a Task. Please resolve validatione errors and try again.", GardenLogToastLevel.Error);
        }
        else if (!response.IsSuccess)
        {
            _toastService.ShowToast($"Received an invalid response from Task post: {response.ErrorMessage}", GardenLogToastLevel.Error);
        }
        else
        {
            task.PlantTaskId = response.Response!;

            AddOrUpdateToPlantTaskLists(task);

            _toastService.ShowToast($"Task saved", GardenLogToastLevel.Success);
        }

        return response;
    }

    public async Task<ApiResponse> UpdatePlantTask(PlantTaskModel task)
    {
        var httpClient = _httpClientFactory.CreateClient(GlobalConstants.PLANTHARVEST_API);

        var response = await httpClient.ApiPutAsync(HarvestRoutes.UpdateTask.Replace("{id}", task.PlantTaskId), task);

        if (response.ValidationProblems != null)
        {
            _toastService.ShowToast($"Unable to update Task. Please resolve validatione errors and try again.", GardenLogToastLevel.Error);
        }
        else if (!response.IsSuccess)
        {
            _toastService.ShowToast($"Received an invalid response: {response.ErrorMessage}", GardenLogToastLevel.Error);
        }
        else
        {
            AddOrUpdateToPlantTaskLists(task);

            _toastService.ShowToast($"Task successfully saved.", GardenLogToastLevel.Success);
        }

        return response;
    }

    public async Task<ApiResponse> CompletePlantTask(PlantTaskModel task)
    {
        var httpClient = _httpClientFactory.CreateClient(GlobalConstants.PLANTHARVEST_API);

        var response = await httpClient.ApiPutAsync(HarvestRoutes.CompleteTask.Replace("{id}", task.PlantTaskId), task);

        if (response.ValidationProblems != null)
        {
            _toastService.ShowToast($"Unable to update Task. Please resolve validatione errors and try again.", GardenLogToastLevel.Error);
        }
        else if (!response.IsSuccess)
        {
            _toastService.ShowToast($"Received an invalid response: {response.ErrorMessage}", GardenLogToastLevel.Error);
        }
        else
        {
            AddOrUpdateToPlantTaskLists(task);
            RemoveTaskFromActiveTaskList(task);

            _toastService.ShowToast($"Task successfully saved.", GardenLogToastLevel.Success);
        }

        return response;
    }

    public async Task<long> GetNumberOfCompletedTasks(string harvestCycleId)
    {
        var httpClient = _httpClientFactory.CreateClient(GlobalConstants.PLANTHARVEST_API);

        var response = await httpClient.ApiGetAsync<long>(HarvestRoutes.GetCompleteTaskCount.Replace("{harvestId}", harvestCycleId), _logger);

        if (!response.IsSuccess)
        {
            _toastService.ShowToast("Unable to get Completed Tasks", GardenLogToastLevel.Error);
            return 0;
        }

        return response.Response;
    }
    #endregion


    #region Private Plant Task Functions
    private async Task<List<PlantTaskModel>> GetPlantTasks()
    {
        var httpClient = _httpClientFactory.CreateClient(GlobalConstants.PLANTHARVEST_API);

        var response = await httpClient.ApiGetAsync<List<PlantTaskModel>>(HarvestRoutes.GetTasks, _logger);

        if (!response.IsSuccess)
        {
            _toastService.ShowToast("Unable to get Tasks", GardenLogToastLevel.Error);
            return new List<PlantTaskModel>();
        }

        return response.Response!;
    }

    private async Task<List<PlantTaskModel>> GetActivePlantTasks()
    {
        var httpClient = _httpClientFactory.CreateClient(GlobalConstants.PLANTHARVEST_API);

        var response = await httpClient.ApiGetAsync<List<PlantTaskModel>>(HarvestRoutes.GetActiveTasks, _logger);

        if (!response.IsSuccess)
        {
            _toastService.ShowToast("Unable to get Active Tasks", GardenLogToastLevel.Error);
            return new List<PlantTaskModel>();
        }

        return response.Response!;
    }

    private void AddOrUpdateToPlantTaskLists(PlantTaskModel task)
    {


        if (_cacheService.TryGetValue<List<PlantTaskModel>>(PLANT_TASK_KEY, out List<PlantTaskModel>? tasks))
        {
            var index = tasks!.FindIndex(p => p.PlantTaskId == task.PlantTaskId);
            if (index > -1)
            {
                tasks[index] = task;
                return;
            }
            tasks.Add(task);
        }

        if (!task.CompletedDateTime.HasValue)
        {
            tasks = null;

            if (_cacheService.TryGetValue<List<PlantTaskModel>>(PLANT_ACTIVE_TASK_KEY, out tasks))
            {
                var index = tasks!.FindIndex(p => p.PlantTaskId == task.PlantTaskId);
                if (index > -1)
                {
                    tasks[index] = task;
                    return;
                }
                tasks.Add(task);
            }
        }

    }

    private void RemoveTaskFromActiveTaskList(PlantTaskModel task)
    {
        if (_cacheService.TryGetValue<List<PlantTaskModel>>(PLANT_ACTIVE_TASK_KEY, out var tasks))
        {
            var index = tasks!.FindIndex(p => p.PlantTaskId == task.PlantTaskId);
            if (index > -1)
            {
                tasks.RemoveAt(index);
            }
        }
    }
    #endregion
}
