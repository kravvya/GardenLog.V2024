using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using System.Text;

namespace GardenLog.SharedInfrastructure.Healthchecks;

public class EnvironmentHealthCheck(ILogger<EnvironmentHealthCheck> logger, IConfiguration configuration) : IHealthCheck
{
    private readonly ILogger<EnvironmentHealthCheck> _logger = logger;
    private readonly IConfiguration _configuration = configuration;

    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        //print all environmental values ot s atringbuilder
        foreach (var item in _configuration.AsEnumerable())
        {
            string message = $"{item.Key} = {item.Value}";
            _logger.LogInformation("Env {message}", message);
        }

        return await Task.FromResult(new HealthCheckResult(status: HealthStatus.Healthy));
    }
}

