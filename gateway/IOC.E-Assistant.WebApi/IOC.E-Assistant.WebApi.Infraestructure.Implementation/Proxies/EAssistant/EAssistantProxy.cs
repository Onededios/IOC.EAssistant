using IOC.E_Assistant.Infrastructure.Contracts.Proxy;
using IOC.E_Assistant.WebApi.Infrastructure.Implementation;
using IOC.E_Assistant.WebApi.Library.Models.Entities;
using Microsoft.Extensions.Configuration;

namespace IOC.E_Assistant.Infrastructure.Implementation.Proxies.EAssistant;
public class EAssistantProxy(IConfiguration configuration) : Proxy(configuration["EASSISTANT_API"]), IEAssistantProxy
{
    public async Task<ChatResponse> ChatAsync(ChatRequest body)
    {
        var res = await this.POSTAsync<ChatRequest, ChatResponse>("/chat", body: body);
        return res;
    }
}
