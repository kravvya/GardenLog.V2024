using GrowConditions.Contract.ViewModels;
using GrowConditions.IntegrationTest.Clients;
using GrowConditions.IntegrationTest.Fixture;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using UserManagement.Contract.ViewModels;
using Xunit.Abstractions;

namespace GrowConditions.IntegrationTest;

public class WeatherTest : IClassFixture<GrowConditionsServiceFixture>
{
    private readonly ITestOutputHelper _output;
    private readonly WeatherClient _weatherClient;

    public WeatherTest(GrowConditionsServiceFixture fixture, ITestOutputHelper output)
    {
        _weatherClient = fixture.WeatherClient;
        _output = output;
        _output.WriteLine($"Service id {fixture.FixtureId} @ {DateTime.Now:F}");
    }

    [Fact]
    public async Task Get_WeatherStation_ShouldFindOne()
    {
        var response = await _weatherClient.GetWeatherstation("TestGarden", 44.9778, -93.667);

        response.EnsureSuccessStatusCode();

        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            Converters =
                {
                    new JsonStringEnumConverter(),
                },
        };

        var weatherstation = await response.Content.ReadFromJsonAsync<WeatherstationViewModel>();

        Assert.NotNull(weatherstation);
        Assert.Equal("MPX", weatherstation.ForecastOffice);
        Assert.Equal(95, weatherstation.GridX);
        Assert.Equal(72, weatherstation.GridY);
        Assert.Equal("America/Chicago", weatherstation.Timezone);
    }

    [Fact]
    public async Task Get_WeatherStation_ShouldNotFindOne()
    {
        var response = await _weatherClient.GetWeatherstation("TestGarden", 0, 0);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Get_WeatherForecast_ShouldFindOne()
    {
        var response = await _weatherClient.GetWeatherForecast("TestGarden");

        response.EnsureSuccessStatusCode();

        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            Converters =
            {
                    new JsonStringEnumConverter(),
                },
        };

        var weatherForecast = await response.Content.ReadFromJsonAsync<WeatherForecastViewModel>();

        Assert.NotNull(weatherForecast);
        Assert.True(weatherForecast.DailyForecasts.Count > 0);
        Assert.NotNull(weatherForecast.DailyForecasts[0].Forecast);
        Assert.NotEqual(0, weatherForecast.DailyForecasts[0].Forecast!.Temp);
        Assert.NotNull(weatherForecast.DailyForecasts[0].Conditions);
        Assert.NotEmpty(weatherForecast.DailyForecasts[0].Conditions!.Main);
        Assert.NotEmpty(weatherForecast.DailyForecasts[0].Conditions!.Description);
        Assert.NotEmpty(weatherForecast.DailyForecasts[0].Conditions!.Icon);        
    }
}
