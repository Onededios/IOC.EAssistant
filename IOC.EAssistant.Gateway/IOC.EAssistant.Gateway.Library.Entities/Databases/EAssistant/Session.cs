namespace IOC.EAssistant.Gateway.Library.Entities.Databases.EAssistant;
public class Session : Entity
{
    public List<Conversation> conversations { get; set; } = new List<Conversation>();
}