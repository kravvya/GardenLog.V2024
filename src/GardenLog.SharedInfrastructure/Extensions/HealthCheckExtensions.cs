using GardenLog.SharedInfrastructure.Healthchecks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace GardenLog.SharedInfrastructure.Extensions;

public static class HealthCheckExtensions
{
    private const string LIVENESS = "liveness";
    private const string STARTUP = "startup";
    private const string ENV = "env";   
    private static readonly string[] tags = new[] { "mongodb" };
    private static readonly string[] env_tags = new[] { ENV };

    public static IServiceCollection AddBasicHealthChecks(this IServiceCollection services)
    {
        services.AddHealthChecks().AddCheck("BasicStartupHealthCheck", () => HealthCheckResult.Healthy(), tags: new[] { STARTUP });
        services.AddHealthChecks().AddCheck("BasicLivenessHealthCheck", () => HealthCheckResult.Healthy(), tags: new[] { LIVENESS });
        return services;
    }

    public static IServiceCollection AddEnvironmentHeathChecks(this IServiceCollection services)
    {
        services.AddHealthChecks().AddCheck<EnvironmentHealthCheck>("EnvironmentHealthCheck", tags: env_tags);
        return services;
    }


    public static IServiceCollection AddMongoDBHealthCheck(this IServiceCollection services)
    {
        services.AddHealthChecks().AddCheck<MongoDBHealthCheck>("MongoDB", tags: tags);
        return services;
    }


    public static IServiceCollection AddAdditionStartupHealthChecks<T>(this IServiceCollection services) where T : class, IHealthCheck
    {
        services.AddHealthChecks().AddCheck<T>(nameof(T), tags: new[] { STARTUP });
        return services;
    }

    public static IServiceCollection AddAdditionLivenessHealthChecks<T>(this IServiceCollection services) where T : class, IHealthCheck
    {
        services.AddHealthChecks().AddCheck<T>(nameof(T), tags: new[] { LIVENESS });
        return services;
    }

    public static IApplicationBuilder UseMongoDBHealthCheck(this IApplicationBuilder app) =>
        app.UseHealthChecks("/health/mongodb",
                       new HealthCheckOptions
                       {
                           Predicate = x => x.Tags.Contains("mongodb")
                       });

    public static IApplicationBuilder UseEnvironmentDBHealthCheck(this IApplicationBuilder app) =>
        app.UseHealthChecks("/health/env",
                       new HealthCheckOptions
                       {
                           Predicate = x => x.Tags.Contains(ENV)
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
