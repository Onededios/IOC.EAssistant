namespace IOC.EAssistant.Gateway.Library.Entities.Proxies.EAssistant;
public class ChatRequest
{
    public string[]? Messages { get; set; } // Message array to maintain context
    public string? Prompt { get; set; } // Entry prompt for the model
    public int MaxTokens { get; set; } = 128; // Token limit for response
    public float Temperature { get; set; } = 0.7F; // Response randomness
    public float TopP { get; set; } = 1.0F; // Controls sampling
    public float PresencePenalty { get; set; } = 0.0F; // Penalizes new response theme appearance
    public float FrequencyPenalty { get; set; } = 0.0F; // Penalizes word repetition
}