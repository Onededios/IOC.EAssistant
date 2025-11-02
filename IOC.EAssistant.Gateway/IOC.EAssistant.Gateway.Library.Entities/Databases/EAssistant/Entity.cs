namespace IOC.EAssistant.Gateway.Library.Entities.Databases.EAssistant;
public abstract class Entity
{
    public Guid id { get; set; }
    public DateTime created_at { get; set; }
}