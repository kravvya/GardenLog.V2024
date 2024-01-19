﻿using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;

namespace PlantCatalog.IntegrationTest.Fixture;

public class PlantCatalogApplicationFactory<TEntryPoint> : WebApplicationFactory<Program> where TEntryPoint : Program
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        //base.ConfigureWebHost(builder);
        var config = new ConfigurationBuilder()
            .AddUserSecrets<PlantCatalogTests>()
            .AddEnvironmentVariables()
            .Build();


        builder.ConfigureAppConfiguration((context, configBuilder) =>
        {
#pragma warning disable CS8620 // Argument cannot be used for parameter due to differences in the nullability of reference types.
            configBuilder.AddInMemoryCollection(new Dictionary<string, string>
                {
                    { "MongoDB:Server",  config["MongoDB:Server"]! },
                    { "MongoDB:DatabaseName",  config["MongoDB:DatabaseName"]! },
                    { "MongoDB:UserName", config["MongoDB:UserName"]! },
                    { "MongoDB:Password", config["MongoDB:Password"]! }
             });
#pragma warning restore CS8620 // Argument cannot be used for parameter due to differences in the nullability of reference types.
        });

    }
}
