using GardenLog.SharedKernel.Interfaces;
using MediatR;
using PlantHarvest.Api.Extensions;
using PlantHarvest.Domain.WorkLogAggregate;
using System.Threading.Tasks;

namespace PlantHarvest.Api.CommandHandlers;


public interface IWorkLogCommandHandler
{
    Task<string> CreateWorkLog(CreateWorkLogCommand request);
    Task<string> DeleteWorkLog(string id);
    Task<string> UpdateWorkLog(UpdateWorkLogCommand request);
}

public class WorkLogCommandHandler : IWorkLogCommandHandler
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IWorkLogRepository _workLogRepository;
    private readonly ILogger<WorkLogCommandHandler> _logger;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IMediator _mediator;

    public WorkLogCommandHandler(IUnitOfWork unitOfWork, IWorkLogRepository workLogRepository, ILogger<WorkLogCommandHandler> logger, IHttpContextAccessor httpContextAccessor, IMediator mediator)
    {
        _unitOfWork = unitOfWork;
        _workLogRepository = workLogRepository;
        _logger = logger;
        _httpContextAccessor = httpContextAccessor;
        _mediator = mediator;
    }

    #region Work log

    public async Task<string> CreateWorkLog(CreateWorkLogCommand request)
    {
        _logger.LogInformation("Received request to create a new worklog {0}", request);

        string userProfileId = _httpContextAccessor.HttpContext?.User.GetUserProfileId(_httpContextAccessor.HttpContext.Request.Headers)!;


        var workLog = WorkLog.Create(
                 log: request.Log,
                 eventDateTime: request.EventDateTime,
                 reason: request.Reason,
                 userProfileId: userProfileId,
                 relatedEntities: request.RelatedEntities);

        _workLogRepository.Add(workLog);

        await _mediator.DispatchDomainEventsAsync(workLog);

        await _unitOfWork.SaveChangesAsync();

        return workLog.Id;
    }

    public async Task<string> UpdateWorkLog(UpdateWorkLogCommand request)
    {
        _logger.LogInformation("Received request to update work log {0}", request);

        string userProfileId = _httpContextAccessor.HttpContext?.User.GetUserProfileId(_httpContextAccessor.HttpContext.Request.Headers)!;

       var workLog = await _workLogRepository.GetByIdAsync(request.WorkLogId);

        workLog.Update(request.Log, request.EventDateTime, request.Reason);

        _workLogRepository.Update(workLog);

        await _mediator.DispatchDomainEventsAsync(workLog);

        await _unitOfWork.SaveChangesAsync();

        return workLog.Id;
    }

    public async Task<string> DeleteWorkLog(string id)
    {
        _logger.LogInformation("Received request to delete work log {0}", id);

        var workLog = await _workLogRepository.GetByIdAsync(id);

        workLog.Delete();

        _workLogRepository.Delete(id);

        await _mediator.DispatchDomainEventsAsync(workLog);

        await _unitOfWork.SaveChangesAsync();

        return id;
    }
    #endregion

}

