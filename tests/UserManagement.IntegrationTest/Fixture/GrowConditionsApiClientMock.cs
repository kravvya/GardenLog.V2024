using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using UserManagement.Api.Data.ApiClients;
using UserManagement.Contract.ViewModels;

namespace UserManagement.IntegrationTest.Fixture;

public class GrowConditionsApiClientMock : IGrowConditionsApiClient
{
    public GrowConditionsApiClientMock(HttpClient httpClient, IConfiguration confguration, ILogger<GrowConditionsApiClientMock> logger)
    {
        
    }

    public Task<WeatherstationViewModel?> GetWeatherStation(decimal latitude, decimal longitude)
    {
        WeatherstationViewModel? weatherstation = new()
        {
           ForecastOffice = "MPX",
           GridX = 95,
           GridY = 72,
           Timezone = "America/Chicago",
           WeatherstationId = Guid.NewGuid().ToString()
        };

#pragma warning disable CS8619 // Nullability of reference types in value doesn't match target type.
        return Task.FromResult(weatherstation);
#pragma warning restore CS8619 // Nullability of reference types in value doesn't match target type.
    }
}
