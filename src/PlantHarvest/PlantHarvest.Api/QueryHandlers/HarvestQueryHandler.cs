using AutoMapper;
using GardenLog.SharedInfrastructure.Extensions;
using PlantHarvest.Contract.Query;
using PlantHarvest.Domain.HarvestAggregate;
using PlantHarvest.Domain.WorkLogAggregate;
using PlantHarvest.Infrastructure.Data.Repositories;

namespace PlantHarvest.Api.QueryHandlers;

public interface IHarvestQueryHandler
{
    Task<IReadOnlyCollection<HarvestCycleViewModel>> GetAllHarvestCycles();
    Task<IReadOnlyCollection<GardenBedPlantHarvestCycleViewModel>> GetGardenBedViewsByGardenBedId(string gardenId, string gardenBedId);
    Task<HarvestCycleViewModel> GetHarvestCycleByHarvestCycleId(string harvestCycleId);
    Task<string> GetHarvestCycleIdByHarvestCycleName(string name);

    Task<PlantHarvestCycleViewModel> GetPlantHarvestCycle(string harvestCycleId, string id);
    Task<IReadOnlyCollection<PlantHarvestCycleViewModel>> GetPlantHarvestCycles(string harvestCycleId);
    Task<IReadOnlyCollection<PlantHarvestCycleIdentityOnlyViewModel>> GetPlantHarvestCyclesByPlantId(string plantId);
    Task<IReadOnlyCollection<PlantHarvestCycleViewModel>> SearchPlantHarvestCycles(PlantHarvestCycleSearch search);
}


public class HarvestQueryHandler : IHarvestQueryHandler
{
    private readonly IHarvestCycleRepository _harvestCycleRepository;
    private readonly IPlantHarvestCycleRepository _plantHarvestCycleRepository;
    private readonly IGardenBedPlantHarvestCycleRepository _gardenBedPlantHarvestCycleRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<HarvestQueryHandler> _logger;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public HarvestQueryHandler(IHarvestCycleRepository harvestCycleRepository, IPlantHarvestCycleRepository plantHarvestCycleRepository, IGardenBedPlantHarvestCycleRepository gardenBedPlantHarvestCycleRepository, IMapper mapper, ILogger<HarvestQueryHandler> logger, IHttpContextAccessor httpContextAccessor)
    {
        _harvestCycleRepository = harvestCycleRepository;
        _plantHarvestCycleRepository = plantHarvestCycleRepository;
        _gardenBedPlantHarvestCycleRepository = gardenBedPlantHarvestCycleRepository;
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
            var plantTask = _plantHarvestCycleRepository.GetPlantHarvestCycleView(harvestCycleId, id);
            var gardenBedsTask = _gardenBedPlantHarvestCycleRepository.GetGardenBedViewsByPlantHarvestCycleId(harvestCycleId, id);

            await Task.WhenAll(plantTask, gardenBedsTask);

            var plant = plantTask.Result;
            plant.GardenBedLayout.AddRange(gardenBedsTask.Result);

            return plant;

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
            var plantsTasks = _plantHarvestCycleRepository.GetPlantHarvestCycleViews(harvestCycleId);
            var gardenBedsTask = _gardenBedPlantHarvestCycleRepository.GetGardenBedViewsByHarvestCycleId(harvestCycleId);

            await Task.WhenAll(plantsTasks, gardenBedsTask);

            var plants = plantsTasks.Result;

            foreach (var plant in plants)
            {
                plant.GardenBedLayout.AddRange(gardenBedsTask.Result.Where(g => g.PlantHarvestCycleId == plant.PlantHarvestCycleId));
            }

            return plants;

        }
        catch (Exception ex)
        {
            _logger.LogCritical(ex, "Exception readding plant harvest cycles for {harvestCycleId}", harvestCycleId);
            throw;
        }
    }

    public async Task<IReadOnlyCollection<PlantHarvestCycleIdentityOnlyViewModel>> GetPlantHarvestCyclesByPlantId(string plantId)
    {
        _logger.LogInformation("Received request to get plant harvest cycles for plant {plantId}", plantId);

        try
        {
            return await _plantHarvestCycleRepository.GetPlantHarvestCyclesByPlantId(plantId);

        }
        catch (Exception ex)
        {
            _logger.LogCritical(ex, "Exception readding plant harvest cycles for plant {plantId}", plantId);
            throw;
        }
    }

    public async Task<IReadOnlyCollection<PlantHarvestCycleViewModel>> SearchPlantHarvestCycles(PlantHarvestCycleSearch search)
    {
        _logger.LogInformation("Received request to search plant harvest cycles");
        string userProfileId = _httpContextAccessor.HttpContext?.User.GetUserProfileId(_httpContextAccessor.HttpContext.Request.Headers)!;

        try
        {
            return await _plantHarvestCycleRepository.SearchPlantHarvestCyclesForUser(search, userProfileId);
        }
        catch (Exception ex)
        {
            _logger.LogCritical(ex, "Exception searching plant harvest cycles");
            throw;
        }
    }
    #endregion

    #region Garden Bed Usage
    public async Task<IReadOnlyCollection<GardenBedPlantHarvestCycleViewModel>> GetGardenBedViewsByGardenBedId(string gardenId, string gardenBedId)
    {
        _logger.LogInformation("Received request to get garden bed usage {gardenBedId}", gardenBedId);

        try
        {
            return await _gardenBedPlantHarvestCycleRepository.GetGardenBedViewsByGardenBedId(gardenId, gardenBedId);
        }
        catch (Exception ex)
        {
            _logger.LogCritical(ex, "Exception readding garden bed usage {gardenBedId}", gardenBedId);
            throw;
        }         
    }
    #endregion
}