namespace UserManagement.Api.Model;

public interface IWeatherstationRepository :  IRepository<Weatherstation>
{
    Task<Weatherstation> ReadWeatherstation(string gardenId);
    Task<WeatherstationViewModel> GetWeatherstation(string gardenId);
    void AddWeatherstation(Garden garden);
    void DeleteWeatherstation(string gardenId);
    void UpdateWeatherstation(Garden garden);
}
