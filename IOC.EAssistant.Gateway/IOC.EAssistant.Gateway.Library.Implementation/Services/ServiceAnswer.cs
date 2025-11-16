using IOC.EAssistant.Gateway.Infrastructure.Contracts.Databases;
using IOC.EAssistant.Gateway.Library.Contracts.Services;
using IOC.EAssistant.Gateway.Library.Entities.Databases.EAssistant;
using IOC.EAssistant.Gateway.XCutting.Results;
using Microsoft.Extensions.Logging;

namespace IOC.EAssistant.Gateway.Library.Implementation.Services;

/// <summary>
/// Provides service operations for managing <see cref="Answer"/> entities in the data store.
/// </summary>
/// <remarks>
/// This service handles the persistence of AI-generated answers, implementing duplicate detection
/// logic to prevent saving the same answer multiple times. It extends <see cref="ServiceBase{T}"/>
/// to provide specialized answer management functionality.
/// </remarks>
/// <param name="_logger">The logger instance for tracking operations and errors.</param>
/// <param name="_repository">The database repository for Answer entity operations.</param>
public class ServiceAnswer(
        ILogger<ServiceAnswer> _logger,
        IDatabaseEAssistantBase<Answer> _repository
    ) : ServiceBase<Answer>(_logger, _repository), IServiceAnswer
{
    /// <summary>
    /// Saves a single answer entity to the data store, checking for duplicates before insertion.
    /// </summary>
    /// <param name="entity">The <see cref="Answer"/> entity to save.</param>
    /// <returns>
    /// An <see cref="OperationResult{T}"/> containing:
  /// <list type="bullet">
    /// <item><description>true if the answer was saved successfully or already exists</description></item>
    /// <item><description>false if the save operation failed</description></item>
    /// </list>
/// </returns>
    /// <remarks>
    /// This method implements idempotent behavior by checking if an answer with the same ID
/// already exists. If found, it skips the save operation and returns success. This prevents
    /// duplicate answers when retrying operations or processing the same question multiple times.
    /// </remarks>
    public override async Task<OperationResult<bool>> SaveAsync(Answer entity)
    {
        var operationResult = new OperationResult<bool>();

        _logger.LogInformation("Saving Answer for Question ID: {QuestionId}", entity.IdQuestion);

        try
        {
            var existingAnswer = await _repository.GetAsync(entity.Id);
            var answerAlreadyExists = existingAnswer != null;

            if (answerAlreadyExists)
            {
                _logger.LogInformation("Answer with ID: {AnswerId} already exists, skipping save", entity.Id);
                operationResult.AddResult(true);
                return operationResult;
            }

            var answerSaveCount = await _repository.SaveAsync(entity);
            var answerSaved = answerSaveCount > 0;

            if (!answerSaved)
            {
                _logger.LogWarning("Failed to save Answer for Question ID: {QuestionId}", entity.IdQuestion);
                operationResult.AddResult(false);
                return operationResult;
            }

            _logger.LogInformation("Successfully saved Answer with ID: {AnswerId} for Question ID: {QuestionId}",
                entity.Id, entity.IdQuestion);
            operationResult.AddResult(true);
        }
        catch (Exception ex)
        {
            operationResult.AddResultWithError(false, ActionErrorResult("saving"), -1, ex);
            _logger.LogError(ex, "Error saving Answer for Question ID: {QuestionId}", entity.IdQuestion);
        }

        return operationResult;
    }

    /// <summary>
    /// Saves multiple answer entities to the data store in a batch operation.
    /// </summary>
    /// <param name="entities">The collection of <see cref="Answer"/> entities to save.</param>
    /// <returns>
    /// An <see cref="OperationResult{T}"/> containing:
    /// <list type="bullet">
    /// <item><description>true if all new answers were saved successfully</description></item>
    /// <item><description>false if the batch save operation failed</description></item>
    /// </list>
    /// </returns>
    /// <remarks>
    /// <para>
    /// This method optimizes batch operations by:
    /// <list type="number">
    /// <item><description>Checking each answer against the database to detect existing entries</description></item>
    /// <item><description>Filtering out duplicates to create a list of only new answers</description></item>
    /// <item><description>Performing a single batch insert for all new answers</description></item>
    /// </list>
    /// </para>
    /// <para>
/// If all answers already exist in the database, the method returns success without performing
    /// any database write operations, making it safe for idempotent scenarios.
    /// </para>
    /// </remarks>
    public override async Task<OperationResult<bool>> SaveMultipleAsync(IEnumerable<Answer> entities)
    {
        var operationResult = new OperationResult<bool>();
        var entityList = entities.ToList();

        _logger.LogInformation("Saving {Count} Answers", entityList.Count);

        try
        {
            var newAnswers = new List<Answer>();
            foreach (var entity in entityList)
            {
                var existingAnswer = await _repository.GetAsync(entity.Id);
                if (existingAnswer == null)
                {
                    newAnswers.Add(entity);
                }
                else
                {
                    _logger.LogInformation("Answer with ID: {AnswerId} already exists, skipping", entity.Id);
                }
            }

            if (newAnswers.Count == 0)
            {
                _logger.LogInformation("All answers already exist, nothing to save");
                operationResult.AddResult(true);
                return operationResult;
            }

            var answerSaveCount = await _repository.SaveMultipleAsync(newAnswers);

            if (answerSaveCount == 0)
            {
                _logger.LogWarning("Failed to save any new Answers");
                operationResult.AddResult(false);
                return operationResult;
            }

            _logger.LogInformation("Successfully saved {Count} new Answers", answerSaveCount);
            operationResult.AddResult(true);
        }
        catch (Exception ex)
        {
            operationResult.AddResultWithError(false, ActionErrorResult("saving"), -1, ex);
            _logger.LogError(ex, "Error saving multiple Answers");
        }

        return operationResult;
    }
}
