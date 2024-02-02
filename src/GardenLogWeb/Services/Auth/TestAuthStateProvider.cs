using Microsoft.AspNetCore.Components.Authorization;
using System.Security.Claims;

namespace GardenLogWeb.Services.Auth;

public class TestAuthStateProvider : AuthenticationStateProvider
{
    public async override Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        var claims = new List<Claim>
        {
            new Claim("sup", "up1"),
            new Claim(ClaimTypes.Name, "Test User"),
            new Claim(ClaimTypes.Role, "master-gardener")

        };
        var testUser = new ClaimsIdentity(claims, "testAuthType");
        return await Task.FromResult(new AuthenticationState(new ClaimsPrincipal(testUser)));
    }
}
