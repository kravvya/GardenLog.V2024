using AutoMapper;
using GardenLog.SharedInfrastructure.Extensions;
using PlantHarvest.Domain.HarvestAggregate;

namespace PlantHarvest.Api.QueryHandlers;

public interface IHarvestQueryHandler
{
    Task<IReadOnlyCollection<HarvestCycleViewModel>> GetAllHarvestCycles();
    Task<HarvestCycleViewModel> GetHarvestCycleByHarvestCycleId(string harvestCycleId);
    Task<string> GetHarvestCycleIdByHarvestCycleName(string name);

    Task<PlantHarvestCycleViewModel> GetPlantHarvestCycle(string harvestCycleId, string id);
    Task<IReadOnlyCollection<PlantHarvestCycleViewModel>> GetPlantHarvestCycles(string harvestCycleId);
    Task<IReadOnlyCollection<PlantHarvestCycleIdentityOnlyViewModel>> GetPlantHarvestCyclesByPlantId(string plantId);
}


public class HarvestQueryHandler : IHarvestQueryHandler
{
    private readonly IHarvestCycleRepository _harvestCycleRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<HarvestQueryHandler> _logger;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public HarvestQueryHandler(IHarvestCycleRepository harvestCycleRepository, IMapper mapper, ILogger<HarvestQueryHandler> logger, IHttpContextAccessor httpContextAccessor)
    {
        _harvestCycleRepository = harvestCycleRepository;
        _mapper = mapper;
        _logger = logger;
        _httpContextAccessor = httpContextAccessor;
    }

    #region Harvest Cycle
    public async Task<IReadOnlyCollection<HarvestCycleViewModel>> GetAllHarvestCycles()
    {
        _logger.LogInformation("Received request to get all harvest cycles");
        string userProfileId = _httpContextAccessor.HttpContext?.User.GetUserProfileId(_httpContextAccessor.HttpContext.Request.Headers)!;
        return await _harvestCycleRepository.GetAllHarvestCycles(userProfileId);
    }

    public async Task<HarvestCycleViewModel> GetHarvestCycleByHarvestCycleId(string harvestCycleId)
    {
        _logger.LogInformation("Received request to get {harvestCycleId} harvest cycle", harvestCycleId);

        var harvest = await _harvestCycleRepository.GetByIdAsync(harvestCycleId);
        return _mapper.Map<HarvestCycleViewModel>(harvest);

    }

    public async Task<string> GetHarvestCycleIdByHarvestCycleName(string name)
    {
        _logger.LogInformation("Received request to get id for {name} harvest cycle", name);
        string userProfileId = _httpContextAccessor.HttpContext?.User.GetUserProfileId(_httpContextAccessor.HttpContext.Request.Headers)!;
        var harvestId = await _harvestCycleRepository.GetIdByNameAsync(name, userProfileId);
        return harvestId;
    }
    #endregion

    #region Plan Harvest Cycle

    public async Task<PlantHarvestCycleViewModel> GetPlantHarvestCycle(string harvestCycleId, string id)
    {
        _logger.LogInformation("Received request to get plant harvest cycle for {id}", id);

        try
        {
            return await _harvestCycleRepository.GetPlantHarvestCycle(harvestCycleId, id);

        }
        catch (Exception ex)
        {
            _logger.LogCritical(ex, "Exception readding platn harvest cycle for {id}", id);
            throw;
        }
    }

    public async Task<IReadOnlyCollection<PlantHarvestCycleViewModel>> GetPlantHarvestCycles(string harvestCycleId)
    {
        _logger.LogInformation("Received request to get plant harvest cycles for {harvestCycleId}", harvestCycleId);

        try
        {
            return await _harvestCycleRepository.GetPlantHarvestCycles(harvestCycleId);

        }
        catch (Exception ex)
        {
            _logger.LogCritical(ex,"Exception readding plant harvest cycles for {harvestCycleId}", harvestCycleId);
            throw;
        }
    }

    public async Task<IReadOnlyCollection<PlantHarvestCycleIdentityOnlyViewModel>> GetPlantHarvestCyclesByPlantId(string plantId)
    {
        _logger.LogInformation("Received request to get plant harvest cycles for plant {plantId}", plantId);

        try
        {
            return await _harvestCycleRepository.GetPlantHarvestCyclesByPlantId(plantId);

        }
        catch (Exception ex)
        {
            _logger.LogCritical(ex,"Exception readding plant harvest cycles for plant {plantId}", plantId);
            throw;
        }
    }
    #endregion
}