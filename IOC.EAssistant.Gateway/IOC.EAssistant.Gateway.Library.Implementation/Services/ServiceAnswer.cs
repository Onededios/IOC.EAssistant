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
