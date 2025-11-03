using IOC.EAssistant.Gateway.Infrastructure.Contracts.Databases;
using IOC.EAssistant.Gateway.Library.Contracts.Services;
using IOC.EAssistant.Gateway.Library.Entities.Databases.EAssistant;
using IOC.EAssistant.Gateway.XCutting.Results;
using Microsoft.Extensions.Logging;

namespace IOC.EAssistant.Gateway.Library.Implementation.Services;
public class ServiceAnswer(
        ILogger<ServiceAnswer> _logger,
        IDatabaseEAssistantBase<Answer> _repository
    ) : ServiceBase<Answer>(_logger, _repository), IServiceAnswer
{
    public override async Task<OperationResult<bool>> SaveAsync(Answer entity)
    {
        var operationResult = new OperationResult<bool>();

        _logger.LogInformation("Saving Answer for Question ID: {QuestionId}", entity.question_id);

        try
        {
            var answerSaveCount = await _repository.SaveAsync(entity);
            var answerSaved = answerSaveCount > 0;

            if (!answerSaved)
            {
                _logger.LogWarning("Failed to save Answer for Question ID: {QuestionId}", entity.question_id);
                operationResult.AddResult(false);
                return operationResult;
            }

            _logger.LogInformation("Successfully saved Answer with ID: {AnswerId} for Question ID: {QuestionId}",
                entity.id, entity.question_id);
            operationResult.AddResult(true);
        }
        catch (Exception ex)
        {
            operationResult.AddResultWithError(false,
                $"An error occurred while saving Answer for Question ID: {entity.question_id}", -1, ex);
            _logger.LogError(ex, "Error saving Answer for Question ID: {QuestionId}", entity.question_id);
        }

        return operationResult;
    }

    public override async Task<OperationResult<bool>> SaveMultipleAsync(IEnumerable<Answer> entities)
    {
        var operationResult = new OperationResult<bool>();
        var entityList = entities.ToList();

        _logger.LogInformation("Saving {Count} Answers", entityList.Count);

        try
        {
            var answerSaveCount = await _repository.SaveMultipleAsync(entityList);

            if (answerSaveCount == 0)
            {
                _logger.LogWarning("Failed to save any Answers");
                operationResult.AddResult(false);
                return operationResult;
            }

            _logger.LogInformation("Successfully saved {Count} Answers", answerSaveCount);
            operationResult.AddResult(true);
        }
        catch (Exception ex)
        {
            operationResult.AddResultWithError(false,
                $"An error occurred while saving {entityList.Count} Answers.", -1, ex);
            _logger.LogError(ex, "Error saving multiple Answers");
        }

        return operationResult;
    }
}
