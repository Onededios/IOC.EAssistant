namespace IOC.EAssistant.Gateway.Library.Entities.Databases.EAssistant;
public class Session : Entity
{
    public List<Conversation> Conversations { get; set; } = new List<Conversation>();
}