using GardenLog.SharedInfrastructure.Extensions;

namespace UserManagement.CommandHandlers;

public interface IGardenCommandHandler
{
    Task<string> CreateGarden(CreateGardenCommand request);
    Task<string> CreateGardenBed(CreateGardenBedCommand request);
    Task<int> DeleteGarden(string id);
    Task<int> DeleteGardenBed(string gardenId, string gardenBedId);
    Task<int> UpdateGarden(UpdateGardenCommand request);
    Task<int> UpdateGardenBed(UpdateGardenBedCommand request);
}

public class GardenCommandHandler : IGardenCommandHandler
{
    private readonly IGardenRepository _gardenRepository;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<GardenCommandHandler> _logger;

    public GardenCommandHandler(IGardenRepository plantLocationRepository, IHttpContextAccessor httpContextAccessor, IUnitOfWork unitOfWork, ILogger<GardenCommandHandler> logger)
    {
        _gardenRepository = plantLocationRepository;
        _httpContextAccessor = httpContextAccessor;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<string> CreateGarden(CreateGardenCommand request)
    {
        var userProfileId = _httpContextAccessor.HttpContext!.User.GetUserProfileId(_httpContextAccessor.HttpContext.Request.Headers)!;

        var existingGardenId = await _gardenRepository.GetIdByNameAsync(request.Name, userProfileId);
        if (!string.IsNullOrEmpty(existingGardenId))
        {
            throw new ArgumentException("Garden with this name already exists", nameof(request.Name));
        }

        var garden = Garden.Create(
            request.Name,
            request.City,
            request.StateCode,
            request.Latitude,
            request.Longitude,
            request.Notes,
            userProfileId,
            request.LastFrostDate,
            request.FirstFrostDate,
            request.WarmSoilDate,
            request.Length,
            request.Width);

        _gardenRepository.Add(garden);

        try
        {
            await _unitOfWork.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError("Exception creating garden: {ex}s", ex);
            throw;
        }

        return garden.Id;
    }

    public async Task<int> UpdateGarden(UpdateGardenCommand request)
    {

        var garden = await _gardenRepository.GetByIdAsync(request.GardenId);
        if (garden == null) return 0;

        garden.Update(request.Name, request.City, request.StateCode
            , request.Latitude, request.Longitude
            , request.Notes, request.LastFrostDate, request.FirstFrostDate, request.WarmSoilDate
            , request.Length, request.Width);

        _gardenRepository.Update(garden);

        try
        {
            return await _unitOfWork.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError("Exception creating garden: {ex}", ex);
            throw;
        }

    }

    public async Task<int> DeleteGarden(string id)
    {
        _gardenRepository.Delete(id);
        return await _unitOfWork.SaveChangesAsync();
    }

    public async Task<string> CreateGardenBed(CreateGardenBedCommand request)
    {
        var garden = await _gardenRepository.GetByIdAsync(request.GardenId);

        if(garden.GardenBeds.FirstOrDefault(g => g.Name== request.Name) != null)
        {
            throw new ArgumentException("Garden bed with this name already exists", nameof(request.Name));
        }

        var id = garden.AddGardenBed(request);

        _gardenRepository.AddGardenBed(id, garden);

        await _unitOfWork.SaveChangesAsync();

        return id;
    }

    public async Task<int> UpdateGardenBed(UpdateGardenBedCommand request)
    {
        var garden = await _gardenRepository.GetByIdAsync(request.GardenId);

        garden.UpdateGardenBed(request);

        _gardenRepository.UpdateGardenBed(request.GardenBedId, garden);

        return await _unitOfWork.SaveChangesAsync();

    }

    public async Task<int> DeleteGardenBed(string gardenId, string gardenBedId)
    {
        try
        {
            var garden = await _gardenRepository.GetByIdAsync(gardenId);

            garden.DeleteGardenBed(gardenBedId);

            _gardenRepository.DeleteGardenBed(gardenBedId, garden);

            return await _unitOfWork.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError("Exception creating garden: {ex}", ex);
            throw;
        }
       
    }
}
