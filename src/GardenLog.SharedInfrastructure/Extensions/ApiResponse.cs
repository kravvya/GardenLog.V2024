using System.Net;

namespace GardenLog.SharedInfrastructure.Extensions;

public class ApiResponse
{
    public HttpStatusCode StatusCode { get; set; }

    public bool IsSuccess
    {
        get { return StatusCode == HttpStatusCode.OK || StatusCode == HttpStatusCode.Accepted || StatusCode == HttpStatusCode.Created || StatusCode == HttpStatusCode.NoContent; }
    }
    public string ErrorMessage { get; internal set; } = string.Empty;

    public Dictionary<string, string[]>? ValidationProblems { get; set; }
}
