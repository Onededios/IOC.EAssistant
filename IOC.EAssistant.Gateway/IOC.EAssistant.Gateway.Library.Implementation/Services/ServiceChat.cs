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
public class ServiceChat(
    ILogger<ServiceChat> _logger,
    IProxyEAssistant _proxyEAssistant,
    IServiceSession _serviceSession,
    IServiceConversation _serviceConversation,
    IServiceHealthCheck _serviceHealthCheck,
    ValidatorChat _validatorChat
) : IServiceChat
{
    public async Task<OperationResult<ChatResponseDto>> ChatAsync(ChatRequestDto request)
    {
        var operationResult = new OperationResult<ChatResponseDto>();
        try
        {

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

            var (conversation, messages) = await PrepareConversationContextAsync(request);

            var modelResponse = await GetModelResponseAsync(request);

            if (modelResponse.HasErrors)
            {
                operationResult.AddErrors(modelResponse.Errors);
                return operationResult;
            }

            var modelResult = modelResponse.Result!;

            var question = ChatMapper.CreateQuestionEntity(messages.Last(), modelResult, conversation.Id);

            conversation.Questions.Add(question);

            var persistResult = await PersistConversation(conversation, request.SessionId);

            if (persistResult.HasErrors)
            {
                operationResult.AddErrors(persistResult.Errors);
                return operationResult;
            }

            operationResult.AddResult(ChatMapper.MapToResponseDto(modelResult, conversation.IdSession, conversation.Id));
        }
        catch (Exception ex)
        {
            operationResult.AddError(new ErrorResult("Error occurred while processing chat request", "Chat Processing"), ex);
            _logger.LogError(ex, "Error occurred while processing chat request");
        }

        return operationResult;
    }

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

    private async Task<(Conversation conversation, IEnumerable<ChatMessage> messages)> PrepareConversationContextAsync(ChatRequestDto request)
    {
        var conversation = ChatMapper.CreateConversationEntity(request.SessionId ?? Guid.NewGuid());
        var messages = request.Messages;

        if (request.ConversationId.HasValue && request.Messages.Count() == 1)
        {
            var conversationRes = await _serviceConversation.GetAsync(request.ConversationId.Value);

            if (conversationRes.Result?.Questions != null)
            {
                conversation = conversationRes.Result;
                var historicalMessages = ChatMapper.MapQuestionsToMessages(conversation.Questions);

                var newMessage = messages.First();
                ChatMapper.UpdateMessageIndex(newMessage, historicalMessages.Count);
                historicalMessages.Add(newMessage);

                messages = historicalMessages;
            }
        }

        return (conversation, messages);
    }

    private async Task<OperationResult<bool>> PersistConversation(
        Conversation conversation,
        Guid? sessionId
    )
    {
        if (sessionId.HasValue)
        {
            return await _serviceConversation.SaveAsync(conversation);
        }

        var session = ChatMapper.CreateSessionEntity(conversation);
        return await _serviceSession.SaveAsync(session);
    }
}
