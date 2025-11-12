namespace IOC.EAssistant.Gateway.Infrastructure.Contracts.Proxies.Entities;
public class ModelConfiguration
{
    public string? ModelName { get; set; }
    public int? MaxTokens { get; set; } = 128;
    public float? Temperature { get; set; } = 0.7F;
    public float? TopP { get; set; } = 1.0F;
    public float? PresencePenalty { get; set; } = 0.0F;
    public float? FrequencyPenalty { get; set; } = 0.0F;
}
