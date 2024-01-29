using GardenLog.SharedInfrastructure.ApiClients;
using PlantHarvest.IntegrationTest.Clients;
using System.Net.Http.Headers;

namespace PlantHarvest.IntegrationTest.Fixture;

public class PlantHarvestServiceFixture : PlantHarvestApplicationFactory<Program>, IDisposable
{
    private bool _disposedValue;
    private readonly PlantHarvestApplicationFactory<Program> _factory;
    

    public PlantHarvestServiceFixture()
    {
        _factory= new PlantHarvestApplicationFactory<Program>();
        _factory.ConfigureAwait(true);

        var token = (new Auth0Helper()).GetToken(typeof(Program).Assembly);

        FixtureId = Guid.NewGuid().ToString();

        var client = _factory.CreateClient();

        client.DefaultRequestHeaders.Add("RequestUser", "auth0|ec329c32-5705-4e42-a18b-4831916a3003");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        if (client.BaseAddress == null) throw new ArgumentException("Base address is not set on the http client. Fixture setup aborted", "BaseAddress");

        PlantHarvestClient = new PlantHarvestClient(client.BaseAddress, client);
        WorkLogClient = new WorkLogClient(client.BaseAddress, client);
        PlantTaskClient = new PlantTaskClient(client.BaseAddress, client);
    }

    public PlantHarvestClient PlantHarvestClient { get; init; }
    public WorkLogClient WorkLogClient { get; init; }
    public PlantTaskClient PlantTaskClient { get; init; }
    public string FixtureId { get; init; }

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
