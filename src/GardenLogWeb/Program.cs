using Blazored.Toast;
using FluentValidation;
using GardenLogWeb;
using GardenLogWeb.Services.Auth;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Polly;
using Polly.Extensions.Http;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

builder.Services.AddSingleton<ICacheService, CacheService>();
builder.Services.AddSingleton<IMouseService, MouseService>();
builder.Services.AddSingleton<IKeyService, KeyService>();

builder.Services.AddScoped<IPlantService, PlantService>();

builder.Services.AddScoped<IHarvestCycleService, HarvestCycleService>();

builder.Services.AddScoped<IUserProfileService, UserProfileService>();
builder.Services.AddScoped<IImageService, ImageService>();
builder.Services.AddScoped<IVerifyService, VerifyService>();
builder.Services.AddScoped<IGardenService, GardenService>();
builder.Services.AddScoped<IWorkLogService, WorkLogService>();
builder.Services.AddScoped<IPlantTaskService, PlantTaskService>();
builder.Services.AddScoped<IContactService, ContactService>();
builder.Services.AddScoped<IReportService, ReportService>();
builder.Services.AddScoped<IGrowConditionsService, GrowConditionsService>();

builder.Services.AddBlazoredToast();
builder.Services.AddScoped<IGardenLogToastService, GardenLogToastService>();

string serviceUrl = "";
string imageServiceUrl = "";
string harvestServiceUrl = "";
string userServiceUrl = "";
string growConditionsUrl = "";

if (builder.HostEnvironment.IsProduction())
{
    serviceUrl = "https://plantcatalogapi-containerapp.politecoast-efa2ff8d.eastus.azurecontainerapps.io";
    imageServiceUrl = "https://imagecatalogapi-containerapp.politecoast-efa2ff8d.eastus.azurecontainerapps.io";
    harvestServiceUrl = "https://plantharvestapi-containerapp.politecoast-efa2ff8d.eastus.azurecontainerapps.io";
    userServiceUrl = "https://usermanagementapi-containerapp.politecoast-efa2ff8d.eastus.azurecontainerapps.io";
    growConditionsUrl = "https://growconditionsapi-containerapp.politecoast-efa2ff8d.eastus.azurecontainerapps.io";

    builder.Services.AddOidcAuthentication(options =>
    {
        builder.Configuration.Bind("Auth0", options.ProviderOptions);
        options.ProviderOptions.ResponseType = "code";
        options.ProviderOptions.AdditionalProviderParameters.Add("audience", builder.Configuration["Auth0:Audience"]!);
        options.ProviderOptions.DefaultScopes.Add("email");
    }).AddAccountClaimsPrincipalFactory<ArrayClaimsPrincipalFactory<RemoteUserAccount>>(); //this is needed to parse roles from array to individual roles
}
else
{
    serviceUrl = "https://localhost:5051/";
    imageServiceUrl = "http://localhost:5072/";
    harvestServiceUrl = "http://localhost:5049/";
    userServiceUrl = "http://localhost:5212/";

    serviceUrl = "https://plantcatalogapi-containerapp.politecoast-efa2ff8d.eastus.azurecontainerapps.io/";
    imageServiceUrl = "https://imagecatalogapi-containerapp.politecoast-efa2ff8d.eastus.azurecontainerapps.io";
    harvestServiceUrl = "https://plantharvestapi-containerapp.politecoast-efa2ff8d.eastus.azurecontainerapps.io/";
    userServiceUrl = "https://usermanagementapi-containerapp.politecoast-efa2ff8d.eastus.azurecontainerapps.io";
    growConditionsUrl = "https://growconditionsapi-containerapp.politecoast-efa2ff8d.eastus.azurecontainerapps.io";

    //builder.Services.AddAuthorizationCore();
    //builder.Services.AddScoped<AuthenticationStateProvider, TestAuthStateProvider>();

    builder.Services.AddOidcAuthentication(options =>
    {
        builder.Configuration.Bind("Auth0", options.ProviderOptions);
        options.ProviderOptions.ResponseType = "code";
        options.ProviderOptions.AdditionalProviderParameters.Add("audience", builder.Configuration["Auth0:Audience"]!);
        options.ProviderOptions.DefaultScopes.Add("email");
    }).AddAccountClaimsPrincipalFactory<ArrayClaimsPrincipalFactory<RemoteUserAccount>>(); //this is needed to parse roles from array to individual roles
};

var retryPolicy = HttpPolicyExtensions
    .HandleTransientHttpError()
    .WaitAndRetryAsync(new[]
        {
            TimeSpan.FromSeconds(1),
            TimeSpan.FromSeconds(5),
            TimeSpan.FromSeconds(10)
        },
        onRetry: (outcome, timespan, retryAttempt, context) =>
        {
            context.GetLogger()?.LogWarning("Delaying for {delay}ms, then making retry {retry}.", timespan.TotalMilliseconds, retryAttempt);
        }
    );

builder.Services.AddHttpClient(GlobalConstants.PLANTCATALOG_API, client => client.BaseAddress = new Uri(serviceUrl))
                                .AddHttpMessageHandler(sp => sp.GetRequiredService<AuthorizationMessageHandler>()
                                .ConfigureHandler(authorizedUrls: new[] { serviceUrl }))
                                .AddPolicyHandler(retryPolicy);
builder.Services.AddHttpClient(GlobalConstants.IMAGEPLANTCATALOG_API, client => client.BaseAddress = new Uri(imageServiceUrl))
                                .AddHttpMessageHandler(sp => sp.GetRequiredService<AuthorizationMessageHandler>()
                                .ConfigureHandler(authorizedUrls: new[] { imageServiceUrl }))
                                .AddPolicyHandler(retryPolicy);
builder.Services.AddHttpClient(GlobalConstants.PLANTHARVEST_API, client => client.BaseAddress = new Uri(harvestServiceUrl))
                                .AddHttpMessageHandler(sp => sp.GetRequiredService<AuthorizationMessageHandler>()
                                .ConfigureHandler(authorizedUrls: new[] { harvestServiceUrl }))
                                .AddPolicyHandler(retryPolicy);
builder.Services.AddHttpClient(GlobalConstants.USERMANAGEMENT_API, client => client.BaseAddress = new Uri(userServiceUrl))
                                .AddHttpMessageHandler(sp => sp.GetRequiredService<AuthorizationMessageHandler>()
                                .ConfigureHandler(authorizedUrls: new[] { userServiceUrl }))
                                .AddPolicyHandler(retryPolicy);
builder.Services.AddHttpClient(GlobalConstants.USERMANAGEMENT_NO_AUTH, client => client.BaseAddress = new Uri(userServiceUrl))
                                .AddPolicyHandler(retryPolicy);
builder.Services.AddHttpClient(GlobalConstants.GROWCONDITIONS_API, client => client.BaseAddress = new Uri(growConditionsUrl))
                                .AddHttpMessageHandler(sp => sp.GetRequiredService<AuthorizationMessageHandler>()
                                .ConfigureHandler(authorizedUrls: new[] { growConditionsUrl }))
                                .AddPolicyHandler(retryPolicy);

builder.Services.AddValidatorsFromAssemblyContaining<PlantViewModelValidator>();

builder.Services.AddBlazoredToast();

await builder.Build().RunAsync();
