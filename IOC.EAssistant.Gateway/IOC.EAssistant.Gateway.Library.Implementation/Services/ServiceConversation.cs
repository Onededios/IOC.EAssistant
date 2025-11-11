using IOC.EAssistant.Gateway.Infrastructure.Contracts.Databases;
using IOC.EAssistant.Gateway.Library.Contracts.Services;
using IOC.EAssistant.Gateway.Library.Entities.Databases.EAssistant;
using IOC.EAssistant.Gateway.XCutting.Results;
using Microsoft.Extensions.Logging;

namespace IOC.EAssistant.Gateway.Library.Implementation.Services;
public class ServiceConversation(
        ILogger<ServiceConversation> _logger,
        IDatabaseEAssistantBase<Conversation> _repository,
        IServiceQuestion _serviceQuestion
    ) : ServiceBase<Conversation>(_logger, _repository), IServiceConversation
{
    public override async Task<OperationResult<bool>> SaveAsync(Conversation entity)
    {
        var operationResult = new OperationResult<bool>();

        _logger.LogInformation("Saving Conversation with ID: {ConversationId} for Session ID: {SessionId}",
            entity.id, entity.session_id);

        try
        {
            var conversationSaveCount = await _repository.SaveAsync(entity);
            var conversationSaved = conversationSaveCount > 0;

            if (!conversationSaved)
            {
                _logger.LogWarning("Failed to save Conversation with ID: {ConversationId}", entity.id);
                operationResult.AddResult(false);
                return operationResult;
            }

            _logger.LogInformation("Successfully saved Conversation with ID: {ConversationId}", entity.id);

            if (entity.questions != null && entity.questions.Count > 0)
            {
                _logger.LogInformation("Saving {Count} Questions for Conversation ID: {ConversationId}",
                    entity.questions.Count, entity.id);

                var questionsResult = await _serviceQuestion.SaveMultipleAsync(entity.questions);

                if (questionsResult.HasErrors)
                {
                    _logger.LogError("Failed to save Questions for Conversation ID: {ConversationId}", entity.id);
                    operationResult.AddResultWithError(false, ActionSavingResult<Question, Answer>(), -1, null);
                    return operationResult;
                }

                _logger.LogInformation("Successfully saved {Count} Questions for Conversation ID: {ConversationId}",
                    entity.questions.Count, entity.id);
            }

            operationResult.AddResult(true);
        }
        catch (Exception ex)
        {
            operationResult.AddResultWithError(false, ActionErrorResult("saving"), -1, ex);
            _logger.LogError(ex, "Error saving Conversation with ID: {ConversationId}", entity.id);
        }

        return operationResult;
    }

    public override async Task<OperationResult<bool>> SaveMultipleAsync(IEnumerable<Conversation> entities)
    {
        var operationResult = new OperationResult<bool>();
        var entityList = entities.ToList();

        _logger.LogInformation("Saving {Count} Conversations", entityList.Count);

        try
        {
            var conversationSaveCount = await _repository.SaveMultipleAsync(entityList);

            if (conversationSaveCount == 0)
            {
                _logger.LogWarning("Failed to save any Conversations");
                operationResult.AddResult(false);
                return operationResult;
            }

            _logger.LogInformation("Successfully saved {Count} Conversations", conversationSaveCount);

            var allQuestions = entityList
                .Where(c => c.questions != null && c.questions.Count > 0)
                .SelectMany(c => c.questions)
                .ToList();

            if (allQuestions.Count > 0)
            {
                _logger.LogInformation("Saving {Count} Questions across all Conversations", allQuestions.Count);
                var questionsResult = await _serviceQuestion.SaveMultipleAsync(allQuestions);

                if (questionsResult.HasErrors)
                {
                    _logger.LogError("Failed to save some or all Questions");
                    operationResult.AddResultWithError(false, ActionSavingResult<Conversation, Question>(), -1, null);
                    return operationResult;
                }

                _logger.LogInformation("Successfully saved {Count} Questions", allQuestions.Count);
            }

            operationResult.AddResult(true);
        }
        catch (Exception ex)
        {
            operationResult.AddResultWithError(false, ActionErrorResult("saving"), -1, ex);
            _logger.LogError(ex, "Error saving multiple Conversations");
        }

        return operationResult;
    }
}
