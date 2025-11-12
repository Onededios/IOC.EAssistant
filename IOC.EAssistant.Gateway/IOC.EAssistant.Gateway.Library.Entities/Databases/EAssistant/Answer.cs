using System.Text.Json.Nodes;
namespace IOC.EAssistant.Gateway.Library.Entities.Databases.EAssistant;
public class Answer : Entity
{
    public required Guid IdQuestion { get; set; }
    public string Content { get; set; } = string.Empty;
    public int TokenCount { get; set; }
    public JsonObject Metadata { get; set; } = new JsonObject();
    public JsonObject Sources { get; set; } = new JsonObject();
}