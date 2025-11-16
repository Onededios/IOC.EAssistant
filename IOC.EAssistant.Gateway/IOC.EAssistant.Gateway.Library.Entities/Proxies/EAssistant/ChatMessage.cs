using System.Text.Json.Nodes;

namespace IOC.EAssistant.Gateway.Library.Entities.Proxies.EAssistant;
public class ChatMessage
{
    public required int Index { get; set; }
    public required string Question { get; set; }
    public string? Answer { get; set; } = string.Empty;
    public JsonObject? Metadata { get; set; }
}