using IOC.EAssistant.Gateway.Infrastructure.Contracts.Proxies.EAssistant;

namespace IOC.EAssistant.Gateway.Infrastructure.Contracts.Proxies;
/// <summary>
/// Defines methods for interacting with a proxy-based assistant service, including chat functionality and health
/// checks.
/// </summary>
/// <remarks>This interface provides asynchronous methods for sending chat requests and performing health checks
/// on the service. Implementations of this interface are expected to handle communication with the underlying assistant
/// service.</remarks>
public interface IProxyEAssistant
{
    Task<ChatResponse> ChatAsync(ChatRequest body);
    Task<HealthResponse> HealthCheckAsync();
}
