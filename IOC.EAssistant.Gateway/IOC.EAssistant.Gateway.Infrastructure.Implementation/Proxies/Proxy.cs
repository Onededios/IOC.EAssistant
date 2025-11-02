using System.Text;
using System.Text.Json;
using IOC.EAssistant.Gateway.Infrastructure.Implementation.Extension;
using Microsoft.AspNetCore.WebUtilities;

namespace IOC.EAssistant.Gateway.Infrastructure.Implementation.Proxies;
public abstract class Proxy
{
    private readonly Uri _uri;
    protected Proxy(string baseUri)
    {
        this._uri = new Uri(baseUri);
    }

    protected async Task<TResponse> GETAsync<TResponse>(
        string endpoint,
        Dictionary<string, string>? headers = null,
        Dictionary<string, string>? queryParams = null
    )
    {
        var request = new HttpRequestMessage(HttpMethod.Get, BuildUri(endpoint, queryParams));
        if (headers != null) request.AddHeaders(headers);
        return await HandleResponse<TResponse>(request);
    }

    protected async Task<TResponse> POSTAsync<TRequest, TResponse>(
        string endpoint,
        TRequest body,
        Dictionary<string, string>? queryParams = null,
        Dictionary<string, string>? headers = null
    )
    {
        var request = new HttpRequestMessage(HttpMethod.Post, BuildUri(endpoint, queryParams));

        if (headers != null) request.AddHeaders(headers);
        request.Content = BuildContent(body);

        return await HandleResponse<TResponse>(request);
    }

    private Uri BuildUri(string endpoint, Dictionary<string, string>? queryParams = null)
    {
        if (string.IsNullOrWhiteSpace(endpoint)) throw new ArgumentNullException(nameof(endpoint));
        var fullUri = this._uri.Append(endpoint).AbsoluteUri;
        if (queryParams != null) fullUri = QueryHelpers.AddQueryString(fullUri, queryParams);
        return new Uri(fullUri);
    }

    private static StringContent BuildContent<TRequest>(TRequest body) => new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json");

    private static async Task<T> HandleResponse<T>(HttpRequestMessage request)
    {
        var client = new HttpClient();
        using var response = await client.SendAsync(request);
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<T>(json);
        if (result is null) throw new InvalidOperationException("Deserialization returned null.");
        return result;
    }
}
