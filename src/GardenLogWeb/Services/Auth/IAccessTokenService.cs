namespace GardenLogWeb.Services.Auth;

public interface IAccessTokenService
{
    Task<string?> GetCurrentAccessTokenAsync();
}
