using DnsClient.Internal;
using GardenLog.SharedInfrastructure.MongoDB;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace GardenLog.SharedInfrastructure;

public interface IConfigurationService
{
    AuthSettings GetAuthSettings();
    string GetEmailPassword();
    string GetImageBlobConnectionString();
    MongoSettings? GetImageCatalogMongoSettings();
    string GetOpenWeartherApplicationId();
    MongoSettings? GetPlantCatalogMongoSettings();
}

public class ConfigurationService : IConfigurationService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<ConfigurationService> _logger;

    public ConfigurationService(IConfiguration configuration, ILogger<ConfigurationService> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public MongoSettings? GetPlantCatalogMongoSettings()
    {
        var mongoSettings = _configuration.GetSection(MongoSettings.SECTION).Get<MongoSettings>();
        if (mongoSettings == null)
        {
            _logger.LogWarning("MONGODB settngs are not found. Do not expect any good thinng to happen");
            return null;
        }

        if (string.IsNullOrWhiteSpace(mongoSettings.Password))
        {
            _logger.LogWarning("DB PASSWORD is not found. Will try environment");
            mongoSettings.Password = _configuration.GetValue<string>(MongoSettings.PASSWORD_SECRET);
        }

        if (string.IsNullOrWhiteSpace(mongoSettings.Password))
        {
            _logger.LogCritical("DB PASSWORD is not found. Do not expect any good things to happen");
        }
        else
        {
            _logger.LogInformation("DB PASSWORD WAS LOCATED! YEHAA");
        }
        return mongoSettings;
    }

    public MongoSettings? GetImageCatalogMongoSettings()
    {
        var mongoSettings = _configuration.GetSection(MongoSettings.SECTION).Get<MongoSettings>();

        if (mongoSettings == null)
        {
            _logger.LogWarning("MONGODB settngs are not found. Do not expect any good thinng to happen");
            return null;
        }

        if (string.IsNullOrWhiteSpace(mongoSettings.Password))
        {
            _logger.LogWarning("DB PASSWORD is not found. Will try environment");
            mongoSettings.Password = _configuration.GetValue<string>(MongoSettings.PASSWORD_SECRET);
        }

        if (string.IsNullOrWhiteSpace(mongoSettings.Password))
        {
            _logger.LogCritical("DB PASSWORD is not found. Do not expect any good things to happen");
        }
        else
        {
            _logger.LogInformation("DB PASSWORD WAS LOCATED! YEHAA");
        }
        return mongoSettings;
    }

    public string GetImageBlobConnectionString()
    {
        var blobConnectionString = _configuration.GetValue<string>("glimages-url");
        if (string.IsNullOrWhiteSpace(blobConnectionString))
        {
            _logger.LogCritical("Image Blob Url is not found. Do not expect any good things to happen");
        }
        else
        {
            _logger.LogInformation("IMAGE BLBL URL WAS FOUND! YEHAA");
        }
        return blobConnectionString ?? string.Empty;
    }

    public string GetOpenWeartherApplicationId()
    {
        var openWeartherAppId = _configuration.GetValue<string>("openweather-appid");

        if (string.IsNullOrWhiteSpace(openWeartherAppId))
        {
            _logger.LogCritical("OpenWeatherAppId is not found. Do not expect any good things to happen");
        }
        else
        {
            _logger.LogInformation("OPEN WEATHER APP ID WAS LOCATED! YEHAA");
        }
        return openWeartherAppId??string.Empty;
    }

    public AuthSettings GetAuthSettings()
    {
        var authSettings = _configuration.GetSection(AuthSettings.SECTION).Get<AuthSettings>();
        authSettings!.ClientSecret = _configuration.GetValue<string>(AuthSettings.CLIENTSECRET_SECRET);
        authSettings.ApiClientSecret = _configuration.GetValue<string>(AuthSettings.APICLIENTSECRET_SECRET);

        if (string.IsNullOrWhiteSpace(authSettings?.Authority))
        {
            _logger.LogCritical("AUTH DOMAIN is not found. Do not expect any good things to happen");
        }
        else
        {
            _logger.LogInformation("AUTH DOMAIN WAS LOCATED! YEHAA");
            _logger.LogInformation($"AUTH DOMAIN: {authSettings.Authority} AUDIENCE: {authSettings.Audience}  AUDIENCE: {authSettings.Audience} CLIENT: {authSettings.ClientId} SECRET: {authSettings.ClientSecret}");
        }
        return authSettings!;
    }

    public string GetEmailPassword()
    {
        var password = _configuration.GetValue<string>("email-password");
        

        if (string.IsNullOrWhiteSpace(password))
        {
            _logger.LogCritical("EMAIL PASSWORD is not found. Do not expect any good things to happen");
        }
        else
        {
            _logger.LogInformation("EMAIL PASSWORD WAS LOCATED! YEHAA");
        }
        return password!;
    }

}