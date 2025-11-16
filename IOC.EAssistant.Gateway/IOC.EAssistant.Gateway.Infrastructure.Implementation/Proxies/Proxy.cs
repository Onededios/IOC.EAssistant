using IOC.EAssistant.Gateway.Infrastructure.Implementation.Extension;
using Microsoft.AspNetCore.WebUtilities;
using System.Text;
using System.Text.Json;

namespace IOC.EAssistant.Gateway.Infrastructure.Implementation.Proxies;

/// <summary>
/// Abstract base class for HTTP proxy implementations that provides common functionality for making HTTP requests.
/// </summary>
/// <remarks>
/// This class handles HTTP communication with external services, including request building, 
/// JSON serialization/deserialization, and response handling. All derived proxy classes inherit 
/// these capabilities with consistent JSON formatting and error handling.
/// </remarks>
public abstract class Proxy
{
    private readonly Uri _uri;
    private readonly static JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
    };

    /// <summary>
    /// Initializes a new instance of the <see cref="Proxy"/> class with the specified base URI.
    /// </summary>
    /// <param name="baseUri">The base URI for the external service. Must be a valid, non-empty URI string.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="baseUri"/> is null or empty.</exception>
    protected Proxy(string? baseUri)
    {
        if (string.IsNullOrEmpty(baseUri))
            throw new ArgumentNullException(nameof(baseUri), "Base uri string cannot be null or empty.");
        _uri = new Uri(baseUri);
    }

    /// <summary>
    /// Sends an HTTP GET request to the specified endpoint and deserializes the response.
    /// </summary>
    /// <typeparam name="TResponse">The type to deserialize the response body into.</typeparam>
    /// <param name="endpoint">The relative endpoint path to append to the base URI.</param>
    /// <param name="headers">Optional dictionary of HTTP headers to include in the request.</param>
    /// <param name="queryParams">Optional dictionary of query parameters to append to the URI.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the deserialized response of type <typeparamref name="TResponse"/>.</returns>
    /// <exception cref="HttpRequestException">Thrown when the HTTP request fails.</exception>
    /// <exception cref="InvalidOperationException">Thrown when deserialization returns null.</exception>
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

    /// <summary>
    /// Sends an HTTP POST request with a JSON body to the specified endpoint and deserializes the response.
    /// </summary>
    /// <typeparam name="TRequest">The type of the request body to serialize.</typeparam>
    /// <typeparam name="TResponse">The type to deserialize the response body into.</typeparam>
    /// <param name="endpoint">The relative endpoint path to append to the base URI.</param>
    /// <param name="body">The request body object to serialize as JSON.</param>
    /// <param name="queryParams">Optional dictionary of query parameters to append to the URI.</param>
    /// <param name="headers">Optional dictionary of HTTP headers to include in the request.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the deserialized response of type <typeparamref name="TResponse"/>.</returns>
    /// <exception cref="HttpRequestException">Thrown when the HTTP request fails.</exception>
    /// <exception cref="InvalidOperationException">Thrown when deserialization returns null.</exception>
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

    /// <summary>
    /// Builds a complete URI by combining the base URI with the endpoint and optional query parameters.
    /// </summary>
    /// <param name="endpoint">The relative endpoint path to append to the base URI.</param>
    /// <param name="queryParams">Optional dictionary of query parameters to append to the URI.</param>
    /// <returns>A <see cref="Uri"/> instance representing the complete request URI.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="endpoint"/> is null or whitespace.</exception>
    private Uri BuildUri(string endpoint, Dictionary<string, string>? queryParams = null)
    {
        if (string.IsNullOrWhiteSpace(endpoint)) throw new ArgumentNullException(nameof(endpoint));
        var fullUri = _uri.Append(endpoint).AbsoluteUri;
        if (queryParams != null) fullUri = AddQueryParams(fullUri, queryParams);
        return new Uri(fullUri);
    }

    /// <summary>
    /// Adds query parameters to a URI string.
    /// </summary>
    /// <param name="uri">The base URI string.</param>
    /// <param name="queryParams">A dictionary of query parameter names and values.</param>
    /// <returns>The URI string with query parameters appended.</returns>
    private static string AddQueryParams(string uri, Dictionary<string, string> queryParams)
    {
        IDictionary<string, string?> nullableQueryParams = queryParams.ToDictionary(kvp => kvp.Key, kvp => (string?)kvp.Value);
        return QueryHelpers.AddQueryString(uri, nullableQueryParams);
    }

    /// <summary>
    /// Creates HTTP content from a request body object by serializing it to JSON.
    /// </summary>
    /// <typeparam name="TRequest">The type of the request body object.</typeparam>
    /// <param name="body">The request body object to serialize.</param>
    /// <returns>A <see cref="StringContent"/> instance containing the JSON-serialized body with UTF-8 encoding and application/json content type.</returns>
    private static StringContent BuildContent<TRequest>(TRequest body) => new StringContent(JsonSerializer.Serialize(body, _jsonOptions), Encoding.UTF8, "application/json");

    /// <summary>
    /// Sends an HTTP request and handles the response by deserializing it to the specified type.
    /// </summary>
    /// <typeparam name="T">The type to deserialize the response body into.</typeparam>
    /// <param name="request">The HTTP request message to send.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the deserialized response of type <typeparamref name="T"/>.</returns>
    /// <exception cref="HttpRequestException">Thrown when the HTTP response indicates failure (non-success status code).</exception>
    /// <exception cref="InvalidOperationException">Thrown when deserialization returns null.</exception>
    /// <remarks>
    /// This method creates a new <see cref="HttpClient"/> for each request, sends the request, 
    /// ensures the response has a success status code, and deserializes the JSON response body.
    /// </remarks>
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
