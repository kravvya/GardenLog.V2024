using Microsoft.AspNetCore.Components.WebAssembly.Authentication;

namespace GardenLogWeb.Services.Auth;

public class AccessTokenService : IAccessTokenService
{
    private readonly IAccessTokenProvider _tokenProvider;

    public AccessTokenService(IAccessTokenProvider tokenProvider)
    {
        _tokenProvider = tokenProvider;
    }

    public async Task<string?> GetCurrentAccessTokenAsync()
    {
        try
        {
            var tokenResult = await _tokenProvider.RequestAccessToken();

            if (tokenResult.TryGetToken(out var token))
            {
                return token.Value;
            }

            return null;
        }
        catch
        {
            return null;
        }
    }
}
