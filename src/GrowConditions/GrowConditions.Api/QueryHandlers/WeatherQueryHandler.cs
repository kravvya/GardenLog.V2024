using GrowConditions.Api.Data;
using GrowConditions.Contract.ViewModels;

namespace GrowConditions.Api.QueryHandlers
{
    public interface IWeatherQueryHandler
    {
        Task<IList<WeatherUpdateViewModel>> GetHistoryOfWeatherUpdates(string gardenId, int numberOfDays);
        Task<WeatherUpdateViewModel> GetLastWeatherUpdate(string gardenId);
    }

    public class WeatherQueryHandler : IWeatherQueryHandler
    {
        private readonly IWeatherRepository _weatherRepository;

        public WeatherQueryHandler(IWeatherRepository weatherRepository)
        {
            _weatherRepository = weatherRepository;
        }

        public async Task<WeatherUpdateViewModel> GetLastWeatherUpdate(string gardenId) => await _weatherRepository.GetLastWeatherUpdate(gardenId);

        public async Task<IList<WeatherUpdateViewModel>> GetHistoryOfWeatherUpdates(string gardenId, int numberOfDays) => await _weatherRepository.GetHistoryOfWeatherUpdates(gardenId, numberOfDays);   
    }
}
