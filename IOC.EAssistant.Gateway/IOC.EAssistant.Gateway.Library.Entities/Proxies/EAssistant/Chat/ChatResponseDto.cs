namespace IOC.EAssistant.Gateway.Library.Entities.Proxies.EAssistant.Chat;
public class ChatResponseDto
{
    public required IEnumerable<Choice> Choices { get; set; }
    public required Usage Usage { get; set; }
    public required Guid IdSession { get; set; }
    public required Guid IdConversation { get; set; }
}
