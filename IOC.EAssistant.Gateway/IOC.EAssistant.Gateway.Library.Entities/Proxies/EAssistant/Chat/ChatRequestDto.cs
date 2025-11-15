using System.Text.Json.Nodes;

namespace IOC.EAssistant.Gateway.Library.Entities.Proxies.EAssistant.Chat;
public class ChatRequestDto
{
    public Guid? SessionId { get; set; }
    public Guid? ConversationId { get; set; }
    public required IEnumerable<ChatMessage> Messages { get; set; }
}