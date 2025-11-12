using System.Text.Json.Serialization;

namespace IOC.EAssistant.Gateway.Library.Entities.Databases.EAssistant;
public abstract class Entity
{
    [JsonPropertyOrder(-1)]
    public Guid Id { get; set; }
    public DateTime CreatedAt { get; set; }
}