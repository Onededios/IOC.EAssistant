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
