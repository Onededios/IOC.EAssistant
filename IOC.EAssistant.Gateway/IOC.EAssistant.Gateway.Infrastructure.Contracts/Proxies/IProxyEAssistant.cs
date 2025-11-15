using IOC.EAssistant.Gateway.Infrastructure.Contracts.Proxies.EAssistant;

namespace IOC.EAssistant.Gateway.Infrastructure.Contracts.Proxies;
public interface IProxyEAssistant
{
    Task<ChatResponse> ChatAsync(ChatRequest body);
    Task<HealthResponse> HealthCheckAsync();
}
