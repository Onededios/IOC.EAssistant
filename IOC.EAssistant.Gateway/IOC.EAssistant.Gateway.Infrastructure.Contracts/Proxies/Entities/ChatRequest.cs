using IOC.EAssistant.Gateway.Library.Entities.Proxies.EAssistant;

namespace IOC.EAssistant.Gateway.Infrastructure.Contracts.Proxies.Entities;
public class ChatRequest
{
    public ModelConfiguration ModelConfiguration { get; set; } = new ModelConfiguration();
}