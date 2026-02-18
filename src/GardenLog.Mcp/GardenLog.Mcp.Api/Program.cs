using GardenLog.Mcp.Application.Services;
using GardenLog.Mcp.Application.Tools;
using GardenLog.Mcp.Infrastructure.ApiClients;
using GardenLog.Mcp.Infrastructure.Authentication;
using GardenLog.SharedInfrastructure.Extensions;
using Serilog;
using Serilog.Enrichers.Span;

Log.Logger = new LoggerConfiguration()
    .Enrich.FromLogContext()
    .Enrich.WithSpan()
    .WriteTo.Console(Serilog.Events.LogEventLevel.Information)
    .CreateLogger();

Log.Information("Starting up GardenLog.Mcp.Api");

try
{
    var builder = WebApplication.CreateBuilder(args);

    // Add Serilog
    builder.Host.UseLogging();

    builder.Services.AddHttpContextAccessor();
    builder.Services.AddScoped<IUserContextAccessor, UserContextAccessor>();
    builder.Services.AddTransient<UserAuthenticationHandler>();
    builder.Services.AddMemoryCache();

    // Register API Clients with UserAuthenticationHandler (forwards user's JWT token)
    // Unlike PlantHarvest which uses Auth0AuthenticationHandler (gets new service token),
    // MCP forwards the user's token to maintain user context through the call chain
    builder.Services.AddHttpClient<IPlantCatalogApiClient, PlantCatalogApiClient>()
        .AddHttpMessageHandler<UserAuthenticationHandler>();

    builder.Services.AddHttpClient<IPlantHarvestApiClient, PlantHarvestApiClient>()
        .AddHttpMessageHandler<UserAuthenticationHandler>();

    builder.Services.AddHttpClient<IUserManagementApiClient, UserManagementApiClient>()
        .AddHttpMessageHandler<UserAuthenticationHandler>();

    // Configure Authentication - Always require valid Auth0 JWT
    Log.Information("Configuring Auth0 JWT authentication");
    builder.RegisterForAuthentication();

    builder.Services.AddAuthorization();

    // Add healthchecks
    builder.Services.AddBasicHealthChecks();

    // Configure MCP Server with auto-discovery of [McpServerTool] methods
    builder.Services.AddMcpServer()
    .WithToolsFromAssembly(typeof(GetPlantDetailsTool).Assembly)
     .WithHttpTransport(x =>
              {
                  // use stateless mode to prevent issues when running in multiple replicas behind a load balancer
                  x.Stateless = true;
              });

    Log.Information("MCP Server configured");

    var app = builder.Build();

    // Map MCP Server - allow anonymous access, authentication happens at tool level
    app.MapMcp("mcp");

    // Configure middleware
    app.UseKubernetesHealthChecks();

    app.UseAuthentication();
    app.UseAuthorization();

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Unhandled exception in GardenLog.Mcp.Api");
}
finally
{
    Log.Information("Shut down complete for GardenLog.Mcp.Api");
    Log.CloseAndFlush();
}

public partial class Program { }
