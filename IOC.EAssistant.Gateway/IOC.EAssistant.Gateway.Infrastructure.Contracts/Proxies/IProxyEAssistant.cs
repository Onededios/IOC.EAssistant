using IOC.EAssistant.Gateway.Infrastructure.Contracts.Proxies.EAssistant;
using IOC.EAssistant.Gateway.Library.Entities.Proxies.EAssistant.Chat;

namespace IOC.EAssistant.Gateway.Infrastructure.Contracts.Proxies;
public interface IProxyEAssistant
{
    Task<ChatResponse> ChatAsync(ChatRequest body);
}
