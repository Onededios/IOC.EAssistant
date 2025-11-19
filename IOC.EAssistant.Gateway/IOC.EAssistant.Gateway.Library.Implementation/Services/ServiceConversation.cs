using IOC.EAssistant.Gateway.Infrastructure.Contracts.Databases;
using IOC.EAssistant.Gateway.Library.Contracts.Services;
using IOC.EAssistant.Gateway.Library.Entities.Databases.EAssistant;
using IOC.EAssistant.Gateway.XCutting.Results;
using Microsoft.Extensions.Logging;

namespace IOC.EAssistant.Gateway.Library.Implementation.Services;

/// <summary>
/// Provides service operations for managing <see cref="Conversation"/> entities and their related questions.
/// </summary>
/// <remarks>
/// <para>
/// This service implements cascading save operations to handle the parent-child relationship
/// between conversations and questions. It ensures that when a conversation is saved, all
/// associated questions (and their answers) are also persisted correctly.
/// </para>
/// <para>
/// The service implements duplicate detection at the conversation level, allowing new questions
/// to be added to existing conversations without re-saving the conversation itself.
/// </para>
/// </remarks>
/// <param name="_logger">The logger instance for tracking operations and errors.</param>
/// <param name="_repository">The database repository for Conversation entity operations.</param>
/// <param name="_serviceQuestion">The question service for managing related question entities.</param>
public class ServiceConversation(
      ILogger<ServiceConversation> _logger,
    IDatabaseEAssistantBase<Conversation> _repository,
    IServiceQuestion _serviceQuestion
    ) : ServiceBase<Conversation>(_logger, _repository), IServiceConversation
{
    /// <summary>
    /// Saves a single conversation entity along with its associated questions to the data store.
    /// </summary>
    /// <param name="entity">The <see cref="Conversation"/> entity to save, including its questions collection.</param>
    /// <returns>
    /// An <see cref="OperationResult{T}"/> containing:
    /// <list type="bullet">
    /// <item><description>true if the conversation and all questions were saved successfully</description></item>
    /// <item><description>false with errors if any save operation failed</description></item>
    /// </list>
    /// </returns>
    /// <exception cref="InvalidOperationException">Thrown when database operation fails.</exception>
    /// <exception cref="TimeoutException">Thrown when database operation times out.</exception>
    /// <remarks>
    /// <para>
    /// This method implements a two-phase save operation:
    /// <list type="number">
    /// <item><description>Checks if the conversation exists; if not, saves the conversation entity</description></item>
    /// <item><description>Saves all questions in the conversation's Questions collection</description></item>
    /// </list>
    /// </para>
    /// <para>
    /// If the conversation already exists, only the questions are saved. This allows adding
    /// new questions to existing conversations without modifying the conversation record itself.
    /// </para>
    /// <para>
    /// The method uses cascading saves through the question service, which in turn saves
    /// the associated answers, maintaining referential integrity across all levels.
    /// </para>
    /// <para>
    /// Infrastructure exceptions are allowed to propagate to middleware for proper error handling.
    /// </para>
    /// </remarks>
    public override async Task<OperationResult<bool>> SaveAsync(Conversation entity)
    {
        var operationResult = new OperationResult<bool>();

        _logger.LogInformation("Saving Conversation with ID: {ConversationId} for Session ID: {SessionId}", entity.Id, entity.IdSession);

        var existingConversation = await _repository.GetAsync(entity.Id);
        var conversationAlreadyExists = existingConversation != null;

        if (!conversationAlreadyExists)
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
        }
        else
        {
            _logger.LogInformation("Conversation with ID: {ConversationId} already exists, saving only child entities", entity.Id);
        }

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

            _logger.LogInformation("Successfully saved {Count} Questions for Conversation ID: {ConversationId}", entity.Questions.Count, entity.Id);
        }

        operationResult.AddResult(true);
        return operationResult;
    }

    /// <summary>
    /// Saves multiple conversation entities along with their questions in a batch operation.
    /// </summary>
    /// <param name="entities">The collection of <see cref="Conversation"/> entities to save.</param>
    /// <returns>
    /// An <see cref="OperationResult{T}"/> containing:
    /// <list type="bullet">
    /// <item><description>true if all conversations and questions were saved successfully</description></item>
    /// <item><description>false with errors if any save operation failed</description></item>
    /// </list>
    /// </returns>
    /// <exception cref="InvalidOperationException">Thrown when database operation fails.</exception>
    /// <exception cref="TimeoutException">Thrown when database operation times out.</exception>
    /// <remarks>
    /// <para>
    /// This method optimizes batch operations by:
    /// <list type="number">
    /// <item><description>Separating new conversations from existing ones through existence checks</description></item>
    /// <item><description>Performing a single batch insert for all new conversations</description></item>
    /// <item><description>Collecting questions from both new and existing conversations</description></item>
    /// <item><description>Saving all questions in a single batch operation</description></item>
    /// </list>
    /// </para>
    /// <para>
    /// For existing conversations, only their new questions are saved, preventing unnecessary
    /// conversation updates and maintaining data integrity.
    /// </para>
    /// <para>
    /// If all conversations already exist and no questions need saving, the method returns
    /// success without performing database writes, ensuring idempotent behavior.
    /// </para>
    /// <para>
    /// Infrastructure exceptions are allowed to propagate to middleware for proper error handling.
    /// </para>
    /// </remarks>
    public override async Task<OperationResult<bool>> SaveMultipleAsync(IEnumerable<Conversation> entities)
    {
        var operationResult = new OperationResult<bool>();
        var entityList = entities.ToList();

        _logger.LogInformation("Saving {Count} Conversations", entityList.Count);

        var newConversations = new List<Conversation>();
        var existingConversationsWithNewQuestions = new List<Conversation>();

        foreach (var entity in entityList)
        {
            var existingConversation = await _repository.GetAsync(entity.Id);
            if (existingConversation == null)
            {
                newConversations.Add(entity);
            }
            else
            {
                _logger.LogInformation("Conversation with ID: {ConversationId} already exists, will save only new questions", entity.Id);
                existingConversationsWithNewQuestions.Add(entity);
            }
        }

        if (newConversations.Count > 0)
        {
            var conversationSaveCount = await _repository.SaveMultipleAsync(newConversations);

            if (conversationSaveCount == 0)
            {
                _logger.LogWarning("Failed to save any new Conversations");
                operationResult.AddResult(false);
                return operationResult;
            }

            _logger.LogInformation("Successfully saved {Count} new Conversations", conversationSaveCount);
        }

        var allQuestions = new List<Question>();

        allQuestions.AddRange(
            newConversations
            .Where(c => c.Questions != null && c.Questions.Count > 0)
            .SelectMany(c => c.Questions)
        );

        allQuestions.AddRange(
            existingConversationsWithNewQuestions
            .Where(c => c.Questions != null && c.Questions.Count > 0)
            .SelectMany(c => c.Questions)
        );

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
        else if (newConversations.Count == 0)
        {
            _logger.LogInformation("All conversations already exist and no new questions to save");
        }

        operationResult.AddResult(true);
        return operationResult;
    }
}
