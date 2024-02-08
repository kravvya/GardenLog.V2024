using GrowConditions.Api.Data.ApiClients;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestPlatform.TestHost;

namespace GrowConditions.IntegrationTest.Fixture;

public  class GrowConditionsApplicationFactory<TEntryPoint> : WebApplicationFactory<Program> where TEntryPoint : Program
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            var userManagementDescriptor = services.SingleOrDefault(services => services.ServiceType == typeof(IUserManagementApiClient));

            if (userManagementDescriptor != null)
                services.Remove(userManagementDescriptor);

            services.AddHttpClient<IUserManagementApiClient, UserManagementApiClientMock>();
        });
    }
}
