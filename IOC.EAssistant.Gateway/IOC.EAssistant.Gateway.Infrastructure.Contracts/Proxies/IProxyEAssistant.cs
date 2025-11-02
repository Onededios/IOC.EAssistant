using IOC.EAssistant.Gateway.Library.Entities.Proxies.EAssistant;

namespace IOC.EAssistant.Gateway.Infrastructure.Contracts.Proxies;
public interface IProxyEAssistant
{
    Task<ChatResponse> ChatAsync(ChatRequest body);
}
