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
            entity.Id, entity.IdSession);

        try
        {
            var conversationSaveCount = await _repository.SaveAsync(entity);
            var conversationSaved = conversationSaveCount > 0;

            if (!conversationSaved)
            {
                _logger.LogWarning("Failed to save Conversation with ID: {ConversationId}", entity.Id);
                operationResult.AddResult(false);
                return operationResult;
            }

            _logger.LogInformation("Successfully saved Conversation with ID: {ConversationId}", entity.Id);

            if (entity.Questions != null && entity.Questions.Count > 0)
            {
                _logger.LogInformation("Saving {Count} Questions for Conversation ID: {ConversationId}",
                    entity.Questions.Count, entity.Id);

                var questionsResult = await _serviceQuestion.SaveMultipleAsync(entity.Questions);

                if (questionsResult.HasErrors)
                {
                    _logger.LogError("Failed to save Questions for Conversation ID: {ConversationId}", entity.Id);
                    operationResult.AddResultWithError(false, ActionSavingResult<Question, Answer>(), -1, null);
                    return operationResult;
                }

                _logger.LogInformation("Successfully saved {Count} Questions for Conversation ID: {ConversationId}",
                    entity.Questions.Count, entity.Id);
            }

            operationResult.AddResult(true);
        }
        catch (Exception ex)
        {
            operationResult.AddResultWithError(false, ActionErrorResult("saving"), -1, ex);
            _logger.LogError(ex, "Error saving Conversation with ID: {ConversationId}", entity.Id);
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
                .Where(c => c.Questions != null && c.Questions.Count > 0)
                .SelectMany(c => c.Questions)
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
