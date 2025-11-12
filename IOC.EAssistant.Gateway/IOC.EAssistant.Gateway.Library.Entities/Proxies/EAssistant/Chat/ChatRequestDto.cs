namespace IOC.EAssistant.Gateway.Library.Entities.Proxies.EAssistant.Chat;
public class ChatRequestDto
{
    public required IEnumerable<ChatMessage> Messages { get; set; }
}