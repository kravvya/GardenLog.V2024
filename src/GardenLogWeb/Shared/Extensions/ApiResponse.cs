using System.Net;

namespace GardenLogWeb.Shared.Extensions;

public class ApiResponse
{
    public HttpStatusCode StatusCode { get; set; }

    public bool IsSuccess
    {
        get { return StatusCode == HttpStatusCode.OK || StatusCode == HttpStatusCode.Accepted; }
    }
    public string? ErrorMessage { get; internal set; }

    public Dictionary<string, string[]>? ValidationProblems { get; set; }
}
