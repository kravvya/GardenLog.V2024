using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace GardenLog.SharedInfrastructure.Healthchecks;

public class ApiHealthCheck : IHealthCheck
{
    private readonly HttpClient _httpClient;

    public ApiHealthCheck(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        var reseponse = await _httpClient.GetAsync("");

        if(reseponse.IsSuccessStatusCode)
        {
            return await Task.FromResult(new HealthCheckResult(
                status: HealthStatus.Healthy,
                description: "The Api is up and running"));
        }
        return await Task.FromResult(new HealthCheckResult(
               status: HealthStatus.Unhealthy,
               description: "The Api is down"));
    }
}
{
}
