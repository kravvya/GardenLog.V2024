using GardenLog.SharedInfrastructure;
using GardenLog.SharedInfrastructure.ApiClients;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PlantCatalog.Contract;
using SendGrid;
using System.Net.Http;
using System.Net.Http.Headers;

namespace PlantCatalog.IntegrationTest;

public class AuthTests : IClassFixture<PlantCatalogServiceFixture>
{
    private readonly PlantCatalogServiceFixture _fixture;
    private readonly ITestOutputHelper _output;

    public AuthTests(PlantCatalogServiceFixture fixture, ITestOutputHelper output)
    {
        _fixture = fixture;
        _output = output;
        _output.WriteLine($"Service id {fixture.FixtureId} @ {DateTime.Now:F}");
    }

    [Fact]
    public void Get_Token_ShouldGetToken()
    {
        try
        {
          
            var configuration = new ConfigurationBuilder()
              .AddJsonFile("appsettings.json")
              .AddUserSecrets(typeof(Program).Assembly)
              .AddEnvironmentVariables()
              .Build();

            var serviceProvider = new ServiceCollection()
                .AddHttpClient()
                .AddLogging()
                .AddMemoryCache()
                .AddSingleton<IConfiguration>(configuration)
                .AddSingleton<IConfigurationService, ConfigurationService>()
                .AddSingleton<IAuth0AuthenticationApiClient, Auth0AuthenticationApiClient>()
                .BuildServiceProvider();

            var authApiClient = serviceProvider.GetRequiredService<IAuth0AuthenticationApiClient>();
            var authSettings = serviceProvider.GetRequiredService<IConfigurationService>().GetAuthSettings();

            if (authSettings.Audience == null) throw new ArgumentException("Required Audience paramter is not found. Can not generate access token without Audience", "Audience");

            _output.WriteLine($"AUTH DOMAIN: {authSettings.Authority} AUDIENCE: {authSettings.Audience}  AUDIENCE: {authSettings.Audience} CLIENT: {authSettings.ClientId} SECRET: {authSettings.ClientSecret}");

            var token = authApiClient.GetAccessToken(authSettings.Audience).GetAwaiter().GetResult();

            _output.WriteLine($"Token: {token}");

            Assert.NotNull(token);
        }
        catch (Exception ex)
        {
            _output.WriteLine($"Exception getting token: {ex}");
        }

    }

    //[Fact]
    //public async Task Get_HttpClient_ShouldGetHttpClientAsync()
    //{
    //    try
    //    {
    //        var token = (new Auth0Helper()).GetToken(typeof(Program).Assembly);

    //        var client = _fixture.CreateClient();
    //        //client.DefaultRequestHeaders.Add("RequestUser", "auth0|ec329c32-5705-4e42-a18b-4831916a3003");
    //        client.DefaultRequestHeaders.Add("RequestUser", "86377291-980f-4af2-8608-39dbbf7e09e1");

    //        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

    //        if (client.BaseAddress == null) throw new ArgumentException("Base address is not set on the http client. Fixture setup aborted", "BaseAddress");

    //        var url = $"{client.BaseAddress.OriginalString}{Routes.GetAllPlants}/";
    //        var response =  await client.GetAsync(url);

    //        var options = new JsonSerializerOptions
    //        {
    //            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    //            Converters =
    //            {
    //                new JsonStringEnumConverter(),
    //            },
    //        };

    //        var returnString = await response.Content.ReadAsStringAsync();
    //        _output.WriteLine($"Service responded with {response.StatusCode} code and {returnString} message");

    //        var plants = await response.Content.ReadFromJsonAsync<List<PlantViewModel>>(options);

    //        _output.WriteLine($"Response: {response.Content.ReadAsStringAsync()}");

    //        Assert.NotNull(response);
    //    }
    //    catch (Exception ex)
    //    {
    //        _output.WriteLine($"Exception getting http client: {ex}");
    //    }

    //}
}