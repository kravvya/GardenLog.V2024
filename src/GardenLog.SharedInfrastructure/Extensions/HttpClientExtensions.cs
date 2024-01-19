using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace GardenLog.SharedInfrastructure.Extensions;

public static class HttpClientExtensions
{
    public static readonly JsonSerializerOptions OPTIONS = new JsonSerializerOptions() { PropertyNameCaseInsensitive = true };

    static HttpClientExtensions()
    {
        OPTIONS.Converters.Add(new JsonStringEnumConverter());
    }

    public static async Task<ApiObjectResponse<TObject>> ApiGetAsync<TObject>(this HttpClient httpClient, string requestUri)
        where TObject : new()
    {
        var result = await httpClient.GetAsync(requestUri);

        return await ProcessHttpResponse<TObject>(result);

    }

    public static async Task<ApiObjectResponse<TObject>> ApiGetAsync<TObject>(this HttpClient httpClient, string requestUri, string bearerToken)
        where TObject : new()
    {
        var httpRequestMessage = new HttpRequestMessage
        {
            Method = HttpMethod.Get,
            RequestUri = new Uri(requestUri, UriKind.Relative),
            Headers = {
            { HttpRequestHeader.Authorization.ToString(), $"Bearer {bearerToken}" },
            { HttpRequestHeader.Accept.ToString(), "application/json" }
            }
        };

        var result = await httpClient.SendAsync(httpRequestMessage);

        return await ProcessHttpResponse<TObject>(result);

    }

    public static async Task<ApiObjectResponse<string>> ApiPostAsync(this HttpClient httpClient, string requestUri, object content)
    {
        var result = await httpClient.PostAsync(requestUri, GenerateByteContent(content));
        var response = await ProcessHttpResponse<string>(result);

        if (response.IsSuccess) response.Response = await result.Content.ReadAsStringAsync();

        return response;
    }

    public static async Task<ApiObjectResponse<string>> ApiPostAsync(this HttpClient httpClient, string requestUri, object content, List<KeyValuePair<string, string>> headers)
    {
        var httpRequestMessage = new HttpRequestMessage
        {
            Method = HttpMethod.Post,
            RequestUri = new Uri(requestUri, UriKind.Relative),
            Content = GenerateByteContent(content)
        };

        headers.ForEach(i => httpRequestMessage.Headers.Add(i.Key, i.Value));

        var result = await httpClient.SendAsync(httpRequestMessage);
        var response = await ProcessHttpResponse<string>(result);

        if (response.IsSuccess) response.Response = await result.Content.ReadAsStringAsync();

        return response;
    }

    

    public static async Task<ApiObjectResponse<TObject>> ApiPostAsync<TObject>(this HttpClient httpClient, string requestUri, object content)
        where TObject : new()
    {
        var result = await httpClient.PostAsync(requestUri, GenerateByteContent(content));

        return await ProcessHttpResponse<TObject>(result);
    }

    public static async Task<ApiObjectResponse<TObject>> ApiPostAsync<TObject>(this HttpClient httpClient, string requestUri, object content, string bearerToken)
    {
        var httpRequestMessage = new HttpRequestMessage
        {
            Method = HttpMethod.Post,
            RequestUri = new Uri(requestUri, UriKind.Relative),
            Headers = {
            { HttpRequestHeader.Authorization.ToString(), $"Bearer {bearerToken}" },
            { HttpRequestHeader.Accept.ToString(), "application/json" }
        },
            Content = GenerateByteContent(content)
        };

        var result = await httpClient.SendAsync(httpRequestMessage);
        return await ProcessHttpResponse<TObject>(result);
    }

    public static async Task<ApiResponse> ApiPutAsync(this HttpClient httpClient, string requestUri, object content)
    {
        var result = await httpClient.PutAsync(requestUri, GenerateByteContent(content));
        var response = await ProcessHttpResponse<string>(result);

        if (response.IsSuccess) response.Response = await result.Content.ReadAsStringAsync();

        return response;
    }

    public static async Task<ApiResponse> ApiDeleteAsync(this HttpClient httpClient, string requestUri)
    {
        var result = await httpClient.DeleteAsync(requestUri);

        var response = await ProcessHttpResponse<string>(result);

        if (response.IsSuccess) response.Response = await result.Content.ReadAsStringAsync();

        return response;
    }

    public static async Task<ApiResponse> ApiDeleteAsync(this HttpClient httpClient, string requestUri, string bearerToken)
    {
        var httpRequestMessage = new HttpRequestMessage
        {
            Method = HttpMethod.Delete,
            RequestUri = new Uri(requestUri, UriKind.Relative),
            Headers = {
            { HttpRequestHeader.Authorization.ToString(), $"Bearer {bearerToken}" },
            { HttpRequestHeader.Accept.ToString(), "application/json" }
            }
        };

        var result = await httpClient.SendAsync(httpRequestMessage);

        var response = await ProcessHttpResponse<string>(result);

        return response;
    }

    public static async Task<ApiObjectResponse<TObject>> ApiPatchAsync<TObject>(this HttpClient httpClient, string requestUri, object content, string bearerToken)
    {
        var httpRequestMessage = new HttpRequestMessage
        {
            Method = HttpMethod.Patch,
            RequestUri = new Uri(requestUri, UriKind.Relative),
            Headers = {
            { HttpRequestHeader.Authorization.ToString(), $"Bearer {bearerToken}" },
            { HttpRequestHeader.Accept.ToString(), "application/json" }
        },
            Content = GenerateByteContent(content)
        };

        var result = await httpClient.SendAsync(httpRequestMessage);
        return await ProcessHttpResponse<TObject>(result);
    }

    private static ByteArrayContent GenerateByteContent(object content)
    {
        var myContent = JsonSerializer.Serialize(content);
        var buffer = System.Text.Encoding.UTF8.GetBytes(myContent);
        var byteContent = new ByteArrayContent(buffer);

        byteContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");
        byteContent.Headers.ContentType.Parameters.Add(new NameValueHeaderValue("charset", "utf-8"));
        return byteContent;
    }

    private static async Task<ApiObjectResponse<TObject>> ProcessHttpResponse<TObject>(HttpResponseMessage result)
    {
        ApiObjectResponse<TObject> response = new();

        response.StatusCode = result.StatusCode;

        if (result.IsSuccessStatusCode && !typeof(TObject).ToString().Equals("System.String"))
        {
            response.Response = await result.Content.ReadFromJsonAsync<TObject>(OPTIONS);
        }
        else if (result.StatusCode == HttpStatusCode.BadRequest)
        {
            response.ValidationProblems = await result.Content.ReadFromJsonAsync<Dictionary<string, string[]>>();
        }
        else if (result.StatusCode == HttpStatusCode.NotFound)
        {
            response.ErrorMessage = "Requested resource is not found";
        }
        else
        {
            response.ErrorMessage = await result.Content.ReadAsStringAsync();
        }

        return response;
    }
}
