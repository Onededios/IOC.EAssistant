using IOC.E_Assistant.Infraestructure.Implementation.Proxies.EAssistant.Chat;
using IOC.E_Assistant.WebApi.Infrastructure.Implementation;

namespace IOC.E_Assistant.Infraestructure.Implementation.Proxies.EAssistant;
public class EAssistantProxy : Proxy
{
    EAssistantProxy(IConfiguration configuration) : base(configuration["EASSISTANT_API"]) { }
    public Task<ChatResponse> SendMessageAsync(ChatResponse body)
    {
        var res = await this.POSTAsync<ChatRequest, ChatResponse>("/chat", null, body);
        return res;
    }
}
