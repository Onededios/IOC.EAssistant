namespace IOC.E_Assistant.WebApi.Library.Models.Entities;
public class Session : Entity
{
    public DateTime expired_at { get; set; }
    public List<Conversation> conversations { get; set; }
}
