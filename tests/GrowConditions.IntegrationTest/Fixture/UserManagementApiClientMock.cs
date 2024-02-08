using GrowConditions.Api.Data.ApiClients;
using UserManagement.Contract.ViewModels;

namespace GrowConditions.IntegrationTest.Fixture;

public class UserManagementApiClientMock : IUserManagementApiClient
{
    public UserManagementApiClientMock(HttpClient httpClient)
    {
        
    }
#pragma warning disable CS8619 // Nullability of reference types in value doesn't match target type.
    public Task<List<GardenViewModel>?> GetAllGardens()
    {
        return Task.FromResult(new List<GardenViewModel>() { GetGardenViewModel()});
    }

    public Task<GardenViewModel?> GetGarden(string gardenId)
    {
       

        return Task.FromResult(GetGardenViewModel());
    }

   private GardenViewModel GetGardenViewModel()
    {
        return  new()
        {
            GardenId = Guid.NewGuid().ToString(),
            Latitude = 44.9778m,
            Longitude = -93.667m,
            Name = "Test Garden"
        };
    }

    public Task<WeatherstationViewModel?> GetWeatherstation(string gardenId)
    {
        WeatherstationViewModel? weatherstation = new()
        {
            ForecastOffice = "MPX",
            GridX = 95,
            GridY = 72,
            Timezone = "America/Chicago",
            WeatherstationId = Guid.NewGuid().ToString()
        };
        return Task.FromResult(weatherstation);
    }

    public Task SetWeatherstation(string gardenId, WeatherstationViewModel weatherstation)
    {
        return Task.CompletedTask;
    }
#pragma warning restore CS8619 // Nullability of reference types in value doesn't match target type.
}
