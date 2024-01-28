using GardenLog.SharedInfrastructure.ApiClients;
using PlantCatalog.IntegrationTest.Fixture;
using System.Net.Http.Headers;

namespace ImageCatalog.IntegrationTest.Fixture;

public class ImageCatalogServiceFixture : ImageCatalogApplicationFactory<Program>, IDisposable
{
    private bool _disposedValue;
    private readonly ImageCatalogApplicationFactory<Program> _factory;
    

    public ImageCatalogServiceFixture()
    {
        _factory= new ImageCatalogApplicationFactory<Program>();
        _factory.ConfigureAwait(true);

        var token = (new Auth0Helper()).GetToken(typeof(Program).Assembly);

        FixtureId = Guid.NewGuid().ToString();

        var client = _factory.CreateClient();

        client.DefaultRequestHeaders.Add("RequestUser", "auth0|ec329c32-5705-4e42-a18b-4831916a3003");

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        if (client.BaseAddress == null) throw new ArgumentException("Base address is not set on the http client. Fixture setup aborted", "BaseAddress");

        ImageCatalogClient = new ImageCatalogClient(client.BaseAddress, client);
        FileCatalogClient = new FileCatalogClient(client.BaseAddress, client);
    }

    public ImageCatalogClient ImageCatalogClient { get; init; }
    public FileCatalogClient FileCatalogClient { get; init; }
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
