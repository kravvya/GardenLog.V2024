using GardenLog.SharedInfrastructure.ApiClients;
using Newtonsoft.Json.Linq;

namespace PlantCatalog.IntegationTests;

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

}
