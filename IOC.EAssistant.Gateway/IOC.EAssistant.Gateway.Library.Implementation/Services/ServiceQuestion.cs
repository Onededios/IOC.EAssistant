using IOC.EAssistant.Gateway.Infrastructure.Contracts.Databases;
using IOC.EAssistant.Gateway.Library.Contracts.Services;
using IOC.EAssistant.Gateway.Library.Entities.Databases.EAssistant;
using IOC.EAssistant.Gateway.XCutting.Results;
using Microsoft.Extensions.Logging;

namespace IOC.EAssistant.Gateway.Library.Implementation.Services;

/// <summary>
/// Provides service operations for managing <see cref="Question"/> entities and their associated answers.
/// </summary>
/// <remarks>
/// <para>
/// This service manages the persistence of user questions and their AI-generated answers,
/// implementing cascading save operations to maintain the parent-child relationship between
/// questions and answers. It includes sophisticated duplicate detection logic to prevent
/// re-saving existing questions while allowing new answers to be added.
/// </para>
/// <para>
/// Questions are central to the conversation structure, containing user input, metadata,
/// token counts, and ordering information (index) necessary for maintaining conversation context.
/// </para>
/// </remarks>
/// <param name="_logger">The logger instance for tracking operations and errors.</param>
/// <param name="_repository">The database repository for Question entity operations.</param>
/// <param name="_serviceAnswer">The answer service for managing related answer entities.</param>
public class ServiceQuestion(
   ILogger<ServiceQuestion> _logger,
        IDatabaseEAssistantBase<Question> _repository,
   IServiceAnswer _serviceAnswer
    ) : ServiceBase<Question>(_logger, _repository), IServiceQuestion
{
    /// <summary>
    /// Saves a single question entity along with its associated answer to the data store.
    /// </summary>
    /// <param name="entity">The <see cref="Question"/> entity to save, including its answer.</param>
    /// <returns>
    /// An <see cref="OperationResult{T}"/> containing:
    /// <list type="bullet">
    /// <item><description>true if the question and answer were saved successfully</description></item>
    /// <item><description>false with errors if any save operation failed</description></item>
    /// </list>
    /// </returns>
    /// <remarks>
    /// <para>
    /// This method implements intelligent duplicate handling:
    /// <list type="number">
    /// <item><description>Checks if the question already exists in the database</description></item>
    /// <item><description>If new, saves the question entity first</description></item>
    /// <item><description>If existing, skips the question save but proceeds to answer save</description></item>
    /// <item><description>Saves the associated answer (which also implements duplicate detection)</description></item>
    /// </list>
    /// </para>
    /// <para>
    /// This behavior is crucial for retry scenarios or when the same question is processed
    /// multiple times (e.g., in distributed systems with message replay). It ensures that
    /// the answer is always attempted to be saved, even if the question was previously persisted.
    /// </para>
    /// <para>
    /// The cascading save maintains referential integrity between questions and answers
    /// through the IdQuestion foreign key relationship.
    /// </para>
    /// </remarks>
    public override async Task<OperationResult<bool>> SaveAsync(Question entity)
    {
        var operationResult = new OperationResult<bool>();

        _logger.LogInformation("Saving Question with ID: {QuestionId}", entity.Id);

        try
        {
            var existingQuestion = await _repository.GetAsync(entity.Id);
            var questionAlreadyExists = existingQuestion != null;

            if (!questionAlreadyExists)
            {
                var questionSaveCount = await _repository.SaveAsync(entity);
                var questionSaved = questionSaveCount > 0;

                if (!questionSaved)
                {
                    _logger.LogWarning("Failed to save Question with ID: {QuestionId}", entity.Id);
                    operationResult.AddResult(false);
                    return operationResult;
                }

                _logger.LogInformation("Successfully saved Question with ID: {QuestionId}", entity.Id);
            }
            else
            {
                _logger.LogInformation("Question with ID: {QuestionId} already exists, saving only child entities", entity.Id);
            }

            if (entity.Answer != null)
            {
                _logger.LogInformation("Saving associated Answer for Question ID: {QuestionId}", entity.Id);
                var answerResult = await _serviceAnswer.SaveAsync(entity.Answer);

                if (answerResult.HasErrors)
                {
                    _logger.LogError("Failed to save Answer for Question ID: {QuestionId}", entity.Id);
                    operationResult.AddResultWithError(false, ActionSavingResult<Question, Answer>(), -1, null);
                    return operationResult;
                }

                _logger.LogInformation("Successfully saved Answer for Question ID: {QuestionId}", entity.Id);
            }

            operationResult.AddResult(true);
        }
        catch (Exception ex)
        {
            operationResult.AddResultWithError(false, ActionErrorResult("saving"), -1, ex);
            _logger.LogError(ex, "Error saving Question with ID: {QuestionId}", entity.Id);
        }

        return operationResult;
    }

    /// <summary>
    /// Saves multiple question entities along with their answers in a batch operation.
    /// </summary>
  /// <param name="entities">The collection of <see cref="Question"/> entities to save.</param>
    /// <returns>
    /// An <see cref="OperationResult{T}"/> containing:
    /// <list type="bullet">
    /// <item><description>true if all questions and answers were saved successfully</description></item>
    /// <item><description>false with errors if any save operation failed</description></item>
    /// </list>
    /// </returns>
    /// <remarks>
    /// <para>
    /// This method optimizes batch operations through a sophisticated multi-phase process:
    /// <list type="number">
    /// <item><description>Separates new questions from existing ones via individual existence checks</description></item>
    /// <item><description>Performs a single batch insert for all new questions</description></item>
    /// <item><description>Collects answers from both new and existing questions</description></item>
    /// <item><description>Saves all answers in a single batch operation</description></item>
    /// </list>
    /// </para>
    /// <para>
    /// The dual-list approach (newQuestions and existingQuestionsWithNewAnswers) ensures that:
    /// <list type="bullet">
    /// <item><description>New questions are inserted efficiently in batch</description></item>
    /// <item><description>Existing questions don't cause constraint violations</description></item>
    /// <item><description>Answers for both new and existing questions are processed</description></item>
    /// <item><description>Retry operations are safe and idempotent</description></item>
    /// </list>
    /// </para>
    /// <para>
    /// If all questions exist and no new answers need saving, the method returns success
    /// without performing database writes, making it suitable for idempotent scenarios.
    /// </para>
    /// </remarks>
    public override async Task<OperationResult<bool>> SaveMultipleAsync(IEnumerable<Question> entities)
    {
        var operationResult = new OperationResult<bool>();
        var entityList = entities.ToList();

        _logger.LogInformation("Saving {Count} Questions", entityList.Count);

        try
        {
            var newQuestions = new List<Question>();
            var existingQuestionsWithNewAnswers = new List<Question>();

            foreach (var entity in entityList)
            {
                var existingQuestion = await _repository.GetAsync(entity.Id);
                if (existingQuestion == null)
                {
                    newQuestions.Add(entity);
                }
                else
                {
                    _logger.LogInformation("Question with ID: {QuestionId} already exists, will save only new answers", entity.Id);
                    existingQuestionsWithNewAnswers.Add(entity);
                }
            }

            if (newQuestions.Count > 0)
            {
                var questionSaveCount = await _repository.SaveMultipleAsync(newQuestions);

                if (questionSaveCount == 0)
                {
                    _logger.LogWarning("Failed to save any new Questions");
                    operationResult.AddResult(false);
                    return operationResult;
                }

                _logger.LogInformation("Successfully saved {Count} new Questions", questionSaveCount);
            }

            var allAnswers = new List<Answer>();

            allAnswers.AddRange(
                    newQuestions
          .Where(q => q.Answer != null)
                .Select(q => q.Answer!)
        );

            allAnswers.AddRange(
                   existingQuestionsWithNewAnswers
             .Where(q => q.Answer != null)
                        .Select(q => q.Answer!)
                );

            if (allAnswers.Count > 0)
            {
                _logger.LogInformation("Saving {Count} associated Answers", allAnswers.Count);
                var answerResult = await _serviceAnswer.SaveMultipleAsync(allAnswers);

                if (answerResult.HasErrors)
                {
                    _logger.LogError("Failed to save some or all Answers");
                    operationResult.AddResultWithError(false, ActionSavingResult<Question, Answer>(), -1, null);
                    return operationResult;
                }

                _logger.LogInformation("Successfully saved {Count} Answers", allAnswers.Count);
            }
            else if (newQuestions.Count == 0)
            {
                _logger.LogInformation("All questions already exist and no new answers to save");
            }

            operationResult.AddResult(true);
        }
        catch (Exception ex)
        {
            operationResult.AddResultWithError(false, ActionErrorResult("saving"), -1, ex);
            _logger.LogError(ex, "Error saving multiple Questions");
        }

        return operationResult;
    }
}
