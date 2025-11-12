using IOC.EAssistant.Gateway.Library.Entities.Proxies.EAssistant;
using IOC.EAssistant.Gateway.Library.Entities.Proxies.EAssistant.Chat;

namespace IOC.EAssistant.Gateway.Infrastructure.Contracts.Proxies.EAssistant;
public class ChatRequest : ChatRequestDto
{
    public ModelConfiguration? ModelConfiguration { get; set; }

}
