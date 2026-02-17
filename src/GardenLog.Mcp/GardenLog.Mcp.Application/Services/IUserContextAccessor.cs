namespace GardenLog.Mcp.Application.Services;

public interface IUserContextAccessor
{
    /// <summary>
    /// Gets the UserProfileId from the current authenticated user's claims
    /// </summary>
    string? GetUserId();
    
    /// <summary>
    /// Gets the JWT token from the Authorization header
    /// </summary>
    string? GetJwtToken();
}
