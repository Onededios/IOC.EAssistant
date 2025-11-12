using IOC.EAssistant.Gateway.Library.Entities.Proxies.EAssistant;
using System.Text.Json.Nodes;

namespace IOC.EAssistant.Gateway.Infrastructure.Contracts.Proxies.Entities;
public class ChatResponse
{
    public JsonObject? Metadata { get; set; }
}

