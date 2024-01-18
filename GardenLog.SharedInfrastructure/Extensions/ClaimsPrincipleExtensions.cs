using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace GardenLog.SharedInfrastructure.Extensions;

public static class ClaimsPrincipleExtensions
{
    public static string GetUserProfileIdAbsolete(this ClaimsPrincipal principal)
    {
        if (principal == null) return "auth0|up1";
        // throw new ArgumentNullException(nameof(principal));

        var userName = principal.FindFirstValue(ClaimTypes.Sid);

        return userName ?? "auth0|up1";
    }

   public static string GetUserProfileId(this ClaimsPrincipal principal, IHeaderDictionary headers)
    {
        var deafultuser = "";

        deafultuser = principal.FindFirstValue(ClaimTypes.NameIdentifier);

        if ((string.IsNullOrEmpty(deafultuser) || deafultuser .Contains("@clients" )) && headers != null)
        {
            deafultuser = headers.FirstOrDefault(h => h.Key == "RequestUser").Value;
        }

        if (string.IsNullOrEmpty(deafultuser)) deafultuser = "auth0|up1";

        //to get to gl id, need to strip out autho prefix.
       return deafultuser;

        // throw new ArgumentNullException(nameof(principal));

        //var userName = principal.FindFirstValue(ClaimTypes.NameIdentifier);

        //return userName ?? deafultuser;
    }
}
