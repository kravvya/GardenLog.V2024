using AutoMapper;
using PlantCatalog.Domain.PlantAggregate;

namespace PlantCatalog.Api.QueryHandlers;

public interface IPlantQueryHandler
{
    Task<IReadOnlyCollection<PlantViewModel>> GetAllPlants();
    Task<IReadOnlyCollection<PlantNameOnlyViewModel>> GetAllPlantNames();
    Task<PlantViewModel> GetPlantByPlantId(string plantId);
    Task<PlantGrowInstructionViewModel> GetPlantGrowInstruction(string plantId, string id);
    Task<IReadOnlyCollection<PlantGrowInstructionViewModel>> GetPlantGrowInstructions(string plantId);
    Task<string> GetPlantIdByPlantName(string nane);
    Task<IReadOnlyCollection<PlantVarietyViewModel>> GetPlantVarieties(string plantId);
    Task<IReadOnlyCollection<PlantVarietyViewModel>> GetPlantVarieties();
    Task<PlantVarietyViewModel> GetPlantVariety(string plantId, string id);
}

public class PlantQueryHandler : IPlantQueryHandler
{
    private readonly IPlantRepository _plantRepository;
    private readonly IPlantVarietyRepository _varietyRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<PlantQueryHandler> _logger;

    public PlantQueryHandler(IPlantRepository repository, IPlantVarietyRepository varietyRepository, IMapper mapper, ILogger<PlantQueryHandler> logger)
    {
        _plantRepository = repository;
        _varietyRepository = varietyRepository;
        _mapper = mapper;
        _logger = logger;
    }

    #region Plant
    public async Task<PlantViewModel> GetPlantByPlantId(string plantId)
    {
        _logger.LogInformation($"Received request to get plant by plantid: {plantId}");
        var plant = await _plantRepository.GetByIdAsync(plantId);

        return _mapper.Map<PlantViewModel>(plant);
    }

    public async Task<string> GetPlantIdByPlantName(string nane)
    {
        _logger.LogInformation($"Received request to get plant id by plant name: {nane}");
        return await _plantRepository.GetIdByNameAsync(nane);
    }

    public async Task<IReadOnlyCollection<PlantViewModel>> GetAllPlants()
    {
        _logger.LogInformation("Received request to get all plants");

        return await _plantRepository.GetAllPlants();
    }

    public async Task<IReadOnlyCollection<PlantNameOnlyViewModel>> GetAllPlantNames()
    {
        _logger.LogInformation("Received request to get all plant names");

        return await _plantRepository.GetAllPlantNames();
    }
    #endregion

    #region Plant Grow Instruction

    public async Task<IReadOnlyCollection<PlantGrowInstructionViewModel>> GetPlantGrowInstructions(string plantId)
    {
        _logger.LogInformation($"Received request to get plant grow instructions for {plantId}");
     
        try
        {
            return await _plantRepository.GetPlantGrowInstractions(plantId);
          
        }
        catch (Exception ex)
        {
            _logger.LogCritical($"Exception readding grow instructions for {plantId}", ex);
            throw;
        }
      
    }

    public async Task<PlantGrowInstructionViewModel> GetPlantGrowInstruction(string plantId, string id)
    {
        _logger.LogInformation($"Received request to get plant grow instruction for {plantId} and {id}");

        try
        {
            return  await _plantRepository.GetPlantGrowInstraction(plantId, id);
        }
        catch (Exception ex)
        {
            _logger.LogCritical($"Exception readding grow instruction for {plantId} and {id}", ex);
            throw;
        }

    }

    #endregion

    #region Plant Variety

    public async Task<IReadOnlyCollection<PlantVarietyViewModel>> GetPlantVarieties()
    {
        _logger.LogInformation($"Received request to get all plant varieties");

        try
        {
            return await _varietyRepository.GetPlantVarieties();

        }
        catch (Exception ex)
        {
            _logger.LogCritical($"Exception readding all plant varieties", ex);
            throw;
        }

    }

    public async Task<IReadOnlyCollection<PlantVarietyViewModel>> GetPlantVarieties(string plantId)
    {
        _logger.LogInformation($"Received request to get plant varieties for {plantId}");

        try
        {
            return await _varietyRepository.GetPlantVarieties(plantId);

        }
        catch (Exception ex)
        {
            _logger.LogCritical($"Exception readding plant varieties for {plantId}", ex);
            throw;
        }

    }

    public async Task<PlantVarietyViewModel> GetPlantVariety(string plantId, string id)
    {
        _logger.LogInformation($"Received request to get plant variety for {plantId} and {id}");

        try
        {
            var variety =  await _varietyRepository.GetByIdAsync(id);
            return _mapper.Map<PlantVarietyViewModel>(variety);

        }
        catch (Exception ex)
        {
            _logger.LogCritical($"Exception readding grow instruction for {plantId} and {id}", ex);
            throw;
        }

    }

    #endregion
}
