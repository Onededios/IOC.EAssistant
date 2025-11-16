using IOC.EAssistant.Gateway.Library.Entities.Proxies.EAssistant;

namespace IOC.EAssistant.Gateway.Infrastructure.Contracts.Proxies.EAssistant;
public class ChatRequest
{
    public required IEnumerable<ChatMessage> Messages { get; set; }
    public ModelConfiguration ModelConfiguration { get; set; } = new ModelConfiguration();
}
