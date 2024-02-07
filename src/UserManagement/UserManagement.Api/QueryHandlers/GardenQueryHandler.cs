using GardenLog.SharedInfrastructure.Extensions;

namespace UserManagement.QueryHandlers;

public interface IGardenQueryHandler
{
    Task<GardenViewModel> GetGarden(string id);
    Task<GardenBedViewModel?> GetGardenBed(string gardenId, string id);
    Task<IReadOnlyCollection<GardenBedViewModel>> GetGardenBeds(string id);
    Task<GardenViewModel> GetGardenByName(string gardenName);
    Task<IReadOnlyCollection<GardenViewModel>> GetGardens();
    Task<IReadOnlyCollection<GardenViewModel>> GetAllGardens();
    Task<WeatherstationViewModel> GetWeatherstation(string gardenId);
}

public class GardenQueryHandler : IGardenQueryHandler
{
    private readonly IGardenRepository _gardenRepository;
    private readonly IWeatherstationRepository _weatherstationRepository;
    private readonly IHttpContextAccessor _httpContextAccessor;
    
    public GardenQueryHandler(IGardenRepository plantLocationRepository, IWeatherstationRepository weatherstationRepository,  IHttpContextAccessor httpContextAccessor)
    {
        _gardenRepository = plantLocationRepository;
        _weatherstationRepository = weatherstationRepository;
        _httpContextAccessor = httpContextAccessor;

    }
    public Task<GardenViewModel> GetGarden(string id) => _gardenRepository.GetGarden(id);

    public Task<GardenViewModel> GetGardenByName(string gardenName) => _gardenRepository.GetGardenByName(gardenName, _httpContextAccessor.HttpContext!.User.GetUserProfileId(_httpContextAccessor.HttpContext.Request.Headers));

    public Task<IReadOnlyCollection<GardenBedViewModel>> GetGardenBeds(string id) => _gardenRepository.GetGardenBeds(id);

    public Task<GardenBedViewModel?> GetGardenBed(string gardenId, string id) => _gardenRepository.GetGardenBed(gardenId, id);

    public Task<IReadOnlyCollection<GardenViewModel>> GetGardens() => _gardenRepository.GetGardens(_httpContextAccessor.HttpContext!.User.GetUserProfileId(_httpContextAccessor.HttpContext.Request.Headers));

    public Task<IReadOnlyCollection<GardenViewModel>> GetAllGardens() => _gardenRepository.GetAllGardens();

    public Task<WeatherstationViewModel> GetWeatherstation(string gardenId) => _weatherstationRepository.GetWeatherstation(gardenId);
}
