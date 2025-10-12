namespace IOC.E_Assistant.Infraestructure.Implementation.Proxies.EAssistant.Chat;
public class ChatRequest
{
    public string[] Messages { get; set; } // Message array to maintain context
    public string? Prompt { get; set; } // Entry prompt for the model
    public int MaxTokens { get; set; } = 128; // Token limit for response
    public float Temperature { get; set; }  = 0.7; // Response randomness
    public float TopP { get; set; } = 1.0; // Controls sampling
    public float PresencePenalty { get; set; } = 0.0; // Penalizes new response theme appearance
    public float FrequencyPenalty { get; set; } = 0.0; // Penalizes word repetition
}
