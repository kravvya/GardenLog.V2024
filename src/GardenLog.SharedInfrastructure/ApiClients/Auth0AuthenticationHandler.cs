using System.Net.Http.Headers;

namespace GardenLog.SharedInfrastructure.ApiClients;

public class Auth0AuthenticationHandler : DelegatingHandler
{
    private readonly IAuth0AuthenticationApiClient _authApiClient;
    private readonly string _audience;

    public Auth0AuthenticationHandler(IAuth0AuthenticationApiClient authApiClient, IConfigurationService configService)
    {
        _authApiClient = authApiClient;
        _audience = configService.GetAuthSettings().Audience!;
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        if (request.Headers.Authorization?.Parameter == null)
        {
            var accessToken = await _authApiClient.GetAccessToken(_audience).ConfigureAwait(false);

            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        }

        return await base.SendAsync(request, cancellationToken).ConfigureAwait(false);
    }
}