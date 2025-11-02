namespace IOC.EAssistant.Gateway.Library.Entities.Databases.EAssistant;
public class Conversation : Entity
{
    public required Session session { get; set; }
    public List<Question> questions { get; set; } = new List<Question>();
    public required string title { get; set; }
}