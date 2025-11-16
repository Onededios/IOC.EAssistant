namespace IOC.EAssistant.Gateway.Infrastructure.Contracts.Proxies.EAssistant;
/// <summary>
/// Represents the health status of a system or component, including its model, current status, and an optional
/// timestamp.
/// </summary>
/// <remarks>This class is typically used to convey the health state of a system or component in monitoring or
/// diagnostic scenarios.</remarks>
public class HealthResponse
{
    public string? Model { get; set; }
    public required string Status { get; set; }
    public DateTime? Timestamp { get; set; }
}
