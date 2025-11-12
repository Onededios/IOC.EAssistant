using System.Text.Json.Nodes;

namespace IOC.EAssistant.Gateway.Library.Entities.Databases.EAssistant;

public class Question : Entity
{
    public required Guid IdConversation { get; set; }
    public required string Content { get; set; }
    public int TokenCount { get; set; }
    public required JsonObject Metadata { get; set; }
    public required Answer Answer { get; set; }
}