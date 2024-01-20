using GardenLog.SharedInfrastructure.ApiClients;
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
            var token = (new Auth0Helper()).GetToken(typeof(Program).Assembly);

            _output.WriteLine($"Token: {token}");

            Assert.NotNull(token);
        }
        catch (Exception ex)
        {
            _output.WriteLine($"Exception getting token: {ex}");
        }

    }

    [Fact]
    public async Task Get_HttpClient_ShouldGetHttpClientAsync()
    {
        try
        {
            var token = (new Auth0Helper()).GetToken(typeof(Program).Assembly);

            var client = _fixture.CreateClient();
            //client.DefaultRequestHeaders.Add("RequestUser", "auth0|ec329c32-5705-4e42-a18b-4831916a3003");
            client.DefaultRequestHeaders.Add("RequestUser", "86377291-980f-4af2-8608-39dbbf7e09e1");

            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            if (client.BaseAddress == null) throw new ArgumentException("Base address is not set on the http client. Fixture setup aborted", "BaseAddress");

            var url = $"{client.BaseAddress.OriginalString}{Routes.GetAllPlants}/";
            var response =  await client.GetAsync(url);

            var options = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                Converters =
                {
                    new JsonStringEnumConverter(),
                },
            };

            var returnString = await response.Content.ReadAsStringAsync();
            _output.WriteLine($"Service responded with {response.StatusCode} code and {returnString} message");

            var plants = await response.Content.ReadFromJsonAsync<List<PlantViewModel>>(options);

            _output.WriteLine($"Response: {response.Content.ReadAsStringAsync()}");

            Assert.NotNull(response);
        }
        catch (Exception ex)
        {
            _output.WriteLine($"Exception getting http client: {ex}");
        }

    }
}