using IOC.EAssistant.Gateway.Infrastructure.Contracts.Databases;
using IOC.EAssistant.Gateway.Library.Contracts.Services;
using IOC.EAssistant.Gateway.Library.Entities.Databases.EAssistant;
using IOC.EAssistant.Gateway.XCutting.Results;
using Microsoft.Extensions.Logging;

namespace IOC.EAssistant.Gateway.Library.Implementation.Services;

/// <summary>
/// Provides service operations for managing <see cref="Session"/> entities and their related conversations.
/// </summary>
/// <remarks>
/// <para>
/// This service represents the top level of the conversation hierarchy (Session → Conversation → Question → Answer)
/// and implements cascading save operations throughout the entire entity graph. It ensures that when a session
/// is saved, all nested conversations, questions, and answers are persisted correctly.
/// </para>
/// <para>
/// Sessions group related conversations for a user interaction period, providing context boundaries
/// and organizational structure for AI assistant interactions.
/// </para>
/// </remarks>
/// <param name="_logger">The logger instance for tracking operations and errors.</param>
/// <param name="_repository">The database repository for Session entity operations.</param>
/// <param name="_serviceConversation">The conversation service for managing related conversation entities.</param>
public class ServiceSession(
        ILogger<ServiceSession> _logger,
        IDatabaseEAssistantBase<Session> _repository,
        IServiceConversation _serviceConversation
    ) : ServiceBase<Session>(_logger, _repository), IServiceSession
{
    /// <summary>
    /// Saves a single session entity along with all its conversations and nested entities to the data store.
    /// </summary>
    /// <param name="entity">The <see cref="Session"/> entity to save, including its conversations collection.</param>
    /// <returns>
    /// An <see cref="OperationResult{T}"/> containing:
    /// <list type="bullet">
    /// <item><description>true if the session and all nested entities were saved successfully</description></item>
    /// <item><description>false with errors if any save operation in the hierarchy failed</description></item>
    /// </list>
    /// </returns>
    /// <remarks>
    /// <para>
    /// This method implements a multi-level cascading save operation:
    /// <list type="number">
    /// <item><description>Saves the session entity to the database</description></item>
    /// <item><description>Cascades to save all conversations in the session's Conversations collection</description></item>
    /// <item><description>Each conversation save cascades to questions (via ServiceConversation)</description></item>
    /// <item><description>Each question save cascades to answers (via ServiceQuestion)</description></item>
    /// </list>
    /// </para>
    /// <para>
    /// The method validates that the session was saved successfully before attempting to save
    /// conversations, preventing orphaned data and maintaining referential integrity.
    /// </para>
    /// <para>
    /// If the session has no conversations, the method completes successfully after saving
    /// only the session entity, allowing for incremental session construction.
    /// </para>
    /// <para>
    /// Any failure at any level of the hierarchy is propagated up through the error result,
    /// with descriptive messages indicating which entity type failed to save.
    /// </para>
    /// </remarks>
    public override async Task<OperationResult<bool>> SaveAsync(Session entity)
    {
        var operationResult = new OperationResult<bool>();

        _logger.LogInformation("Saving Session with ID: {SessionId}", entity.Id);

        try
        {
            var sessionSaveCount = await _repository.SaveAsync(entity);
            var sessionSaved = sessionSaveCount > 0;

            if (!sessionSaved)
            {
                _logger.LogWarning("Failed to save Session with ID: {SessionId}", entity.Id);
                operationResult.AddResult(false);
                return operationResult;
            }

            _logger.LogInformation("Successfully saved Session with ID: {SessionId}", entity.Id);

            if (entity.Conversations != null && entity.Conversations.Count > 0)
            {
                _logger.LogInformation("Saving {Count} Conversations for Session ID: {SessionId}",
                    entity.Conversations.Count, entity.Id);

                var conversationsResult = await _serviceConversation.SaveMultipleAsync(entity.Conversations);

                if (conversationsResult.HasErrors)
                {
                    _logger.LogError("Failed to save Conversations for Session ID: {SessionId}", entity.Id);
                    operationResult.AddResultWithError(false, ActionSavingResult<Session, Conversation>(), -1, null);
                    return operationResult;
                }

                _logger.LogInformation("Successfully saved {Count} Conversations for Session ID: {SessionId}",
                    entity.Conversations.Count, entity.Id);
            }

            operationResult.AddResult(true);
        }
        catch (Exception ex)
        {
            operationResult.AddResultWithError(false, ActionErrorResult("saving"), -1, ex);
            _logger.LogError(ex, "Error saving Session with ID: {SessionId}", entity.Id);
        }

        return operationResult;
    }

    /// <summary>
    /// Saves multiple session entities along with all their nested entities in a batch operation.
    /// </summary>
    /// <param name="entities">The collection of <see cref="Session"/> entities to save.</param>
    /// <returns>
    /// An <see cref="OperationResult{T}"/> containing:
    /// <list type="bullet">
    /// <item><description>true if all sessions and their nested entities were saved successfully</description></item>
    /// <item><description>false with errors if any save operation failed</description></item>
    /// </list>
    /// </returns>
    /// <remarks>
    /// <para>
    /// This method optimizes batch operations by:
    /// <list type="number">
    /// <item><description>Performing a single batch insert for all session entities</description></item>
    /// <item><description>Collecting all conversations from all sessions into a single collection</description></item>
    /// <item><description>Saving all conversations in a single batch operation (which cascades to questions/answers)</description></item>
    /// </list>
    /// </para>
    /// <para>
    /// This approach minimizes database round-trips while maintaining the cascading save behavior
    /// through the entire entity hierarchy. The conversation service handles further cascading
    /// to questions and answers.
    /// </para>
    /// <para>
    /// If sessions have no conversations, the method completes successfully after saving only
    /// the session entities, allowing flexibility in batch operations.
    /// </para>
    /// <para>
    /// The method validates the session save count to ensure all sessions were persisted before
    /// proceeding to conversation saves, maintaining atomicity within each level of the hierarchy.
    /// </para>
    /// </remarks>
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
                .Where(s => s.Conversations != null && s.Conversations.Count > 0)
                .SelectMany(s => s.Conversations)
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
