using Microsoft.AspNetCore.Http;

namespace GardenLog.Mcp.Infrastructure.Authentication;

/// <summary>
/// DelegatingHandler that forwards the user's JWT token to downstream APIs.
/// Unlike Auth0AuthenticationHandler which obtains a NEW token from Auth0,
/// this handler forwards the EXISTING token from the current request.
/// This ensures user-level data isolation is maintained through the chain:
/// Agent -> MCP Server -> Downstream API
/// </summary>
public class UserAuthenticationHandler : DelegatingHandler
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ILogger<UserAuthenticationHandler> _logger;

    public UserAuthenticationHandler(
        IHttpContextAccessor httpContextAccessor,
        ILogger<UserAuthenticationHandler> logger)
    {
        _httpContextAccessor = httpContextAccessor;
        _logger = logger;
    }

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        // Get the JWT token from the current HTTP request context
        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext != null)
        {
            var authHeader = httpContext.Request.Headers.Authorization.FirstOrDefault();
            if (!string.IsNullOrEmpty(authHeader) && authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
            {
                var token = authHeader.Substring("Bearer ".Length).Trim();
                
                // Forward the token to the downstream API
                request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
                
                _logger.LogDebug("Forwarding JWT token to downstream API: {Uri}", request.RequestUri);
            }
            else
            {
                _logger.LogWarning("No Authorization header found in request to forward");
            }
        }

        return await base.SendAsync(request, cancellationToken);
    }
}
