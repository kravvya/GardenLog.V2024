namespace GardenLog.SharedInfrastructure;

public record AuthSettings
{
    public const string SECTION = "Auth0";
    public const string CLIENTSECRET_SECRET = "auth0-clientsecret";
    public const string APICLIENTSECRET_SECRET = "auth0-apiclientsecret";


    public string? Authority { get; set; }
    public string? Audience { get; set; }
    public string? ClientId { get; set; }
    public string? ApiClientId { get; set; }
    public string? ClientSecret { get; set; }
    public string? ApiClientSecret { get; set; }
    public string? SwaggerClientId { get; set; }
}