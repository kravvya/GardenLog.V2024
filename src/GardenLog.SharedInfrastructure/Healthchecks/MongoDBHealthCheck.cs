using GardenLog.SharedInfrastructure.MongoDB;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace GardenLog.SharedInfrastructure.Healthchecks
{
    public class MongoDBHealthCheck : IHealthCheck
    {
        private readonly IMongoDBContext _dbContext;

        public MongoDBHealthCheck(IMongoDBContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            var isHealthy = await _dbContext.IsMongoDBHealthy();

            if (isHealthy)
            {
                return HealthCheckResult.Healthy("MongoDBConnection is healthy");
            }

            return new HealthCheckResult(context.Registration.FailureStatus, "MongoDBConnection is not healthy");
        }
    }
}
