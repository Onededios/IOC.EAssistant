namespace IOC.E_Assistant.Infraestructure.Implementation.Proxies.EAssistant.Chat;
public class ChatResponse
{
    public IEnumerable<Choice> Choices { get; set; }
}

private class Choice
{
    public int Index { get; set; }
    public Message Message { get; set; }
}

private class Message
{
    public string Role { get; set; }
    public string Content { get; set; }
}