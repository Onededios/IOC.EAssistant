namespace IOC.E_Assistant.WebApi.Library.Models.Entities;
public class ChatResponse
{
    public IEnumerable<Choice>? Choices { get; set; }
}

public class Choice
{
    public int Index { get; set; }
    public required Message Message { get; set; }
}

public class Message
{
    public required string Role { get; set; }
    public required string Content { get; set; }
}