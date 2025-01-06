using GardenLog.SharedKernel.Interfaces;
using PlantHarvest.Api.Extensions;

namespace PlantHarvest.Api.CommandHandlers;


public interface IPlantTaskCommandHandler
{
    Task<string> CreatePlantTask(CreatePlantTaskCommand request);
    Task<string> DeletePlantTask(string id);
    Task<string> UpdatePlantTask(UpdatePlantTaskCommand request);
    Task<string> CompletePlantTask(UpdatePlantTaskCommand request);
    Task<int> DeleteSystemGeneratedPlantTasks(string plantHarvestCycleId);
}

public class PlantTaskCommandHandler : IPlantTaskCommandHandler
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPlantTaskRepository _taskRepository;
    private readonly ILogger<PlantTaskCommandHandler> _logger;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IMediator _mediator;

    public PlantTaskCommandHandler(IUnitOfWork unitOfWork, IPlantTaskRepository workLogRepository, ILogger<PlantTaskCommandHandler> logger, IHttpContextAccessor httpContextAccessor, IMediator mediator)
    {
        _unitOfWork = unitOfWork;
        _taskRepository = workLogRepository;
        _logger = logger;
        _httpContextAccessor = httpContextAccessor;
        _mediator = mediator;
    }

    #region Plant Task

    public async Task<string> CreatePlantTask(CreatePlantTaskCommand request)
    {
        _logger.LogInformation("Received request to create a new systemTask {request}", request);

        string userProfileId = _httpContextAccessor.HttpContext?.User.GetUserProfileId(_httpContextAccessor.HttpContext.Request.Headers)!;


        var task = PlantTask.Create(request.Title, request.Type
            , request.CreatedDateTime, request.TargetDateStart, request.TargetDateEnd, request.CompletedDateTime
            , request.HarvestCycleId, request.PlantHarvestCycleId, request.PlantName, request.PlantScheduleId, request.Notes, request.IsSystemGenerated, userProfileId);

        _taskRepository.Add(task);

        await _mediator.DispatchDomainEventsAsync(task);

        try
        {
            await _unitOfWork.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            _logger.LogCritical("Exception storing systemTask: {ex}", ex);
            throw;
        }
       

        return task.Id;
    }

    public async Task<string> UpdatePlantTask(UpdatePlantTaskCommand request)
    {
        _logger.LogInformation("Received request to update systemTask {request}", request);

         var task = await _taskRepository.GetByIdAsync(request.PlantTaskId);

        task.Update(request.TargetDateStart, request.TargetDateEnd, request.CompletedDateTime, request.Notes);

        _taskRepository.Update(task);

        await _mediator.DispatchDomainEventsAsync(task);

        await _unitOfWork.SaveChangesAsync();

        return task.Id;
    }

    public async Task<string> DeletePlantTask(string id)
    {
        _logger.LogInformation("Received request to delete systemTask {id}", id);

        var task = await _taskRepository.GetByIdAsync(id);

        task.Delete();

        _taskRepository.Delete(id);

        await _mediator.DispatchDomainEventsAsync(task);

        await _unitOfWork.SaveChangesAsync();

        return id;
    }

    public async Task<int> DeleteSystemGeneratedPlantTasks(string plantHarvestCycleId)
    {
        _logger.LogInformation("Received request to delete system generated systemTasks for plantHarvestCycleId: {id}", plantHarvestCycleId);

        string userProfileId = _httpContextAccessor.HttpContext?.User.GetUserProfileId(_httpContextAccessor.HttpContext.Request.Headers)!;

        var systemTasks = await _taskRepository.GetNotCompletedSystemGeneratedTasks(plantHarvestCycleId, userProfileId);

        foreach (var systemTask in systemTasks)
        {
            var task = await _taskRepository.GetByIdAsync(systemTask.PlantTaskId);
            task.Delete();
            _taskRepository.Delete(task.Id);
            await _mediator.DispatchDomainEventsAsync(task);
        }      

        await _unitOfWork.SaveChangesAsync();

        return systemTasks.Count();
    }

    public Task<string> CompletePlantTask(UpdatePlantTaskCommand request)
    {
        return UpdatePlantTask(request);
    }
    #endregion

}

