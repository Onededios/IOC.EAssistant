using IOC.EAssistant.Gateway.Infrastructure.Contracts.Proxies.Entities;

namespace IOC.EAssistant.Gateway.Infrastructure.Contracts.Proxies;
public interface IProxyEAssistant
{
    Task<ChatResponse> ChatAsync(ChatRequest body);
}
