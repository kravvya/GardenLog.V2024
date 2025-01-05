using GrowConditions.Api.Data.ApiClients;
using GrowConditions.Api.Data.Repositories;
using GrowConditions.Contract.ViewModels;

namespace GrowConditions.Api.QueryHandlers;

public interface IWeatherQueryHandler
{
    Task<WeatherForecastViewModel?> GetForecast(string gardenId);
    Task<IList<WeatherUpdateViewModel>> GetHistoryOfWeatherUpdates(string gardenId, int numberOfDays);
    Task<WeatherUpdateViewModel> GetLastWeatherUpdate(string gardenId);
    Task<WeatherstationViewModel?> GetWeatherStation(decimal latitude, decimal longitude);
    Task<WeatherstationViewModel?> GetWeatherStation(string gardenId);
}

public class WeatherQueryHandler : IWeatherQueryHandler
{
    private readonly IWeatherRepository _weatherRepository;
    private readonly INationalWeatherServiceApiClient _nationalWeatherServiceApiClient;
    private readonly IUserManagementApiClient _userManagementApiClient;

    public WeatherQueryHandler(IWeatherRepository weatherRepository, INationalWeatherServiceApiClient nationalWeatherServiceApiClient, IUserManagementApiClient userManagementApiClient)
    {
        _weatherRepository = weatherRepository;
        _nationalWeatherServiceApiClient = nationalWeatherServiceApiClient;
        _userManagementApiClient = userManagementApiClient;
    }

    public async Task<WeatherUpdateViewModel> GetLastWeatherUpdate(string gardenId) => await _weatherRepository.GetLastWeatherUpdate(gardenId);

    public async Task<IList<WeatherUpdateViewModel>> GetHistoryOfWeatherUpdates(string gardenId, int numberOfDays) => await _weatherRepository.GetHistoryOfWeatherUpdates(gardenId, numberOfDays);

    public async Task<WeatherstationViewModel?> GetWeatherStation(decimal latitude, decimal longitude) => await _nationalWeatherServiceApiClient.GetWeatherStation(latitude, longitude);

    public async Task<WeatherstationViewModel?> GetWeatherStation(string gardenId)
    {

        var weatherstation = await _userManagementApiClient.GetWeatherstation(gardenId);

        if (weatherstation == null)
        {
            var garden = await _userManagementApiClient.GetGarden(gardenId);

            if (garden == null)
            {
                return null;
            }

            weatherstation = await GetWeatherStation(garden.Latitude, garden.Longitude);
            if (weatherstation == null)
            {
                return null;
            }

            //found weahterstation. lets set it on the garden for next time
            await _userManagementApiClient.SetWeatherstation(gardenId, weatherstation);
        }
                  
        return weatherstation;
    }

    public async Task<WeatherForecastViewModel?> GetForecast(string gardenId)
    {

        var weatherstation  = await GetWeatherStation(gardenId);

        if (weatherstation == null)
        {
            return null;
        }

        return await _nationalWeatherServiceApiClient.GetWeatherForecast(weatherstation);
    }
}
