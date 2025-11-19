using IOC.EAssistant.Gateway.Infrastructure.Contracts.Proxies;
using IOC.EAssistant.Gateway.Infrastructure.Contracts.Proxies.EAssistant;
using Microsoft.Extensions.Http;

namespace IOC.EAssistant.Gateway.Infrastructure.Implementation.Proxies;

/// <summary>
/// Provides HTTP proxy functionality for communicating with the EAssistant service.
/// </summary>
/// <remarks>
/// This class implements the <see cref="IProxyEAssistant"/> interface and provides methods 
/// for interacting with the EAssistant service, including chat operations and health checks.
/// It inherits from <see cref="Proxy"/> to leverage common HTTP communication functionality.
/// Uses <see cref="IHttpClientFactory"/> for efficient HTTP client management.
/// </remarks>
public class ProxyEAssistant(string? baseUri, IHttpClientFactory httpClientFactory) 
    : Proxy(baseUri, httpClientFactory), IProxyEAssistant
{
    /// <summary>
    /// Sends a chat request to the EAssistant service and returns the response.
    /// </summary>
    /// <param name="body">The <see cref="ChatRequest"/> containing the messages and model configuration for the chat operation.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains a <see cref="ChatResponse"/> 
    /// with the generated choices, usage statistics, and optional metadata.</returns>
    /// <exception cref="HttpRequestException">Thrown when the HTTP request to the EAssistant service fails.</exception>
    /// <exception cref="InvalidOperationException">Thrown when the response deserialization returns null.</exception>
    /// <remarks>
    /// This method sends a POST request to the "/chat" endpoint with the provided chat request body.
    /// The request and response are automatically serialized/deserialized as JSON.
    /// </remarks>
    public async Task<ChatResponse> ChatAsync(ChatRequest body)
    {
        var res = await POSTAsync<ChatRequest, ChatResponse>("/chat", body: body);
        return res;
    }

    /// <summary>
    /// Performs a health check on the EAssistant service.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation. The task result contains a <see cref="HealthResponse"/> 
    /// indicating the health status of the service, including model availability and timestamp information.</returns>
    /// <exception cref="HttpRequestException">Thrown when the HTTP request to the EAssistant service fails.</exception>
    /// <exception cref="InvalidOperationException">Thrown when the response deserialization returns null.</exception>
    /// <remarks>
    /// This method sends a GET request to the "/health" endpoint to retrieve the current health status of the EAssistant service.
    /// </remarks>
    public async Task<HealthResponse> HealthCheckAsync()
    {
        var res = await GETAsync<HealthResponse>("/health");
        return res;
    }
}