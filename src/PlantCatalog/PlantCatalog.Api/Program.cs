using FluentValidation;
using FluentValidation.AspNetCore;
using GardenLog.SharedInfrastructure;
using GardenLog.SharedInfrastructure.Extensions;
using GardenLog.SharedInfrastructure.MongoDB;
using GardenLog.SharedKernel.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using PlantCatalog.Contract.Validators;
using PlantCatalog.Domain.PlantAggregate;
using PlantCatalog.Infrustructure.Data;
using PlantCatalog.Infrustructure.Data.Repositories;
using Serilog;
using Serilog.Enrichers.Span;
using Serilog.Formatting.Json;
using System;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

Log.Logger = new LoggerConfiguration()
    .Enrich.FromLogContext()
    .Enrich.WithSpan()
    //.WriteTo.Console(new JsonFormatter())
    .WriteTo.Console(Serilog.Events.LogEventLevel.Information)
    .CreateLogger();
   // .CreateBootstrapLogger();
Log.Information("Starting up PlantCatalog.Api");

try
{
    var builder = WebApplication.CreateBuilder(args);

    // Add services to the container.
    builder.Host.UseLogging();

    //https://github.com/FluentValidation/FluentValidation/issues/1965
    builder.Services.AddFluentValidationAutoValidation();
    builder.Services.AddFluentValidationClientsideAdapters();
    builder.Services.AddValidatorsFromAssemblyContaining<CreatePlantCommandValidator>();

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
    builder.RegisterSwaggerForAuth("Plant Catalog Api");
    builder.Services.AddBasicHealthChecks();

    builder.Services.AddSingleton<IConfigurationService, ConfigurationService>();
    builder.Services.AddSingleton<IMongoDBContext, MongoDbContext>();
    builder.Services.AddScoped<IUnitOfWork, MongoDBUnitOfWork>();


    builder.Services.AddScoped<IPlantRepository, PlantRepository>();
    builder.Services.AddScoped<IPlantVarietyRepository, PlantVarietyRepository>();

    builder.Services.AddScoped<IPlantCommandHandler, PlantCommandHandler>();
    builder.Services.AddScoped<IPlantQueryHandler, PlantQueryHandler>();

    //builder.Services.AddCors(options =>
    //{
    //    options.AddPolicy(name: "glWebPolicy",
    //                policy =>
    //                {
    //                    policy.WithOrigins("https://kravvya.github.io",
    //                        "https://localhost:7014",
    //                        "https://localhost:44318")
    //                    .AllowAnyMethod()
    //                    .AllowAnyHeader();
    //                });
    //});

    // 1. Add Authentication Services
    //builder.RegisterForAuthentication();

    var app = builder.Build();

    // Configure the HTTP request pipeline.
    // app.UseSwaggerForAuth(app.Services.GetRequiredService<IConfigurationService>());

    //Aapp Container ingress is EntityHandling HTTPs redirects. This is not needed.
    //app.UseHttpsRedirection();
    app.UseKubernetesHealthChecks();

    //// 2. Enable authentication middleware
    //app.UseAuthentication();

    //app.UseAuthorization();

    //app.UseCors("glWebPolicy");

    app.MapControllers();

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Unhandled exception in PlantCatalog.Api");
}
finally
{
    Log.Information("Shut down complete for PlantCatalog.Api");
    Log.CloseAndFlush();
}
public partial class Program { }