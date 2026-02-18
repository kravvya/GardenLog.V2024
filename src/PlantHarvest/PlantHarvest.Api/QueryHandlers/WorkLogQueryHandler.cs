using AutoMapper;
using GardenLog.SharedKernel.Enum;
using PlantHarvest.Contract.Query;

namespace PlantHarvest.Api.QueryHandlers;

public interface IWorkLogQueryHandler
{
    Task<IReadOnlyCollection<WorkLogViewModel>> GetWorkLogs(RelatedEntityTypEnum enityType, string entityId);
    Task<IReadOnlyCollection<WorkLogViewModel>> SearchWorkLogs(WorkLogSearch search);
}


public class WorkLogQueryHandler : IWorkLogQueryHandler
{
    private readonly IWorkLogRepository _workLogRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<WorkLogQueryHandler> _logger;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public WorkLogQueryHandler(IWorkLogRepository harvestCycleRepository, IMapper mapper, ILogger<WorkLogQueryHandler> logger, IHttpContextAccessor httpContextAccessor)
    {
        _workLogRepository = harvestCycleRepository;
        _mapper = mapper;
        _logger = logger;
        _httpContextAccessor = httpContextAccessor;
    }

  
    public async Task<IReadOnlyCollection<WorkLogViewModel>> GetWorkLogs(RelatedEntityTypEnum enityType, string entityId)
    {
        _logger.LogInformation("Received request to get work logs");
        string userProfileId = _httpContextAccessor.HttpContext?.User.GetUserProfileId(_httpContextAccessor.HttpContext.Request.Headers)!;
        return await _workLogRepository.GetWorkLogsByEntity(enityType, entityId, userProfileId);
    }

    public async Task<IReadOnlyCollection<WorkLogViewModel>> SearchWorkLogs(WorkLogSearch search)
    {
        _logger.LogInformation("Received request to search work logs");
        string userProfileId = _httpContextAccessor.HttpContext?.User.GetUserProfileId(_httpContextAccessor.HttpContext.Request.Headers)!;
        return await _workLogRepository.SearchWorkLogsForUser(search, userProfileId);
    }

}