using IOC.EAssistant.Gateway.Infrastructure.Contracts.Databases;
using IOC.EAssistant.Gateway.Library.Contracts.Services;
using IOC.EAssistant.Gateway.Library.Entities.Databases.EAssistant;
using IOC.EAssistant.Gateway.XCutting.Results;
using Microsoft.Extensions.Logging;

namespace IOC.EAssistant.Gateway.Library.Implementation.Services;
public class ServiceSession(
        ILogger<ServiceSession> _logger,
        IDatabaseEAssistantBase<Session> _repository,
        IServiceConversation _serviceConversation
    ) : ServiceBase<Session>(_logger, _repository), IServiceSession
{
    public override async Task<OperationResult<bool>> SaveAsync(Session entity)
    {
        var operationResult = new OperationResult<bool>();

        _logger.LogInformation("Saving Session with ID: {SessionId}", entity.id);

        try
        {
            var sessionSaveCount = await _repository.SaveAsync(entity);
            var sessionSaved = sessionSaveCount > 0;

            if (!sessionSaved)
            {
                _logger.LogWarning("Failed to save Session with ID: {SessionId}", entity.id);
                operationResult.AddResult(false);
                return operationResult;
            }

            _logger.LogInformation("Successfully saved Session with ID: {SessionId}", entity.id);

            if (entity.conversations != null && entity.conversations.Count > 0)
            {
                _logger.LogInformation("Saving {Count} Conversations for Session ID: {SessionId}",
                    entity.conversations.Count, entity.id);

                var conversationsResult = await _serviceConversation.SaveMultipleAsync(entity.conversations);

                if (conversationsResult.HasErrors)
                {
                    _logger.LogError("Failed to save Conversations for Session ID: {SessionId}", entity.id);
                    operationResult.AddResultWithError(false, ActionSavingResult<Session, Conversation>(), -1, null);
                    return operationResult;
                }

                _logger.LogInformation("Successfully saved {Count} Conversations for Session ID: {SessionId}",
                    entity.conversations.Count, entity.id);
            }

            operationResult.AddResult(true);
        }
        catch (Exception ex)
        {
            operationResult.AddResultWithError(false, ActionErrorResult("saving"), -1, ex);
            _logger.LogError(ex, "Error saving Session with ID: {SessionId}", entity.id);
        }

        return operationResult;
    }

    public override async Task<OperationResult<bool>> SaveMultipleAsync(IEnumerable<Session> entities)
    {
        var operationResult = new OperationResult<bool>();
        var entityList = entities.ToList();

        _logger.LogInformation("Saving {Count} Sessions", entityList.Count);

        try
        {
            var sessionSaveCount = await _repository.SaveMultipleAsync(entityList);

            if (sessionSaveCount == 0)
            {
                _logger.LogWarning("Failed to save any Sessions");
                operationResult.AddResult(false);
                return operationResult;
            }

            _logger.LogInformation("Successfully saved {Count} Sessions", sessionSaveCount);

            var allConversations = entityList
                .Where(s => s.conversations != null && s.conversations.Count > 0)
                .SelectMany(s => s.conversations)
                .ToList();

            if (allConversations.Count > 0)
            {
                _logger.LogInformation("Saving {Count} Conversations across all Sessions", allConversations.Count);
                var conversationsResult = await _serviceConversation.SaveMultipleAsync(allConversations);

                if (conversationsResult.HasErrors)
                {
                    _logger.LogError("Failed to save some or all Conversations");
                    operationResult.AddResultWithError(false, ActionSavingResult<Session, Conversation>(), -1, null);
                    return operationResult;
                }

                _logger.LogInformation("Successfully saved {Count} Conversations", allConversations.Count);
            }

            operationResult.AddResult(true);
        }
        catch (Exception ex)
        {
            operationResult.AddResultWithError(false, ActionErrorResult("saving"), -1, ex);
            _logger.LogError(ex, "Error saving multiple Sessions");
        }

        return operationResult;
    }
}
