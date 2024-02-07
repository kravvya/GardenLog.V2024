using GardenLog.SharedKernel.Interfaces;
using GrowConditions.Api.Data.ApiClients;
using GrowConditions.Api.Data.Repositories;

namespace GrowConditions.Api.CommandHandlers
{
    public interface IWeatherCommandHandler
    {
        Task GetWeatherUpdates();
    }

    public class WeatherCommandHandler : IWeatherCommandHandler
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IUserManagementApiClient _userManagementApiClient;
        private readonly IOpenWeatherApiClient _openWeatherApiClient;
        private readonly IWeatherRepository _weatherRepository;
        private readonly ILogger<WeatherCommandHandler> _logger;

        public WeatherCommandHandler(IUnitOfWork unitOfWork, IUserManagementApiClient userManagementApiClient, IOpenWeatherApiClient openWeatherApiClient, IWeatherRepository weatherRepository, ILogger<WeatherCommandHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _userManagementApiClient = userManagementApiClient;
            _openWeatherApiClient = openWeatherApiClient;
            _weatherRepository = weatherRepository;
            _logger = logger;
        }

        public async Task GetWeatherUpdates()
        {
            try
            {
                _logger.LogInformation("Fetch weather starting at {time}", DateTime.Now);

                _logger.LogInformation("Fetch weather is going to get gardens/coordinates");

                List<GardenViewModel>? gardens = await _userManagementApiClient.GetAllGardens();
                if (gardens != null)
                {
                    _logger.LogInformation("Fetch weather found {count} coordinates", gardens.Count);

                    foreach (var garden in gardens.Where(g => g.Latitude != -1 && g.Longitude != -1 && g.Latitude != 0 && g.Longitude != 0))
                    {
                        try
                        {
                            _logger.LogInformation("Fetch weather is going to get weather for {Latitude} {Longitude}", garden.Latitude, garden.Longitude);
                            var weather = await _openWeatherApiClient.GetWeatherUpdate(garden);
                            if (weather != null)
                            {
                                _logger.LogInformation("Fetch weather received. Last update at: {date}", weather.UpdatedDateLocal);

                                _weatherRepository.Add(weather);
                            }
                            else
                            {
                                _logger.LogError("Fetch weather did not received for garden: {garden}", garden.GardenId);
                            }
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError("Exception during Fetch weather report for {Latitude} {Longitude}.  {exception}", garden.Latitude, garden.Longitude, ex);
                        }
                    }

                    await _unitOfWork.SaveChangesAsync();
                }
                else
                {
                    _logger.LogError("Did not find any gardens to update weather.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("Exception during Fetch weather report {exception}", ex);
            }
        }
    }
}
