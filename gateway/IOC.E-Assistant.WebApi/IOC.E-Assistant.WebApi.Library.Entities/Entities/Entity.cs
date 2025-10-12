namespace IOC.E_Assistant.WebApi.Library.Models.Entities;
public abstract class Entity
{
    public Guid id { get; set; }
    public DateTime created_at { get; set; }
}
