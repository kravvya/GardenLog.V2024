using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace GardenLog.SharedInfrastructure.ApiClients;

public class Auth0Helper
{
    public string? GetToken(System.Reflection.Assembly secretAssembly)
    {
        var configuration = new ConfigurationBuilder()
              .AddJsonFile("appsettings.json")
              .AddUserSecrets(secretAssembly)
              .AddEnvironmentVariables()
              .Build();

        var serviceProvider = new ServiceCollection()
            .AddHttpClient()
            .AddLogging()
            .AddMemoryCache()
            .AddSingleton<IConfiguration>(configuration)
            .AddSingleton<IConfigurationService, ConfigurationService>()
            .AddSingleton<IAuth0AuthenticationApiClient, Auth0AuthenticationApiClient>()
            .BuildServiceProvider();

        var authApiClient = serviceProvider.GetRequiredService<IAuth0AuthenticationApiClient>();
        var appSettings = serviceProvider.GetRequiredService<IConfigurationService>().GetAuthSettings();

        if (appSettings.Audience == null) throw new ArgumentException("Required Audience paramter is not found. Can not generate access token without Audience", "Audience");

        var accessToken = authApiClient.GetAccessToken(appSettings.Audience).GetAwaiter().GetResult();

        return accessToken;
    }
}