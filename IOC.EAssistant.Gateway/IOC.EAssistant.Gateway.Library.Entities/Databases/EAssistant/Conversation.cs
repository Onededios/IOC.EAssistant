namespace IOC.EAssistant.Gateway.Library.Entities.Databases.EAssistant;
public class Conversation : Entity
{
    public required Guid IdSession { get; set; }
    public List<Question> Questions { get; set; } = new List<Question>();
    public string? Title { get; set; }
}