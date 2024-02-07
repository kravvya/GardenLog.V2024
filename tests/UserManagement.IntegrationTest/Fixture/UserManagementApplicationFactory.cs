using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using UserManagement.Api.Data.ApiClients;

namespace UserManagement.IntegrationTest.Fixture;

public class UserManagementApplicationFactory<TEntryPoint> : WebApplicationFactory<Program> where TEntryPoint : Program
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            var growConditionsDescriptor = services.SingleOrDefault(services => services.ServiceType == typeof(IGrowConditionsApiClient));

            if (growConditionsDescriptor != null)
                services.Remove(growConditionsDescriptor);

            services.AddHttpClient<IGrowConditionsApiClient, GrowConditionsApiClientMock>();
        });
    }
}
