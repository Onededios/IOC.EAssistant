using IOC.EAssistant.Gateway.Infrastructure.Contracts.Databases;
using IOC.EAssistant.Gateway.Library.Contracts.Services;
using IOC.EAssistant.Gateway.Library.Entities.Databases.EAssistant;
using IOC.EAssistant.Gateway.XCutting.Results;
using Microsoft.Extensions.Logging;

namespace IOC.EAssistant.Gateway.Library.Implementation.Services;
public class ServiceQuestion(
        ILogger<ServiceQuestion> _logger,
        IDatabaseEAssistantBase<Question> _repository,
        IServiceAnswer _serviceAnswer
    ) : ServiceBase<Question>(_logger, _repository), IServiceQuestion
{
    public override async Task<OperationResult<bool>> SaveAsync(Question entity)
    {
        var operationResult = new OperationResult<bool>();

        _logger.LogInformation("Saving Question with ID: {QuestionId}", entity.id);

        try
        {
            var questionSaveCount = await _repository.SaveAsync(entity);
            var questionSaved = questionSaveCount > 0;

            if (!questionSaved)
            {
                _logger.LogWarning("Failed to save Question with ID: {QuestionId}", entity.id);
                operationResult.AddResult(false);
                return operationResult;
            }

            _logger.LogInformation("Successfully saved Question with ID: {QuestionId}", entity.id);

            if (entity.answer != null)
            {
                _logger.LogInformation("Saving associated Answer for Question ID: {QuestionId}", entity.id);
                var answerResult = await _serviceAnswer.SaveAsync(entity.answer);

                if (answerResult.HasErrors)
                {
                    _logger.LogError("Failed to save Answer for Question ID: {QuestionId}", entity.id);
                    operationResult.AddResultWithError(false, ActionSavingResult<Question, Answer>(), -1, null);
                    return operationResult;
                }

                _logger.LogInformation("Successfully saved Answer for Question ID: {QuestionId}", entity.id);
            }

            operationResult.AddResult(true);
        }
        catch (Exception ex)
        {
            operationResult.AddResultWithError(false, ActionErrorResult("saving"), -1, ex);
            _logger.LogError(ex, "Error saving Question with ID: {QuestionId}", entity.id);
        }

        return operationResult;
    }

    public override async Task<OperationResult<bool>> SaveMultipleAsync(IEnumerable<Question> entities)
    {
        var operationResult = new OperationResult<bool>();
        var entityList = entities.ToList();

        _logger.LogInformation("Saving {Count} Questions", entityList.Count);

        try
        {
            var questionSaveCount = await _repository.SaveMultipleAsync(entityList);

            if (questionSaveCount == 0)
            {
                _logger.LogWarning("Failed to save any Questions");
                operationResult.AddResult(false);
                return operationResult;
            }

            _logger.LogInformation("Successfully saved {Count} Questions", questionSaveCount);

            var answersToSave = entityList
                .Where(q => q.answer != null)
                .Select(q => q.answer!)
                .ToList();

            if (answersToSave.Count > 0)
            {
                _logger.LogInformation("Saving {Count} associated Answers", answersToSave.Count);
                var answerResult = await _serviceAnswer.SaveMultipleAsync(answersToSave);

                if (answerResult.HasErrors)
                {
                    _logger.LogError("Failed to save some or all Answers");
                    operationResult.AddResultWithError(false, ActionSavingResult<Question, Answer>(), -1, null);
                    return operationResult;
                }

                _logger.LogInformation("Successfully saved {Count} Answers", answersToSave.Count);
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
