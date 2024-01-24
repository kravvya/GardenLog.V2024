using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
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
    private const string VAULT_NAME = "key-vault-name";
    private const string ENVIRONMENT = "ASPNETCORE_ENVIRONMENT";
    private readonly IConfiguration _configuration;
    private readonly ILogger<ConfigurationService> _logger;
    private readonly SecretClient? _kvClient;
    private readonly string _pref=string.Empty;

    public ConfigurationService(IConfiguration configuration, ILogger<ConfigurationService> logger)
    {
        _configuration = configuration;
        _logger = logger;

        var env = _configuration.GetValue<string>(ENVIRONMENT);

        env ??= "Development";

        if (env == "Development" || env == "Production")
        {
            _logger.LogInformation("{env} environment. Will use kv for values", env);

            string? vaultName = _configuration.GetValue<string>(VAULT_NAME);
            if (string.IsNullOrWhiteSpace(vaultName))
            {
                _logger.LogCritical("Vault name is not found. Do not expect any good thinng to happen");
            }
            else
            {
                var kvUrl = $"https://{vaultName}.vault.azure.net/";
                _kvClient = new SecretClient(new Uri(kvUrl), new DefaultAzureCredential());
                _pref = (env == "Development" ? "test-" : "");
                _logger.LogInformation("{_pref} Will use this pref for kv values", _pref);
            }
        }
    }

    public MongoSettings? GetPlantCatalogMongoSettings()
    {
        MongoSettings? mongoSettings;
        if (_kvClient != null)
        {
            _logger.LogInformation("Will use kv for MongoDB values with prefix: {_pref}", _pref);
           
            mongoSettings = new MongoSettings
            {
                Server = _kvClient.GetSecret($"{_pref}mongodb-server").Value.Value,
                DatabaseName = _kvClient.GetSecret($"{_pref}mongodb-databasename").Value.Value,
                UserName = _kvClient.GetSecret($"{_pref}mongodb-username").Value.Value,
                Password = _kvClient.GetSecret($"{_pref}mongodb-password").Value.Value
            };

            _logger.LogInformation("MongoDB server {mongodb-server} @ {mongodb-databasename}", mongoSettings.Server, mongoSettings.DatabaseName);
        }
        else
        {
            mongoSettings = _configuration.GetSection(MongoSettings.SECTION).Get<MongoSettings>();
        }

        if (mongoSettings == null)
        {
            _logger.LogWarning("MONGODB settngs are not found. Do not expect any good things to happen");
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
        return openWeartherAppId ?? string.Empty;
    }

    public AuthSettings GetAuthSettings()
    {
        var authSettings = _configuration.GetSection(AuthSettings.SECTION).Get<AuthSettings>();
        if (_kvClient != null)
        {
            authSettings!.ClientSecret = _kvClient.GetSecret(AuthSettings.CLIENTSECRET_SECRET).Value.Value;           
        }
        else 
        {
            authSettings!.ClientSecret = _configuration.GetValue<string>(AuthSettings.CLIENTSECRET_SECRET);
        }
        authSettings.ApiClientSecret = _configuration.GetValue<string>(AuthSettings.APICLIENTSECRET_SECRET);

        if (string.IsNullOrWhiteSpace(authSettings?.Authority))
        {
            _logger.LogCritical("AUTH DOMAIN is not found. Do not expect any good things to happen");
        }
        else
        {
            _logger.LogInformation("AUTH DOMAIN WAS LOCATED! YEHAA");
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