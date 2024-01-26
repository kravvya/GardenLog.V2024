using GardenLog.SharedInfrastructure.Healthchecks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace GardenLog.SharedInfrastructure.Extensions;

public static class HealthCheckExtensions
{
    private const string Liveness = "liveness";
    private const string Startup = "startup";

    public static IServiceCollection AddBasicHealthChecks(this IServiceCollection services)
    {
        services.AddHealthChecks().AddCheck("BasicStartupHealthCheck", () => HealthCheckResult.Healthy(), tags: new[] { Startup });
        services.AddHealthChecks().AddCheck("BasicLivenessHealthCheck", () => HealthCheckResult.Healthy(), tags: new[] { Liveness });
        return services;
    }

    public static IServiceCollection AddMongoDBHealthCheck(this IServiceCollection services)
    {
        services.AddHealthChecks().AddCheck<MongoDBHealthCheck>("MongoDB", tags: new[] { "mongodb" });
        return services;
    }

    public static IServiceCollection AddAdditionStartupHealthChecks<T>(this IServiceCollection services) where T : class, IHealthCheck
    {
        services.AddHealthChecks().AddCheck<T>(nameof(T), tags: new[] { Startup });
        return services;
    }

    public static IServiceCollection AddAdditionLivenessHealthChecks<T>(this IServiceCollection services) where T : class, IHealthCheck
    {
        services.AddHealthChecks().AddCheck<T>(nameof(T), tags: new[] { Liveness });
        return services;
    }

    public static IApplicationBuilder UserMongoDBHealthCheck(this IApplicationBuilder app) =>
        app.UseHealthChecks("/health/mongodb",
                       new HealthCheckOptions
                       {
                           Predicate = x => x.Tags.Contains("mongodb")
                       });

    public static IApplicationBuilder UseKubernetesHealthChecks(this IApplicationBuilder app) =>
         app
           .UseHealthChecks("/health/startup",
             new HealthCheckOptions
             {
                 Predicate = x => x.Tags.Contains("startup")
             })
           .UseHealthChecks("/health/live",
             new HealthCheckOptions
             {
                 Predicate = x => x.Tags.Contains("liveness")
             });
             //.UseHealthChecks("/",
             //new HealthCheckOptions
             //{
             //    Predicate = x => x.Tags.Contains("liveness")
             //});
}
