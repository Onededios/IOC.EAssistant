namespace IOC.EAssistant.Gateway.Infrastructure.Contracts.Proxies.EAssistant;
public class HealthResponse
{
    public string? Model { get; set; }
    public required string Status { get; set; }
    public DateTime? Timestamp { get; set; }
}
