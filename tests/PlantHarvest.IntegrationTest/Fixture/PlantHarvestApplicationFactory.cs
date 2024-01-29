using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;

namespace PlantHarvest.IntegrationTest.Fixture;

public class PlantHarvestApplicationFactory<TEntryPoint> : WebApplicationFactory<Program> where TEntryPoint : Program
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        //base.ConfigureWebHost(builder); 
    }
}
