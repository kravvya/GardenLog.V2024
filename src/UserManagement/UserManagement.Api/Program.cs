
using FluentValidation;
using FluentValidation.AspNetCore;
using GardenLog.SharedInfrastructure;
using GardenLog.SharedInfrastructure.ApiClients;
using GardenLog.SharedInfrastructure.Extensions;
using GardenLog.SharedInfrastructure.MongoDB;
using Microsoft.AspNetCore.Mvc;
using PlantHarvest.Infrastructure.Data.Repositories;
using Serilog;
using Serilog.Enrichers.Span;
using System.Text.Json.Serialization;
using UserManagement.Api.Data.ApiClients;
using UserManagement.CommandHandlers;
using UserManagement.QueryHandlers;

Log.Logger = new LoggerConfiguration()
    .Enrich.FromLogContext()
    .Enrich.WithSpan()
    //.WriteTo.Console(new JsonFormatter())
    .WriteTo.Console(Serilog.Events.LogEventLevel.Information)
    .CreateLogger();
// .CreateBootstrapLogger();
Log.Information("Starting up UserProfile.Api");

try
{
    var builder = WebApplication.CreateBuilder(args);

    // Add services to the container.
    builder.Host.UseLogging();

    //https://github.com/FluentValidation/FluentValidation/issues/1965
    builder.Services.AddFluentValidationAutoValidation();
    builder.Services.AddFluentValidationClientsideAdapters();
    builder.Services.AddValidatorsFromAssemblyContaining<CreateUserProfileCommandValidator>();

    builder.Services.AddMemoryCache();

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
    builder.RegisterSwaggerForAuth("User Management Api");
    builder.Services.AddBasicHealthChecks();
    builder.Services.AddMongoDBHealthCheck();

    builder.Services.AddSingleton<IConfigurationService, ConfigurationService>();
    builder.Services.AddSingleton<IMongoDBContext, MongoDbContext>();
    builder.Services.AddScoped<IUnitOfWork, MongoDBUnitOfWork>();


    builder.Services.AddScoped<IUserProfileRepository, UserProfileRepository>();
    builder.Services.AddScoped<IGardenRepository, GardenRepository>();

    builder.Services.AddHttpClient<IAuth0AuthenticationApiClient, Auth0AuthenticationApiClient>();
    builder.Services.AddHttpClient<IAuth0ManagementApiClient, Auth0ManagementApiClient>();

    builder.Services.AddScoped<IEmailClient, EmailClient>();
    builder.Services.AddScoped<IContactCommandHandler, ContactCommandHandler>();

    builder.RegisterHttpClient<IPlantHarvestApiClient, PlantHarvestApiClient>();
    builder.Services.AddScoped<INotificationCommandHandler, NotificationCommandHandler>();

    builder.Services.AddScoped<IUserProfileCommandHandler, UserProfileCommandHandler>();
    builder.Services.AddScoped<IUserProfileQueryHandler, UserProfileQueryHandler>();

    builder.Services.AddScoped<IGardenCommandHandler, GardenCommandHandler>();
    builder.Services.AddScoped<IGardenQueryHandler, GardenQueryHandler>();

    builder.Services.AddCors(options =>
    {
        options.AddGlWebPolicy();
    });

    builder.RegisterEmail();

    // 1. Add Authentication Services
    builder.RegisterForAuthentication();

    var app = builder.Build();

    // Configure the HTTP request pipeline.
    app.UseSwaggerForAuth(app.Services.GetRequiredService<IConfigurationService>());

    app.UseKubernetesHealthChecks();
    app.UseMongoDBHealthCheck();

    // 2. Enable authentication middleware
    app.UseAuthentication();

    app.UseAuthorization();

    app.UseCors("glWebPolicy");

    app.MapControllers();

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Unhandled exception in UserProfile.Api");
}
finally
{
    Log.Information("Shut down complete for UserProfile.Api");
    Log.CloseAndFlush();
}
public partial class Program { }