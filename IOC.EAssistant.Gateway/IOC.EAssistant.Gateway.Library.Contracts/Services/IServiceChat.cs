using IOC.EAssistant.Gateway.Library.Entities.Proxies.EAssistant.Chat;
using IOC.EAssistant.Gateway.XCutting.Results;

namespace IOC.EAssistant.Gateway.Library.Contracts.Services;

/// <summary>
/// Defines service operations for managing chat interactions with the AI assistant.
/// </summary>
/// <remarks>
/// This service handles the communication between users and the AI model, processing
/// chat messages and returning responses. It manages conversation context, session handling,
/// and integrates with the underlying AI infrastructure.
/// </remarks>
public interface IServiceChat
{
    /// <summary>
    /// Processes a chat request and returns the AI-generated response.
    /// </summary>
    /// <param name="request">
    /// The <see cref="ChatRequestDto"/> containing the chat messages, session information,
    /// and conversation context.
    /// </param>
    /// <returns>
    /// An <see cref="OperationResult{T}"/> containing a <see cref="ChatResponseDto"/> 
    /// with the AI-generated responses, token usage information, and session identifiers.
    /// </returns>
    /// <remarks>
    /// This method:
    /// <list type="bullet">
    /// <item><description>Creates a new session if one doesn't exist</description></item>
    /// <item><description>Creates a new conversation within the session if needed</description></item>
    /// <item><description>Sends messages to the AI model and receives responses</description></item>
    /// <item><description>Persists the questions and answers to the database</description></item>
    /// <item><description>Tracks token usage for monitoring and billing purposes</description></item>
    /// </list>
    /// The operation result will contain errors if validation fails or if there are issues
    /// communicating with the AI model.
    /// </remarks>
    Task<OperationResult<ChatResponseDto>> ChatAsync(ChatRequestDto request);
}
