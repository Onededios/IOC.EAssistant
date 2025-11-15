using System.Text;
using System.Text.Json;
using IOC.EAssistant.Gateway.Infrastructure.Implementation.Extension;
using Microsoft.AspNetCore.WebUtilities;

namespace IOC.EAssistant.Gateway.Infrastructure.Implementation.Proxies;
public abstract class Proxy
{
    private readonly Uri _uri;
    private readonly static JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
    };
    protected Proxy(string? baseUri)
    {
        if (string.IsNullOrEmpty(baseUri))
            throw new ArgumentNullException(nameof(baseUri), "Base uri string cannot be null or empty.");
        _uri = new Uri(baseUri);
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
        var fullUri = _uri.Append(endpoint).AbsoluteUri;
        if (queryParams != null) fullUri = AddQueryParams(fullUri, queryParams);
        return new Uri(fullUri);
    }

    private static string AddQueryParams(string uri, Dictionary<string, string> queryParams)
    {
        IDictionary<string, string?> nullableQueryParams = queryParams.ToDictionary(kvp => kvp.Key, kvp => (string?)kvp.Value);
        return QueryHelpers.AddQueryString(uri, nullableQueryParams);
    }

    private static StringContent BuildContent<TRequest>(TRequest body) => new StringContent(JsonSerializer.Serialize(body, _jsonOptions), Encoding.UTF8, "application/json");

    private static async Task<T> HandleResponse<T>(HttpRequestMessage request)
    {
        var client = new HttpClient();
        using var response = await client.SendAsync(request);
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<T>(json, _jsonOptions);
        if (result is null) throw new InvalidOperationException("Deserialization returned null.");
        return result;
    }
}
