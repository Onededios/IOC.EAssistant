using IOC.EAssistant.Gateway.Library.Entities.Proxies.EAssistant;
using System.Text.Json.Nodes;

namespace IOC.EAssistant.Gateway.Infrastructure.Contracts.Proxies.EAssistant;
public class ChatResponse
{
    public required IEnumerable<Choice> Choices { get; set; }
    public required Usage Usage { get; set; }
    public JsonObject? Metadata { get; set; }
}