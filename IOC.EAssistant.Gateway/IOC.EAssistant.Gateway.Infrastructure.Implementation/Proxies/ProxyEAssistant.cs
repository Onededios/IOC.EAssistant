using IOC.EAssistant.Gateway.Library.Entities.Proxies.EAssistant;
using IOC.EAssistant.Gateway.Infrastructure.Contracts.Proxies;

namespace IOC.EAssistant.Gateway.Infrastructure.Implementation.Proxies;
public class ProxyEAssistant(string? baseUri) : Proxy(baseUri), IProxyEAssistant
{
    public async Task<ChatResponse> ChatAsync(ChatRequest body)
    {
        var res = await this.POSTAsync<ChatRequest, ChatResponse>("/chat", body: body);
        return res;
    }
}