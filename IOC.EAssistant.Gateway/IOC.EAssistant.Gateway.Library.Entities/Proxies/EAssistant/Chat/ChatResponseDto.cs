namespace IOC.EAssistant.Gateway.Library.Entities.Proxies.EAssistant.Chat;
public class ChatResponseDto
{
    public required IEnumerable<Choice> Choices { get; set; }
    public required Usage Usage { get; set; }
    public required Guid SessionId { get; set; }
    public required Guid ConversationId { get; set; }
}
