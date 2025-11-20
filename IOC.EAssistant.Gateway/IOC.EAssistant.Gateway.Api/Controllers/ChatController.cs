using IOC.EAssistant.Gateway.Library.Contracts.Services;
using IOC.EAssistant.Gateway.Library.Entities.Proxies.EAssistant.Chat;
using IOC.EAssistant.Gateway.XCutting.Results;
using Microsoft.AspNetCore.Mvc;

namespace IOC.EAssistant.Gateway.Api.Controllers;

[ApiController]
[Route("[controller]")]
[Produces("application/json")]
/// <summary>
/// Controller for managing chat entities in the EAssistant Gateway API.
/// </summary>
/// <remarks>
/// This controller provides RESTful endpoints for chat management operations including
/// retrieval and deletion. Chats represent user interactions with the AI and
/// include metadata, sources, and token count information. Inherits standard CRUD operations
/// from <see cref="BaseController{TEntity}"/>.
/// </remarks>
/// <param name="_serviceChat">Service layer implementation for chat business logic.</param>
/// <param name="_logger">Logger instance for tracking chat-related operations.</param>
public class ChatController(
    ILogger<ChatController> _logger,
    IServiceChat _serviceChat
) : ControllerBase
{
    /// <summary>
    /// Processes a chat request and returns AI-generated responses.
    /// </summary>
    /// <param name="request">
    /// The <see cref="ChatRequestDto"/> containing the chat messages, optional session ID, 
    /// optional conversation ID, and the message history to process.
    /// </param>
    /// <returns>
    /// A task that represents the asynchronous operation. The task result contains an
    /// <see cref="OperationResult{T}"/> with a <see cref="ChatResponseDto"/> containing the AI-generated 
    /// responses, token usage information, session identifier, and conversation identifier.
    /// </returns>
    /// <response code="200">Returns the chat response with AI-generated messages and usage metrics.</response>
    /// <remarks>
    /// This endpoint performs a POST request to send messages to the AI assistant and receive responses.
    /// The operation:
    /// <list type="bullet">
    /// <item><description>Creates a new session if <paramref name="request"/>.SessionId is not provided</description></item>
    /// <item><description>Creates a new conversation if <paramref name="request"/>.ConversationId is not provided</description></item>
    /// <item><description>Processes the message history and generates AI responses</description></item>
    /// <item><description>Persists the conversation data to the database</description></item>
    /// <item><description>Returns token usage metrics for monitoring and billing</description></item>
    /// </list>
    /// The operation result may contain errors if validation fails or exceptions if an error occurs during processing.
    /// </remarks>
    [HttpPost]
    [ProducesResponseType(200)]
    public async Task<OperationResult<ChatResponseDto>> Chat([FromBody] ChatRequestDto request)
    {
        var res = await _serviceChat.ChatAsync(request);
        return res;
    }
}
