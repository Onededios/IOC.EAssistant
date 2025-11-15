using IOC.EAssistant.Gateway.Infrastructure.Contracts.Proxies;
using IOC.EAssistant.Gateway.Infrastructure.Contracts.Proxies.EAssistant;
using IOC.EAssistant.Gateway.Library.Contracts.Services;
using IOC.EAssistant.Gateway.Library.Entities.Databases.EAssistant;
using IOC.EAssistant.Gateway.Library.Entities.Proxies.EAssistant;
using IOC.EAssistant.Gateway.Library.Entities.Proxies.EAssistant.Chat;
using IOC.EAssistant.Gateway.XCutting.Results;
using Microsoft.Extensions.Logging;

namespace IOC.EAssistant.Gateway.Library.Implementation.Services;
public class ServiceChat(
    ILogger<ServiceChat> _logger,
    IProxyEAssistant _proxyEAssistant,
    IServiceSession _serviceSession,
    IServiceConversation _serviceConversation,
    IServiceHealthCheck _serviceHealthCheck
) : IServiceChat
{
    public async Task<OperationResult<ChatResponseDto>> ChatAsync(ChatRequestDto request)
    {
        var operationResult = new OperationResult<ChatResponseDto>();
        try
        {
            operationResult = HandleNoRequiredParameters(operationResult, request);
            operationResult = HandleNoMessages(operationResult, request);
            operationResult = HandleEmptyMessage(operationResult, request);

            var healthCheckResult = await _serviceHealthCheck.GetModelHealthAsync();
            if (!healthCheckResult.Result)
            {
                operationResult.AddError(new ErrorResult("EAssistant API is not available", "Model"));
                _logger.LogError("EAssistant model API is not healthy");
            }

            if (operationResult.HasErrors)
            {
                return operationResult;
            }

            var conversationET = new Conversation { IdSession = request.SessionId ?? Guid.NewGuid() };

            if (request.ConversationId != null && request.Messages.Count() == 1)
            {
                var conversationRes = await _serviceConversation.GetAsync(request.ConversationId.Value);

                if (conversationRes.Result != null && conversationRes.Result.Questions != null)
                {
                    conversationET = conversationRes.Result;
                    var mapped = conversationET.Questions.Select(q => new ChatMessage
                    {
                        Index = q.Index,
                        Question = q.Content,
                        Answer = q.Answer.Content,
                        Metadata = q.Metadata
                    }).ToList();

                    var updated = request.Messages.First();

                    updated.Index = mapped.Count + 1;

                    mapped.Add(updated);
                    request.Messages = mapped;
                }
            }

            var modelResponse = await GetModelResponse(request);

            if (modelResponse.HasErrors)
            {
                operationResult.AddErrors(modelResponse.Errors);
                return operationResult;
            }

            if (modelResponse.Result == null || !modelResponse.Result.Choices.Any())
            {
                operationResult.AddError(new ErrorResult("The model provided no answers for your question.", "Model"));
                return operationResult;
            }

            var modelResult = modelResponse.Result;

            var currentMessage = request.Messages.ElementAt(request.Messages.Count() - 1);

            var answerET = new Answer
            {
                IdQuestion = Guid.NewGuid(),
                Id = Guid.NewGuid(),
                Content = modelResult.Choices.First().Message.Content,
                TokenCount = modelResult.Usage.CompletionTokens,
                Metadata = modelResult.Metadata
            };

            var questionET = new Question
            {
                Id = answerET.IdQuestion,
                IdConversation = conversationET.Id,
                Content = currentMessage.Question,
                Metadata = currentMessage.Metadata,
                Answer = answerET,
                TokenCount = modelResult.Usage.PromptTokens
            };

            conversationET.Questions.Add(questionET);

            if (request.SessionId.HasValue)
            {
                var conversationSaveRes = await _serviceConversation.SaveAsync(conversationET);

                if (conversationSaveRes.HasErrors)
                {
                    operationResult.AddErrors(conversationSaveRes.Errors);
                    return operationResult;
                }
            }
            else
            {
                var sessionET = new Session
                {
                    Id = Guid.NewGuid(),
                    Conversations = new List<Conversation>() { conversationET }
                };

                var sessionSaveRes = await _serviceSession.SaveAsync(sessionET);

                if (sessionSaveRes.HasErrors)
                {
                    operationResult.AddErrors(sessionSaveRes.Errors);
                    return operationResult;
                }
            }


            operationResult.AddResult(new ChatResponseDto { Choices = modelResult.Choices, Usage = modelResult.Usage });

            return operationResult;
        }
        catch (Exception ex)
        {
            operationResult.AddError(new ErrorResult("Error occurred while processing chat request", "Chat Processing"), ex);
            _logger.LogError(ex, "Error occurred while processing chat request");
        }

        return operationResult;
    }

    private async Task<OperationResult<ChatResponse>> GetModelResponse(ChatRequestDto request)
    {
        var operationResult = new OperationResult<ChatResponse>();

        var mappedRequest = new ChatRequest { Messages = request.Messages };

        var res = await _proxyEAssistant.ChatAsync(mappedRequest);

        if (res != null)
        {
            operationResult.AddResult(res);
        }
        else
        {
            operationResult.AddError(new ErrorResult("Received null response from EAssistant model API", "Chat Response"));
            _logger.LogError("Received null response from EAssistant model API");
        }

        return operationResult;
    }

    private OperationResult<T> HandleNoRequiredParameters<T>(OperationResult<T> operationResult, ChatRequestDto request)
    {
        if (!request.ConversationId.HasValue && !request.SessionId.HasValue)
        {
            operationResult.AddError(new ErrorResult("Either ConversationId or SessionId must be provided", "Invalid Request"));
            _logger.LogWarning("Chat request missing both ConversationId and SessionId");
        }
        return operationResult;
    }

    private OperationResult<T> HandleNoMessages<T>(OperationResult<T> operationResult, ChatRequestDto request)
    {
        if (request.Messages == null || !request.Messages.Any())
        {
            operationResult.AddError(new ErrorResult("Messages cannot be null or empty", "Invalid Request"));
            _logger.LogWarning("Chat request contains null or empty messages");
        }
        return operationResult;
    }

    private OperationResult<T> HandleEmptyMessage<T>(OperationResult<T> operationResult, ChatRequestDto request)
    {
        if (request.Messages.Any(m => string.IsNullOrWhiteSpace(m.Question)))
        {
            operationResult.AddError(new ErrorResult("All messages must have non-empty content", "Invalid Request"));
            _logger.LogWarning("Chat request contains messages with empty content");
        }
        return operationResult;
    }
}
