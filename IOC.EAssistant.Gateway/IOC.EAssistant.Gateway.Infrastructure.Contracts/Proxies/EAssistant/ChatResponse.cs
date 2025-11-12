using IOC.EAssistant.Gateway.Library.Entities.Proxies.EAssistant.Chat;
using System.Text.Json.Nodes;

namespace IOC.EAssistant.Gateway.Infrastructure.Contracts.Proxies.EAssistant;
public class ChatResponse : ChatResponseDto
{
    public JsonObject? Metadata { get; set; }
}