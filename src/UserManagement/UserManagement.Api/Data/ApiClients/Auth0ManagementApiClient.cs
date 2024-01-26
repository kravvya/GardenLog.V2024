using GardenLog.SharedInfrastructure;
using GardenLog.SharedInfrastructure.ApiClients;
using GardenLog.SharedInfrastructure.Extensions;
using Microsoft.Extensions.Caching.Memory;
using System.Text.Json.Serialization;

namespace UserManagement.Api.Data.ApiClients;

public interface IAuth0ManagementApiClient
{
    Task<string> CreateUser(CreateUserProfileCommand request, string userId);
    Task<bool> UpdateUser(UpdateUserProfileCommand request, string identityId);
    Task<bool> DeleteUser(string identityId);
    Task<string> ReadUserIdByUserNameAndEmail(string userName, string email);
}

public class Auth0ManagementApiClient : IAuth0ManagementApiClient
{
    private readonly HttpClient _httpClient;
    private readonly IAuth0AuthenticationApiClient _authClient;
    private readonly ILogger<Auth0ManagementApiClient> _logger;
    private readonly string _audienceAuth0ManagementApi;

    public Auth0ManagementApiClient(HttpClient httpClient, IConfigurationService configurationService, IAuth0AuthenticationApiClient authClient, ILogger<Auth0ManagementApiClient> logger)
    {
        _httpClient = httpClient;
        _authClient = authClient;
        _logger = logger;

        var authSettings = configurationService.GetAuthSettings();
        _logger.LogInformation("Auth @ {authSettings.Authority}", authSettings.Authority);

        if (authSettings.Authority == null) throw new ArgumentException("Unable to set up Auth0 client. Authority is missing from the configuration file", "Authority");

        _httpClient.BaseAddress = new Uri(authSettings.Authority);
        _audienceAuth0ManagementApi = authSettings.Authority + "api/v2/";
    }

    public async Task<string> CreateUser(CreateUserProfileCommand request, string userId)
    {
        var userRequest = new User()
        {
            Email = request.EmailAddress,
            FamilyName = request.LastName,
            GivenName = request.FirstName,
            Name = $"{request.FirstName} {request.LastName}",
            Password = request.Password,
            UserName = request.UserName,
            UserId = userId
        };

        var route = "api/v2/users";

        string? token = await _authClient.GetAccessTokenForMachineToMachine(_audienceAuth0ManagementApi);

        if (token == null)
        {
            throw new ArgumentException("Unable to connect with security provider", "UserName");
        }
        var response = await _httpClient.ApiPostAsync<User>(route, userRequest, token);

        if (!response.IsSuccess)
        {
            throw new ArgumentException(response.ErrorMessage, "UserName");
        }

        return response.Response!.UserId;
    }

    public async Task<bool> UpdateUser(UpdateUserProfileCommand request, string iserProfileId)
    {
        var userRequest = new UserName()
        {
            FamilyName = request.LastName,
            GivenName = request.FirstName,
            Name = $"{request.FirstName} {request.LastName}"
        };

        var route = $"api/v2/users/{iserProfileId}";

        string? token = await _authClient.GetAccessTokenForMachineToMachine(_audienceAuth0ManagementApi);

        if (token == null)
        {
            throw new ArgumentException("Unable to connect with security provider", "UserName");
        }
        var response = await _httpClient.ApiPatchAsync<User>(route, userRequest, token);

        if (!response.IsSuccess)
        {
            throw new ArgumentException(response.ErrorMessage, "UserName");
        }

        return true;
    }

    public async Task<bool> DeleteUser(string userProfileId)
    {
        var route = $"api/v2/users/{userProfileId}";

        string? token = await _authClient.GetAccessTokenForMachineToMachine(_audienceAuth0ManagementApi);

        if (token == null)
        {
            throw new ArgumentException("Unable to connect with security provider", "UserName");
        }
        var response = await _httpClient.ApiDeleteAsync(route, token);

        if (!response.IsSuccess)
        {
            throw new ArgumentException(response.ErrorMessage, "UserName");
        }

        return true;
    }

    public async Task<string> ReadUserIdByUserNameAndEmail(string userName, string email)
    {

        var route = $"api/v2/users?q=username:{userName.ToLower()} AND email:{email.ToLower()}&search_engine=v3";

        string? token = await _authClient.GetAccessTokenForMachineToMachine(_audienceAuth0ManagementApi);

        if (token == null)
        {
            throw new ArgumentException("Unable to connect with security provider", "UserName");
        }
        var response = await _httpClient.ApiGetAsync<List<User>>(route, token);

        if (!response.IsSuccess)
        {
            throw new ArgumentException(response.ErrorMessage, "UserName");
        }

        return response.Response!= null && response.Response.Count > 0 ? response.Response[0].UserId : string.Empty;
    }
}


public record User : UserName
{
    [JsonPropertyName("email")]
    public string Email { get; set; } = string.Empty;
    [JsonPropertyName("password")]
    public string Password { get; set; } = string.Empty;
    [JsonPropertyName("username")]
    public string UserName { get; set; } = string.Empty;
    [JsonPropertyName("email_verified")]
    public bool EmailVerified { get; set; } = false;
    [JsonPropertyName("verify_email")]
    public bool VerifyEmail { get; set; } = true;
    [JsonPropertyName("user_id")]
    public string UserId { get; set; } = string.Empty;
    [JsonPropertyName("connection")]
    public string Connection { get; set; } = "Username-Password-Authentication";

}

public record UserName
{
    [JsonPropertyName("given_name")]
    public string GivenName { get; set; } = string.Empty;
    [JsonPropertyName("family_name")]
    public string FamilyName { get; set; } = string.Empty;
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;
}