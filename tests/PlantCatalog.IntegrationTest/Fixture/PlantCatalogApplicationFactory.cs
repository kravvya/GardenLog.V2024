using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;

namespace PlantCatalog.IntegrationTest.Fixture;

public class PlantCatalogApplicationFactory<TEntryPoint> : WebApplicationFactory<Program> where TEntryPoint : Program
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        //base.ConfigureWebHost(builder);
        var config = new ConfigurationBuilder()
            //.AddUserSecrets<PlantCatalogTests>()
            .AddUserSecrets<AuthTests>()
            .AddEnvironmentVariables()
            .Build();


        builder.ConfigureAppConfiguration((context, configBuilder) =>
        {
#pragma warning disable CS8620 // Argument cannot be used for parameter due to differences in the nullability of reference types.
            configBuilder.AddInMemoryCollection(new Dictionary<string, string>
                {
                    { "MongoDB:Server",  config["mongodb-server"]! },
                    { "MongoDB:DatabaseName",  config["mongodb-databasename"]! },
                    { "MongoDB:UserName", config["mongodb-username"]! },
                    { "MongoDB:Password", config["mongodb-password"]! }
             });
#pragma warning restore CS8620 // Argument cannot be used for parameter due to differences in the nullability of reference types.
        });

    }
}
