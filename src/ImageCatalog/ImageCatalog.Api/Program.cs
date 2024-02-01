using GardenLog.SharedInfrastructure.MongoDB;
using ImageCatalog.Api.CommandHandlers;
using ImageCatalog.Api.QueryHandlers;
using Serilog;
using Serilog.Enrichers.Span;

Log.Logger = new LoggerConfiguration()
    .Enrich.FromLogContext()
    .Enrich.WithSpan()
    .WriteTo.Console()
    .CreateLogger();

Log.Information("Starting up Image Catalog.Api");

try
{
    var builder = WebApplication.CreateBuilder(args);

    builder.Host.UseLogging();
    // Add services to the container.
    builder.Services.AddHttpClient();

    builder.Services.AddControllers();
    // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
    builder.Services.AddEndpointsApiExplorer();
    builder.RegisterSwaggerForAuth("Image Catalog Api");
    builder.Services.AddBasicHealthChecks();
    builder.Services.AddMongoDBHealthCheck();
    builder.Services.AddEnvironmentHeathChecks();

    builder.Services.AddSingleton<IConfigurationService, ConfigurationService>();

    builder.Services.AddSingleton<IModelConfigurator, DomainModelsConfigurator>();
    builder.Services.AddSingleton<IMongoDBContext, MongoDbContext>();
    builder.Services.AddScoped<IUnitOfWork, MongoDBUnitOfWork>();

    builder.Services.AddSingleton<IFileRepository, BlobRepository>();
    builder.Services.AddScoped<IImageRepository, ImageRepository>();

    builder.Services.AddScoped<IImageQueryHandler, ImageQueryHandler>();
    builder.Services.AddScoped<IImageCommandHandler, ImageCommandHandler>();

    builder.Services.AddScoped<IFileCommandHandler, FileCommandHandler>();


    builder.Services.AddCors(options =>
    {
        options.AddGlWebPolicy();
    });

    // 1. Add Authentication Services
    builder.RegisterForAuthentication();

    var app = builder.Build();

    // Configure the HTTP request pipeline.
    app.UseSwaggerForAuth(app.Services.GetRequiredService<IConfigurationService>());

    app.UseKubernetesHealthChecks();
    app.UseMongoDBHealthCheck();
    app.UseEnvironmentDBHealthCheck();

    //// 2. Enable authentication middleware
    app.UseAuthentication();

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

    Log.Fatal(ex, "Unhandled exception in ImageCatalog.Api");
}
finally
{
    Log.Information("Shut down complete for ImageCatalog.Api");
    Log.CloseAndFlush();
}

public partial class Program { }