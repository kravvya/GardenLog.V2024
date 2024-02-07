using GardenLog.SharedInfrastructure.Extensions;
using UserManagement.Api.Data.ApiClients;

namespace UserManagement.CommandHandlers;

public interface IGardenCommandHandler
{
    Task<string> CreateGarden(CreateGardenCommand request);
    Task<string> CreateGardenBed(CreateGardenBedCommand request);
    Task<string?> CreateWeatherstation(CreateWeatherstationCommand request);
    Task<int> DeleteGarden(string id);
    Task<int> DeleteGardenBed(string gardenId, string gardenBedId);
    Task<int> UpdateGarden(UpdateGardenCommand request);
    Task<int> UpdateGardenBed(UpdateGardenBedCommand request);
}

public class GardenCommandHandler : IGardenCommandHandler
{
    private readonly IGardenRepository _gardenRepository;
    private readonly IWeatherstationRepository _weatherstationRepository;
    private readonly IGrowConditionsApiClient _growConditionsApiClient;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<GardenCommandHandler> _logger;

    public GardenCommandHandler(IGardenRepository plantLocationRepository, IWeatherstationRepository weatherstationRepository, IGrowConditionsApiClient growConditionsApiClient, IHttpContextAccessor httpContextAccessor, IUnitOfWork unitOfWork, ILogger<GardenCommandHandler> logger)
    {
        _gardenRepository = plantLocationRepository;
        _weatherstationRepository = weatherstationRepository;
        _growConditionsApiClient = growConditionsApiClient;
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

        var weatherstation = await _growConditionsApiClient.GetWeatherStation(request.Latitude, request.Longitude);
        if (weatherstation == null)
        {
            throw new ArgumentException("Weather station not found", nameof(request.Latitude));
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

        garden.SetWeatherstation(weatherstation.ForecastOffice, weatherstation.GridX, weatherstation.GridY, weatherstation.Timezone);

        _gardenRepository.Add(garden);
        _weatherstationRepository.AddWeatherstation(garden);

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
        var userProfileId = _httpContextAccessor.HttpContext!.User.GetUserProfileId(_httpContextAccessor.HttpContext.Request.Headers)!;

        var garden = await _gardenRepository.ReadGarden(request.GardenId, userProfileId);
        if (garden == null) return 0;

        var weatherstation = await _growConditionsApiClient.GetWeatherStation(request.Latitude, request.Longitude);
        if (weatherstation == null)
        {
            throw new ArgumentException("Weather station not found", nameof(request.Latitude));
        }

        garden.Update(request.Name, request.City, request.StateCode
            , request.Latitude, request.Longitude
            , request.Notes, request.LastFrostDate, request.FirstFrostDate, request.WarmSoilDate
            , request.Length, request.Width);

        garden.SetWeatherstation(weatherstation.ForecastOffice, weatherstation.GridX, weatherstation.GridY, weatherstation.Timezone);

        _gardenRepository.Update(garden);

        if (garden.Weatherstation!.IsModified)
        {
            _weatherstationRepository.UpdateWeatherstation(garden);
        }
        else
        {
            _weatherstationRepository.AddWeatherstation(garden);
        }

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

    //this route is to deal with exception where for some reason garden has no weatherstation set up. In this case, Grow Conditions Api will call this route during forecast request
    public async Task<string?> CreateWeatherstation(CreateWeatherstationCommand request)
    {

        var garden = await _gardenRepository.GetByIdAsync(request.GardenId);
        if (garden == null) return null;

        var weatherstation = await _weatherstationRepository.ReadWeatherstation(request.GardenId);
        if(weatherstation != null) garden.RehidrateWeatherstation(weatherstation);

        garden.SetWeatherstation(request.ForecastOffice, request.GridX, request.GridY, request.Timezone);

        if (weatherstation != null)
        {
            _weatherstationRepository.UpdateWeatherstation(garden);
        }
        else
        {
            _weatherstationRepository.AddWeatherstation(garden);
        }

        try
        {
            await _unitOfWork.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError("Exception saving weather station: {ex}", ex);
            throw;
        }

        return garden.Weatherstation!.Id;
    }

    public async Task<int> DeleteGarden(string id)
    {
        _gardenRepository.Delete(id);
        _weatherstationRepository.DeleteWeatherstation(id);
        return await _unitOfWork.SaveChangesAsync();
    }

    public async Task<string> CreateGardenBed(CreateGardenBedCommand request)
    {
        var garden = await _gardenRepository.GetByIdAsync(request.GardenId);

        if (garden.GardenBeds.FirstOrDefault(g => g.Name == request.Name) != null)
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
