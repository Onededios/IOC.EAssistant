using IOC.EAssistant.Gateway.Infrastructure.Contracts.Proxies;
using IOC.EAssistant.Gateway.Infrastructure.Contracts.Proxies.Entities;

namespace IOC.EAssistant.Gateway.Infrastructure.Implementation.Proxies;
public class ProxyEAssistant(string? baseUri) : Proxy(baseUri), IProxyEAssistant
{
    public async Task<ChatResponse> ChatAsync(ChatRequest body)
    {
        var res = await this.POSTAsync<ChatRequest, ChatResponse>("/chat", body: body);
        return res;
    }
}