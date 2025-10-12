using IOC.E_Assistant.Infraestructure.Implementation.Extension;
using Microsoft.AspNetCore.WebUtilities;
using System.Text.Json;

namespace IOC.E_Assistant.WebApi.Infrastructure.Implementation;
public abstract class Proxy
{
    private readonly Uri _uri;
    protected Proxy(string baseUri)
    {
        this._uri = new Uri(baseUri);
    }

    protected async Task<T?> GETAsync<T>(
        string endpoint,
        Dictionary<string, string>? headers = null,
        Dictionary<string, string>? queryParams = null
    )
    {
        var request = BuildBaseRequest(endpoint, queryParams);
        if (headers != null) request.AddHeaders(headers);
        return await HandleResponse<T>(request);
    }

    protected async Task<TReponse> POSTAsync<TRequest, TResponse>(
        string endpoint,
        Dictionary<string, string>? headers = null,
        TRequest? body
    )
    {
        var request = BuildBaseRequest(endpoint);
        if (headers != null) request.AddHeaders(headers);
        if (body != null) request.AddBody(body);
        return HandleResponse<TResponse>(request);
    }

    private HttpRequestMessage BuildBaseRequest(
        string endpoint,
        Dictionary<string, string>? queryParams = null
    )
    {
        if (string.IsNullOrWhiteSpace(endpoint)) throw new ArgumentNullException(nameof(endpoint));

        var fullUri = this._uri.Append(endpoint).AbsoluteUri;

        if (queryParams != null) fullUri = QueryHelpers.AddQueryString(fullUri, queryParams);

        return new HttpRequestMessage(HttpMethod.Get, fullUri);
    }


    private static async Task<T?> HandleResponse<T>(HttpRequestMessage request)
    {
        var client = new HttpClient();
        using var response = await client.SendAsync(request);
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<T>(json);
    }
}