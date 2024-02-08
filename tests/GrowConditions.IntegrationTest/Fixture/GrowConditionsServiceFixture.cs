using GardenLog.SharedInfrastructure.ApiClients;
using GrowConditions.IntegrationTest.Clients;
using Microsoft.VisualStudio.TestPlatform.TestHost;
using System.Net.Http.Headers;

namespace GrowConditions.IntegrationTest.Fixture;

public class GrowConditionsServiceFixture : GrowConditionsApplicationFactory<Program>, IDisposable
{
    private bool _disposedValue;
    private readonly GrowConditionsApplicationFactory<Program> _factory;
    public WeatherClient WeatherClient { get; init; }
    public string FixtureId { get; init; }

    public GrowConditionsServiceFixture()
    {
        _factory = new GrowConditionsApplicationFactory<Program>();
        _factory.ConfigureAwait(true);

        var token = (new Auth0Helper()).GetToken(typeof(Program).Assembly);

        FixtureId = Guid.NewGuid().ToString();

        var client = _factory.CreateClient();

        client.DefaultRequestHeaders.Add("RequestUser", "auth0|ec329c32-5705-4e42-a18b-4831916a3003");

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        client.DefaultRequestHeaders.Add("Scope", "IntegrationTest");

        if (client.BaseAddress == null) throw new ArgumentException("Base address is not set on the http client. Fixzture setup aborted", "BaseAddress");
        WeatherClient = new WeatherClient(client.BaseAddress, client);
    }

    protected override void Dispose(bool disposing)
    {
        if (!_disposedValue)
        {
            if (disposing)
            {
                _factory.Dispose();
            }
            _disposedValue = true;
        }
    }

    public new void Dispose()
    {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}
