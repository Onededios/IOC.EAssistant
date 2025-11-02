namespace IOC.EAssistant.Gateway.Library.Entities.Proxies.EAssistant;
public class Choice
{
    public int Index { get; set; }
    public required Message Message { get; set; }
    public string FinishReason { get; set; } = string.Empty;
}