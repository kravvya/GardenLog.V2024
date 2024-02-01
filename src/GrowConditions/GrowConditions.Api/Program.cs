using Serilog;
using Serilog.Enrichers.Span;
using Serilog.Formatting.Json;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json.Serialization;
using GardenLog.SharedInfrastructure.MongoDB;
using GardenLog.SharedKernel.Interfaces;
using GrowConditions.Api.ApiClients;
using GrowConditions.Api.Data;

Log.Logger = new LoggerConfiguration()
    .Enrich.FromLogContext()
    .Enrich.WithSpan()
    .WriteTo.Console(new JsonFormatter())
    .CreateBootstrapLogger();


Log.Information("Starting up Event GrowCondition.Api");

try
{

    var builder = WebApplication.CreateBuilder(args);

    // Add services to the container.
    builder.Host.UseLogging();

   
    builder.Services.AddMemoryCache();
    builder.Services.AddAutoMapper(typeof(Program));


    builder.Services.AddControllers()
    .ConfigureApiBehaviorOptions(options =>
        options.InvalidModelStateResponseFactory = context =>
        {
            return new BadRequestObjectResult(context.ModelState);
        }
    ).AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });

    // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
    builder.Services.AddEndpointsApiExplorer();
    builder.RegisterSwaggerForAuth("Grow Conditions Api");
    builder.Services.AddBasicHealthChecks();
    builder.Services.AddMongoDBHealthCheck();
    builder.Services.AddEnvironmentHeathChecks();

    builder.Services.AddSingleton<IConfigurationService, ConfigurationService>();

    builder.Services.AddSingleton<IModelConfigurator, DomainModelsConfigurator>();
    builder.Services.AddSingleton<IMongoDBContext, MongoDbContext>();
    builder.Services.AddScoped<IUnitOfWork, MongoDBUnitOfWork>();

    builder.Services.AddHttpClient<IOpenWeatherApiClient, OpenWeatherApiClient>();
    builder.RegisterHttpClient<IUserManagementApiClient, UserManagementApiClient>();

    builder.Services.AddScoped<IWeatherRepository, WeatherRepository>();

    builder.Services.AddScoped<IWeatherCommandHandler, WeatherCommandHandler>();
    builder.Services.AddScoped<IWeatherQueryHandler, WeatherQueryHandler>();

    builder.Services.AddCors(options =>
    {
        options.AddGlWebPolicy();
    });

    // 1. Add Authentication Services
    builder.RegisterForAuthentication();

    var app = builder.Build();

    // Configure the HTTP request pipeline.
    app.UseSwaggerForAuth(app.Services.GetRequiredService<IConfigurationService>());

    //Aapp Container ingress is EntityHandling HTTPs redirects. This is not needed.
    //app.UseHttpsRedirection();
    app.UseKubernetesHealthChecks();
    app.UseMongoDBHealthCheck();
    app.UseEnvironmentDBHealthCheck();

    app.UseAuthorization();

    app.UseCors("glWebPolicy");

    app.MapControllers();

    app.Run();
}
catch (Exception ex)
{
    string type = ex.GetType().Name;
    if (type.Equals("StopTheHostException", StringComparison.Ordinal))
    {
        throw;
    }

    Log.Fatal(ex, "Unhandled exception in GrowConditions.Api");
}
finally
{
    Log.Information("Shut down complete for GrowConditions.Api");
    Log.CloseAndFlush();
}
