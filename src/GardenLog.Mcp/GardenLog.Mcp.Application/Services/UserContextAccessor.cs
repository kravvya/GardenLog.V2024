using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace GardenLog.Mcp.Application.Services;

public class UserContextAccessor : IUserContextAccessor
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public UserContextAccessor(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public string? GetUserId()
    {
        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext?.User?.Identity?.IsAuthenticated == true)
        {
            // Try to find UserProfileId claim (custom claim from Auth0)
            var userProfileId = httpContext.User.FindFirst("UserProfileId")?.Value;
            if (!string.IsNullOrEmpty(userProfileId))
                return userProfileId;

            // Fallback to standard subject claim
            return httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value 
                ?? httpContext.User.FindFirst("sub")?.Value;
        }
        return null;
    }

    public string? GetJwtToken()
    {
        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext != null)
        {
            // Extract bearer token from Authorization header
            var authHeader = httpContext.Request.Headers.Authorization.FirstOrDefault();
            if (!string.IsNullOrEmpty(authHeader) && authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
            {
                return authHeader.Substring("Bearer ".Length).Trim();
            }
        }
        return null;
    }
}
