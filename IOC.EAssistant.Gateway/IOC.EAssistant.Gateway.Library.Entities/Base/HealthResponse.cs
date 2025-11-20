namespace IOC.EAssistant.Gateway.Library.Entities.Base;
public class HealthResponse
{
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public bool ModelAvailable { get; set; } = false;
    public bool IsHealthy => ModelAvailable;
}
