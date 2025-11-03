using System.Text.Json.Nodes;

namespace IOC.EAssistant.Gateway.Library.Entities.Databases.EAssistant;

public class Question : Entity
{
    public required Guid conversation_id { get; set; }
    public required string question { get; set; }
    public int token_count { get; set; }
    public required JsonObject metadata { get; set; }
    public required Answer answer { get; set; }
}