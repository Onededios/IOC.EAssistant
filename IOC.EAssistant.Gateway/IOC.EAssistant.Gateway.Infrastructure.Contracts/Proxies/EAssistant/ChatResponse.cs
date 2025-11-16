using IOC.EAssistant.Gateway.Library.Entities.Proxies.EAssistant;
using System.Text.Json.Nodes;

namespace IOC.EAssistant.Gateway.Infrastructure.Contracts.Proxies.EAssistant;
/// <summary>
/// Represents the response returned by a chat operation, including the choices generated, usage statistics, and
/// optional metadata.
/// </summary>
/// <remarks>This class encapsulates the results of a chat operation, providing access to the generated choices,
/// resource usage details, and any additional metadata associated with the response.</remarks>
public class ChatResponse
{
    public required IEnumerable<Choice> Choices { get; set; }
    public required Usage Usage { get; set; }
    public JsonObject? Metadata { get; set; }
}