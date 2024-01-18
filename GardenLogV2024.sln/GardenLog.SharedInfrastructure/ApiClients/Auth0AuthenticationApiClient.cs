using GardenLog.SharedInfrastructure.Extensions;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using System.Text.Json.Serialization;

namespace GardenLog.SharedInfrastructure.ApiClients;

public interface IAuth0AuthenticationApiClient
{
    Task<string?> GetAccessToken(string audience);
    Task<string?> GetAccessTokenForMachineToMachine(string audience);
}

public class Auth0AuthenticationApiClient : IAuth0AuthenticationApiClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<Auth0AuthenticationApiClient> _logger;
    private readonly IMemoryCache _cache;
    private const string TOKEN_CACHE_KEY = "Token_{0}";
    private readonly AuthSettings _authSettings;
    private readonly TokenRequest _tokenRequest;
    private readonly SemaphoreSlim _semaphore;



    public Auth0AuthenticationApiClient(HttpClient httpClient, IConfigurationService configurationService, ILogger<Auth0AuthenticationApiClient> logger, IMemoryCache cache)
    {
        _httpClient = httpClient;
        _logger = logger;
        _cache = cache;
        _semaphore = new SemaphoreSlim(1, 1);

        _authSettings = configurationService.GetAuthSettings();

        _tokenRequest = new()
        {
            ClientId = _authSettings.ClientId,
            ClientSecret = _authSettings.ClientSecret,
            GrantType = "client_credentials"
        };

        _logger.LogInformation($"Auth @ {_tokenRequest}");

        if (_authSettings.Authority != null) _httpClient.BaseAddress = new Uri(_authSettings.Authority);
    }

    public async Task<string?> GetAccessToken(string audience)
    {
        string? token;
        string key = string.Format(TOKEN_CACHE_KEY, audience);

        if (!_cache.TryGetValue(key, out token))
        {
            await _semaphore.WaitAsync().ConfigureAwait(false);


            if (_cache.TryGetValue(key, out token))
            {
                return token;
            }

            var route = "oauth/token";

            _tokenRequest.Audience = audience;

            var response = await _httpClient.ApiPostAsync<TokenResponse>(route, _tokenRequest);
            if (!response.IsSuccess)
            {
                _logger.LogError($"Unable to get Token: {response.ErrorMessage}");
                return null;
            }

            token = response.Response!.AccessToken;
            _cache.Set(key, token, new MemoryCacheEntryOptions()
            {
                //set expiration to 5 min beforea actual expire
                SlidingExpiration = TimeSpan.FromSeconds(response.Response.ExpiresIn - 300)
            });

            _semaphore.Release();
        }

        return token;
    }

    public async Task<string?> GetAccessTokenForMachineToMachine(string audience)
    {
        string? token;
        string key = string.Format(TOKEN_CACHE_KEY, audience);

        if (!_cache.TryGetValue(key, out token))
        {
            await _semaphore.WaitAsync().ConfigureAwait(false);


            if (_cache.TryGetValue(key, out token))
            {
                return token;
            }

            var route = "oauth/token";

            var request = new TokenRequest()
            {
                Audience = audience,
                GrantType = _tokenRequest.GrantType,
                ClientId = _authSettings.ApiClientId,
                ClientSecret = _authSettings.ApiClientSecret
            };

            var response = await _httpClient.ApiPostAsync<TokenResponse>(route, request);
            if (!response.IsSuccess)
            {
                _logger.LogError($"Unable to get Token: {response.ErrorMessage}");
                return null;
            }

            token = response.Response!.AccessToken;
            _cache.Set(key, token, new MemoryCacheEntryOptions()
            {
                //set expiration to 5 min beforea actual expire
                SlidingExpiration = TimeSpan.FromSeconds(response.Response.ExpiresIn - 300)
            });

            _semaphore.Release();
        }

        return token;
    }
}

public record TokenRequest
{

    [JsonPropertyName("grant_type")]
    public string? GrantType { get; set; }
    [JsonPropertyName("client_id")]
    public string? ClientId { get; set; }
    [JsonPropertyName("client_secret")]
    public string? ClientSecret { get; set; }
    [JsonPropertyName("audience")]
    public string? Audience { get; set; }
}

public record TokenResponse
{
    [JsonPropertyName("access_token")]
    public string AccessToken { get; set; } = string.Empty;
    [JsonPropertyName("expires_in")]
    public int ExpiresIn { get; set; }
    [JsonPropertyName("scope")]
    public string Scope { get; set; } = string.Empty;
    [JsonPropertyName("token_type")]
    public string TokenType { get; set; } = string.Empty;
}