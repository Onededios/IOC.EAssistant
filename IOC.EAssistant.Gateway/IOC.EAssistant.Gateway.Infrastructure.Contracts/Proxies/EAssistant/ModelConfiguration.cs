namespace IOC.EAssistant.Gateway.Library.Entities.Proxies.EAssistant;
/// <summary>
/// Represents the configuration settings for a language model, including parameters that control token limits,
/// randomness, and penalties for token usage.
/// </summary>
/// <remarks>This class is typically used to configure the behavior of a language model during text generation.
/// The properties allow fine-tuning of the model's output, such as controlling the maximum number of tokens, the
/// randomness of responses, and penalties for token repetition.</remarks>
public class ModelConfiguration
{
    public int MaxTokens { get; set; } = 128;
    public float Temperature { get; set; } = 0.7F;
    public float TopP { get; set; } = 1.0F;
    public float PresencePenalty { get; set; } = 0.0F;
    public float FrequencyPenalty { get; set; } = 0.0F;
}
