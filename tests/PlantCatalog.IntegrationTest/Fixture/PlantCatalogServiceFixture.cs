using GardenLog.SharedInfrastructure;
using GardenLog.SharedInfrastructure.ApiClients;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using System.Net.Http.Headers;

namespace PlantCatalog.IntegrationTest.Fixture;

public class PlantCatalogServiceFixture : PlantCatalogApplicationFactory<Program>, IDisposable
{
    private bool _disposedValue;
    private readonly PlantCatalogApplicationFactory<Program> _factory;
    

    public PlantCatalogServiceFixture()
    {
        _factory= new PlantCatalogApplicationFactory<Program>();
        _factory.ConfigureAwait(true);

        //var token = (new Auth0Helper()).GetToken(typeof(Program).Assembly);

        FixtureId = Guid.NewGuid().ToString();
        
        var client = _factory.CreateClient();

        //client.DefaultRequestHeaders.Add("RequestUser", "auth0|ec329c32-5705-4e42-a18b-4831916a3003");

        //client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        //if (client.BaseAddress == null) throw new ArgumentException("Base address is not set on the http client. Fixture setup aborted", "BaseAddress");

        PlantCatalogClient = new PlantCatalogClient(client.BaseAddress, client);
    }

    public PlantCatalogClient PlantCatalogClient { get; init; }
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
