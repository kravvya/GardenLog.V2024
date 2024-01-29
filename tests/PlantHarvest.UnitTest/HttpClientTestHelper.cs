using System.Net;
using System.Text.Json;

namespace PlantHarvest.UnitTest;

public static class HttpClientTestHelper
{
    //
    // Summary:
    //     This will give you an HttpClient where any Http verb call returns your specified
    //     response. Optionally, you may also specify a base url. If you don't, it defaults
    //     to "http://url.com".
    //
    // Parameters:
    //   expectedResponse:
    //     The response that you'd like to get back from an HTTP call.
    //
    //   baseUrl:
    //     (Optional) Sets the HttpClient BaseAddress to this.
    //
    // Returns:
    //     An HttpClient that will return your expected response.
    public static HttpClient GetMockedHttpClient(HttpResponseMessage expectedResponse, Uri baseUrl)
    {
        return new HttpClient(new MockHttpMessageHandler(expectedResponse))
        {
            BaseAddress = baseUrl
        };
    }

    //
    // Summary:
    //     This will give you an HttpClient where any Http verb call returns your specified
    //     response. Optionally, you may also specify a base url. If you don't, it defaults
    //     to "http://url.com".
    //
    // Parameters:
    //   expectedStatusCode:
    //
    //   expectedResponseContent:
    //
    //   baseUrl:
    public static HttpClient GetMockedHttpClient(HttpStatusCode expectedStatusCode, string expectedResponseContent, Uri baseUrl)
    {
        return new HttpClient(new MockHttpMessageHandler(new HttpResponseMessage
        {
            StatusCode = expectedStatusCode,
            Content = new StringContent(expectedResponseContent)
        }))
        {
            BaseAddress = baseUrl
        };
    }

    //
    // Summary:
    //     This will give you an HttpClient where any Http verb call returns your specified
    //     response. Optionally, you may also specify a base url. If you don't, it defaults
    //     to "http://url.com".
    //
    // Parameters:
    //   expectedStatusCode:
    //
    //   expectedResponseObject:
    //
    //   baseUrl:
    public static HttpClient GetMockedHttpClient(HttpStatusCode expectedStatusCode, object expectedResponseObject, Uri baseUrl)
    {
        return new HttpClient(new MockHttpMessageHandler(new HttpResponseMessage
        {
            StatusCode = expectedStatusCode,
            Content = new StringContent(JsonSerializer.Serialize(expectedResponseObject))
        }))
        {
            BaseAddress = baseUrl
        };
    }

    public static HttpClient GetMockedHttpClient(HttpStatusCode expectedStatusCode, IList<KeyValuePair<string, string>> expectedResponses, Uri baseUrl)
    {
        Dictionary<string, HttpResponseMessage> responses = new();
        foreach (var item in expectedResponses)
        {
            responses.Add(item.Key, (new HttpResponseMessage
            {
                StatusCode = expectedStatusCode,
                Content = new StringContent(item.Value)
            }));
        }

        return new HttpClient(new MockHttpMessageHandler(responses))
        {
            BaseAddress = baseUrl
        };
    }

    public static HttpClient GetMockedHttpClient(HttpRequestException expectedException, Uri baseUrl)
    {
        return new HttpClient(new MockHttpMessageHandler(expectedException))
        {
            BaseAddress = baseUrl
        };
    }
}

internal class MockHttpMessageHandler : HttpMessageHandler
{
    private readonly HttpResponseMessage? _expectedResponse;

    private readonly Dictionary<string, HttpResponseMessage>? _expectedResponses;

    private readonly HttpRequestException? _expectedException;

    public MockHttpMessageHandler(HttpResponseMessage expectedResponse)
    {
        _expectedResponse = expectedResponse;
    }

    public MockHttpMessageHandler(Dictionary<string, HttpResponseMessage> expectedResponses)
    {
        _expectedResponses = expectedResponses;
    }

    public MockHttpMessageHandler(HttpRequestException expectedException)
    {
        _expectedException = expectedException;
    }

    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        if (_expectedException != null)
        {
            throw _expectedException;
        }
              

        HttpResponseMessage response;

        if (_expectedResponses != null && _expectedResponses.ContainsKey(request.RequestUri!.AbsolutePath))
        {
            response = _expectedResponses[request.RequestUri.AbsolutePath];
        }
        else
        {
            response = _expectedResponse!;
        }

        response.RequestMessage = request;

        return Task.FromResult(response);
    }
}
