namespace IOC.E_Assistant.WebApi.Library.Models.Entities;
public class Conversation : Entity
{
    public Session session { get; set; }
    public List<Question> questions { get; set; } = new List<Question>();
    public string title { get; set; }
}
