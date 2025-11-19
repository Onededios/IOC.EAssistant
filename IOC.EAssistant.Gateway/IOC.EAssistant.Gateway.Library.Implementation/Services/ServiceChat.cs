using IOC.EAssistant.Gateway.Infrastructure.Contracts.Proxies;
using IOC.EAssistant.Gateway.Infrastructure.Contracts.Proxies.EAssistant;
using IOC.EAssistant.Gateway.Library.Contracts.Services;
using IOC.EAssistant.Gateway.Library.Entities.Databases.EAssistant;
using IOC.EAssistant.Gateway.Library.Entities.Proxies.EAssistant;
using IOC.EAssistant.Gateway.Library.Entities.Proxies.EAssistant.Chat;
using IOC.EAssistant.Gateway.Library.Implementation.Mappers;
using IOC.EAssistant.Gateway.Library.Implementation.Validators;
using IOC.EAssistant.Gateway.XCutting.Results;
using Microsoft.Extensions.Logging;

namespace IOC.EAssistant.Gateway.Library.Implementation.Services;

/// <summary>
/// Provides service operations for managing chat interactions with the AI assistant.
/// </summary>
/// <remarks>
/// <para>
/// This service orchestrates the complete chat workflow, including:
/// <list type="bullet">
/// <item><description>Request validation and health checking</description></item>
/// <item><description>Conversation context preparation and history management</description></item>
/// <item><description>Communication with the AI model through the proxy layer</description></item>
/// <item><description>Persistence of questions and answers</description></item>
/// <item><description>Session and conversation lifecycle management</description></item>
/// </list>
/// </para>
/// <para>
/// The service implements sophisticated context management, automatically retrieving conversation
/// history when continuing an existing conversation and maintaining message ordering through indexing.
/// </para>
/// </remarks>
/// <param name="_logger">The logger instance for tracking operations and errors.</param>
/// <param name="_proxyEAssistant">The proxy for communicating with the AI model service.</param>
/// <param name="_serviceSession">The session service for managing user sessions.</param>
/// <param name="_serviceConversation">The conversation service for managing conversation entities.</param>
/// <param name="_serviceHealthCheck">The health check service for verifying model availability.</param>
/// <param name="_validatorChat">The validator for chat requests and model responses.</param>
public class ServiceChat(
  ILogger<ServiceChat> _logger,
    IProxyEAssistant _proxyEAssistant,
    IServiceSession _serviceSession,
    IServiceConversation _serviceConversation,
    IServiceHealthCheck _serviceHealthCheck,
    ValidatorChat _validatorChat
) : IServiceChat
{
    /// <summary>
    /// Processes a chat request and returns the AI-generated response.
    /// </summary>
    /// <param name="request">
    /// The <see cref="ChatRequestDto"/> containing the chat messages, optional session ID,
    /// and optional conversation ID for context.
    /// </param>
    /// <returns>
    /// An <see cref="OperationResult{T}"/> containing a <see cref="ChatResponseDto"/> with:
    /// <list type="bullet">
    /// <item><description>AI-generated response choices</description></item>
    /// <item><description>Token usage statistics</description></item>
    /// <item><description>Session and conversation identifiers</description></item>
    /// </list>
    /// </returns>
    /// <exception cref="InvalidOperationException">Thrown when model response is invalid or conversation persistence fails.</exception>
    /// <exception cref="HttpRequestException">Thrown when communication with the AI model fails.</exception>
    /// <exception cref="TimeoutException">Thrown when database or AI model operations time out.</exception>
    /// <remarks>
    /// <para>
    /// This method executes the following workflow:
    /// <list type="number">
    /// <item><description>Validates the incoming request structure and content</description></item>
    /// <item><description>Checks the AI model health status before proceeding</description></item>
    /// <item><description>Prepares conversation context (retrieves history if continuing a conversation)</description></item>
    /// <item><description>Sends the messages to the AI model and receives the response</description></item>
    /// <item><description>Creates Question and Answer entities from the interaction</description></item>
    /// <item><description>Persists the conversation data (creating a new session if needed)</description></item>
    /// <item><description>Returns the formatted response with identifiers</description></item>
    /// </list>
    /// </para>
    /// <para>
    /// The method handles both new and existing conversations:
    /// - For new conversations: Creates a new conversation entity and optionally a new session
    /// - For existing conversations: Retrieves and appends to the conversation history
    /// </para>
    /// <para>
    /// Validation errors and business logic errors (model unavailability, persistence failures)
    /// are captured in the operation result's error collection. Infrastructure exceptions
    /// (network failures, database timeouts) are allowed to propagate to be handled by
    /// exception middleware for proper error responses and logging.
    /// </para>
    /// </remarks>
    public async Task<OperationResult<ChatResponseDto>> ChatAsync(ChatRequestDto request)
    {
        var operationResult = new OperationResult<ChatResponseDto>();

        var validationErrors = _validatorChat.ValidateRequest(request);
        if (validationErrors.Any())
        {
            operationResult.AddErrors(validationErrors);
            return operationResult;
        }

        var healthCheckResult = await _serviceHealthCheck.GetModelHealthAsync();
        if (!healthCheckResult.Result)
        {
            operationResult.AddError(new ErrorResult("EAssistant API is not available", "Model"));
            _logger.LogError("EAssistant model API is not healthy");
            return operationResult;
        }

        var (conversation, messages, isExistingConversation) = await PrepareConversationContextAsync(request);

        var modelResponse = await GetModelResponseAsync(request);

        if (modelResponse.HasErrors)
        {
            operationResult.AddErrors(modelResponse.Errors);
            return operationResult;
        }

        var modelResult = modelResponse.Result!;

        var question = ChatMapper.CreateQuestionEntity(messages.Last(), modelResult, conversation.Id);

        conversation.Questions.Add(question);

        var persistResult = await PersistConversation(conversation, isExistingConversation);

        if (persistResult.HasErrors)
        {
            operationResult.AddErrors(persistResult.Errors);
            return operationResult;
        }

        operationResult.AddResult(ChatMapper.MapToResponseDto(modelResult, conversation.IdSession, conversation.Id));

        return operationResult;
    }

    /// <summary>
    /// Retrieves the AI model response for the given chat request.
    /// </summary>
    /// <param name="request">The chat request containing messages to process.</param>
    /// <returns>
    /// An <see cref="OperationResult{T}"/> containing the <see cref="ChatResponse"/> 
    /// from the AI model, or errors if validation or communication failed.
    /// </returns>
    /// <remarks>
    /// This method maps the request to the proxy format, communicates with the AI model,
    /// and validates the response before returning. Any validation errors are included
    /// in the operation result.
    /// </remarks>
    private async Task<OperationResult<ChatResponse>> GetModelResponseAsync(ChatRequestDto request)
    {
        var operationResult = new OperationResult<ChatResponse>();

        var mappedRequest = ChatMapper.MapToProxyRequest(request.Messages);

        var res = await _proxyEAssistant.ChatAsync(mappedRequest);

        var validationErrors = _validatorChat.ValidateModelResponse(res);

        if (validationErrors.Any())
        {
            operationResult.AddErrors(validationErrors);
            return operationResult;
        }

        operationResult.AddResult(res);
        return operationResult;
    }

    /// <summary>
    /// Prepares the conversation context for processing, including retrieving historical messages.
    /// </summary>
    /// <param name="request">The chat request with session, conversation, and message information.</param>
    /// <returns>
    /// A tuple containing:
    /// <list type="bullet">
    /// <item><description>The <see cref="Conversation"/> entity (new or existing)</description></item>
    /// <item><description>The complete message collection including history if applicable</description></item>
    /// <item><description>A boolean indicating whether the conversation already exists in the database</description></item>
    /// </list>
    /// </returns>
    /// <remarks>
    /// <para>
    /// This method implements intelligent context management:
    /// <list type="bullet">
    /// <item><description>If a conversation ID is provided and found, it retrieves the conversation with its history</description></item>
    /// <item><description>Historical messages are converted from Question entities to ChatMessage format</description></item>
    /// <item><description>The new message is appended with the correct index based on history length</description></item>
    /// <item><description>If no conversation ID is provided or history retrieval fails, a new conversation is created</description></item>
    /// <item><description>For existing conversations, the original session relationship is preserved</description></item>
    /// </list>
    /// </para>
    /// <para>
    /// The session ID from the request is used to link new conversations, or a new GUID is generated
    /// for new sessions. Existing conversations maintain their original session association.
    /// </para>
    /// </remarks>
    private async Task<(
        Conversation conversation,
        IEnumerable<ChatMessage> messages,
        bool isExistingConversation
    )> PrepareConversationContextAsync(ChatRequestDto request)
    {
        var sessionId = request.SessionId ?? Guid.NewGuid();
        var conversation = ChatMapper.CreateConversationEntity(sessionId);
        var messages = request.Messages;
        var isExistingConversation = false;

        if (request.ConversationId.HasValue)
        {
            var conversationRes = await _serviceConversation.GetAsync(request.ConversationId.Value);

            if (conversationRes.Result?.Questions != null)
            {
                conversation = conversationRes.Result;
                isExistingConversation = true;
                var historicalMessages = ChatMapper.MapQuestionsToMessages(conversation.Questions);

                var newMessage = messages.First();
                ChatMapper.UpdateMessageIndex(newMessage, historicalMessages.Count);
                historicalMessages.Add(newMessage);

                messages = historicalMessages;
            }
        }
        else if (request.SessionId.HasValue)
        {
            conversation.Id = Guid.NewGuid();
            conversation.IdSession = request.SessionId.Value;
            conversation.CreatedAt = DateTime.Now;
        }

        return (conversation, messages, isExistingConversation);
    }

    /// <summary>
    /// Persists the conversation and its associated session to the database.
    /// </summary>
    /// <param name="conversation">
    /// The <see cref="Conversation"/> entity to persist, containing the questions and answers
    /// from the current chat interaction.
    /// </param>
    /// <param name="isExistingConversation">
    /// Indicates whether the conversation already exists in the database.
    /// When <see langword="true"/>, the conversation is updated with new questions.
    /// When <see langword="false"/>, this is a new conversation that needs to be created.
    /// </param>
    /// <returns>
    /// An <see cref="OperationResult{T}"/> containing:
    /// <list type="bullet">
    /// <item><description><see langword="true"/> if the persistence operation succeeded</description></item>
    /// <item><description><see langword="false"/> with error details if the operation failed</description></item>
    /// </list>
    /// </returns>
    /// <remarks>
    /// <para>
    /// This method implements a smart persistence strategy that handles three distinct scenarios:
    /// </para>
    /// <para>
    /// <strong>Scenario 1: Updating an Existing Conversation</strong>
    /// <list type="bullet">
    /// <item><description>When <paramref name="isExistingConversation"/> is <see langword="true"/></description></item>
    /// <item><description>Saves only the conversation entity with its new questions</description></item>
    /// <item><description>Preserves the existing session relationship</description></item>
    /// <item><description>Most efficient path as no session lookup or creation is needed</description></item>
    /// </list>
    /// </para>
    /// <para>
    /// <strong>Scenario 2: Creating a Conversation in a New Session</strong>
    /// <list type="bullet">
    /// <item><description>When the conversation is new AND its session doesn't exist</description></item>
    /// <item><description>Creates a new session entity containing the conversation</description></item>
    /// <item><description>Saves the session, which cascades to save the conversation</description></item>
    /// <item><description>Establishes the parent-child relationship in a single operation</description></item>
    /// </list>
    /// </para>
    /// <para>
    /// <strong>Scenario 3: Adding a Conversation to an Existing Session</strong>
    /// <list type="bullet">
    /// <item><description>When the conversation is new BUT its session already exists</description></item>
    /// <item><description>Saves only the conversation entity</description></item>
    /// <item><description>Links to the existing session via the IdSession foreign key</description></item>
    /// <item><description>Avoids creating duplicate sessions for the same user</description></item>
    /// </list>
    /// </para>
    /// <para>
    /// <strong>Error Handling:</strong>
    /// Any errors from the session or conversation service operations are propagated
    /// through the returned <see cref="OperationResult{T}"/>, allowing the caller to
    /// handle persistence failures appropriately.
    /// </para>
    /// <para>
    /// <strong>Performance Consideration:</strong>
    /// The method performs a session existence check only for new conversations,
    /// optimizing the common case of continuing an existing conversation.
    /// </para>
    /// </remarks>
    private async Task<OperationResult<bool>> PersistConversation(
        Conversation conversation,
        bool isExistingConversation
    )
    {
        if (isExistingConversation)
        {
            return await _serviceConversation.SaveAsync(conversation);
        }

        var sessionResult = await _serviceSession.GetAsync(conversation.IdSession);
        var sessionExists = sessionResult.Result != null && !sessionResult.HasErrors;

        if (sessionExists)
        {
            return await _serviceConversation.SaveAsync(conversation);
        }
        else
        {
            var newSession = ChatMapper.CreateSessionEntity(conversation);
            return await _serviceSession.SaveAsync(newSession);
        }
    }
}
