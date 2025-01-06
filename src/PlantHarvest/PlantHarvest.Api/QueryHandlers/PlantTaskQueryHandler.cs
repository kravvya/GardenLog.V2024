using PlantHarvest.Contract.Query;

namespace PlantHarvest.Api.QueryHandlers;


public interface IPlantTaskQueryHandler
{
    Task<IReadOnlyCollection<PlantTaskViewModel>> GetPlantTasks();
    Task<IReadOnlyCollection<PlantTaskViewModel>> GetActivePlantTasks();
    Task<IReadOnlyCollection<PlantTaskViewModel>> SearchPlantTasks(PlantTaskSearch search);
    Task<long> GetCompletedTaskCount(string harvestCycleId);
    Task<IReadOnlyCollection<PlantTaskViewModel>> GetNotCompletedSystemGeneratedTasks(string plantHarvestCycleId);
}


public class PlantTaskQueryHandler : IPlantTaskQueryHandler
{
    private readonly IPlantTaskRepository _taskRepository;
    private readonly ILogger<PlantTaskQueryHandler> _logger;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public PlantTaskQueryHandler(IPlantTaskRepository taskRepository, ILogger<PlantTaskQueryHandler> logger, IHttpContextAccessor httpContextAccessor)
    {
        _taskRepository = taskRepository;
        _logger = logger;
        _httpContextAccessor = httpContextAccessor;
    }


    public async Task<IReadOnlyCollection<PlantTaskViewModel>> GetPlantTasks()
    {
        _logger.LogInformation("Received request to get all tasks");
        string userProfileId = _httpContextAccessor.HttpContext?.User.GetUserProfileId(_httpContextAccessor.HttpContext.Request.Headers)!;
        return await _taskRepository.GetPlantTasksForUser(userProfileId);
    }

    public async Task<IReadOnlyCollection<PlantTaskViewModel>> GetActivePlantTasks()
    {
        _logger.LogInformation("Received request to get all tasks");
        string userProfileId = _httpContextAccessor.HttpContext?.User.GetUserProfileId(_httpContextAccessor.HttpContext.Request.Headers)!;
        return await _taskRepository.GetActivePlantTasksForUser(userProfileId);
    }

    public async Task<IReadOnlyCollection<PlantTaskViewModel>> SearchPlantTasks(PlantTaskSearch search)
    {
        _logger.LogInformation("Received request to search for tasks {search}", search);
        string userProfileId = _httpContextAccessor.HttpContext?.User.GetUserProfileId(_httpContextAccessor.HttpContext.Request.Headers)!;
        return await _taskRepository.SearchPlantTasksForUser(search, userProfileId);
    }

    public async Task<IReadOnlyCollection<PlantTaskViewModel>> GetNotCompletedSystemGeneratedTasks(string plantHarvestCycleId)
    {
        _logger.LogInformation("Received request for all not completed system generated tasks");
        string userProfileId = _httpContextAccessor.HttpContext?.User.GetUserProfileId(_httpContextAccessor.HttpContext.Request.Headers)!;
        return await _taskRepository.GetNotCompletedSystemGeneratedTasks(plantHarvestCycleId, userProfileId);
    }

    public async Task<long> GetCompletedTaskCount(string harvestCycleId)
    {
        _logger.LogInformation("Received request to get count of completed tasks");

        if (_httpContextAccessor.HttpContext == null || _httpContextAccessor.HttpContext?.User == null) return 0;
        string userProfileId = _httpContextAccessor.HttpContext.User.GetUserProfileId(_httpContextAccessor.HttpContext.Request.Headers)!;

        return await _taskRepository.GetNumberOfCompletedTasksForUser(userProfileId, harvestCycleId);
    }
}