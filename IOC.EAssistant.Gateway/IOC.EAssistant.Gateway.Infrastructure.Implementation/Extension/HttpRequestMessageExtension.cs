namespace IOC.EAssistant.Gateway.Infrastructure.Implementation.Extension;

/// <summary>
/// Provides extension methods for <see cref="HttpRequestMessage"/> to simplify common HTTP request operations.
/// </summary>
public static class HttpRequestMessagesExtension
{
    /// <summary>
    /// Adds multiple headers to the HTTP request message without validation.
    /// </summary>
    /// <param name="request">The <see cref="HttpRequestMessage"/> to which headers will be added.</param>
    /// <param name="headers">A dictionary containing the header names and values to add to the request.</param>
    /// <returns>The same <see cref="HttpRequestMessage"/> instance with the headers added, enabling method chaining.</returns>
    /// <remarks>
    /// This method uses <see cref="System.Net.Http.Headers.HttpHeaders.TryAddWithoutValidation(string, string)"/> 
    /// to add headers, which allows adding headers that might not pass standard validation rules.
    /// If a header already exists, it will not be overwritten.
    /// </remarks>
    public static HttpRequestMessage AddHeaders(this HttpRequestMessage request, Dictionary<string, string> headers)
    {
        foreach (var header in headers)
        {
            request.Headers.TryAddWithoutValidation(header.Key, header.Value);
        }

        return request;
    }
}