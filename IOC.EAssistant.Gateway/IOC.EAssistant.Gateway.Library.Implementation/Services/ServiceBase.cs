using IOC.EAssistant.Gateway.Library.Contracts.Services;
using IOC.EAssistant.Gateway.Infrastructure.Contracts.Databases;
using IOC.EAssistant.Gateway.XCutting.Results;
using Microsoft.Extensions.Logging;

namespace IOC.EAssistant.Gateway.Library.Implementation.Services;
public abstract class ServiceBase<TEntity>(
        ILogger _logger,
        IDatabaseEAssistantBase<TEntity> _repository
    ) : IServiceBase<TEntity>
{

    protected static ErrorResult SearchingForIdErrorResult(Guid id) => new ErrorResult($"Entity of type {typeof(TEntity).Name} with ID {id} not found.", $"search for {typeof(TEntity).Name}");
    protected static ErrorResult ActionErrorResult(string action) => new ErrorResult($"An error occurred while {action} the {typeof(TEntity).Name}.", action);
    protected static ErrorResult ActionSavingResult<T1, T2>() => new ErrorResult($"{typeof(T1).Name} but {typeof(T2).Name} failed to save.", "multiple entity saving");
    public async Task<OperationResult<TEntity?>> GetAsync(Guid id)
    {
        var operationResult = new OperationResult<TEntity>();

        _logger.LogInformation("Getting entity of type {EntityType} with ID {EntityId}", typeof(TEntity).Name, id);

        try
        {
            var res = await _repository.GetAsync(id);

            if (!EqualityComparer<TEntity>.Default.Equals(res, default))
            {
                operationResult.AddResult(res);
                _logger.LogInformation("Successfully retrieved entity of type {EntityType} with ID {EntityId}", typeof(TEntity).Name, id);
            }
            else
            {
                operationResult.AddError(SearchingForIdErrorResult(id), default);
                _logger.LogWarning("Entity of type {EntityType} with ID {EntityId} not found", typeof(TEntity).Name, id);
            }
        }
        catch (Exception ex)
        {
            operationResult.AddError(ActionErrorResult("retrieving"), ex);
            _logger.LogError(ex, "Error retrieving entity of type {EntityType} with ID {EntityId}", typeof(TEntity).Name, id);
        }

        return operationResult;
    }

    public async Task<OperationResult<bool>> DeleteAsync(Guid id)
    {
        var operationResult = new OperationResult<bool>();

        _logger.LogInformation("Deleting entity of type {EntityType} with ID {EntityId}", typeof(TEntity).Name, id);

        try
        {
            var res = await _repository.DeleteAsync(id);
            operationResult.AddResult(res > 0);
            _logger.LogInformation("Successfully deleted entity of type {EntityType} with ID {EntityId}", typeof(TEntity).Name, id);
        }
        catch (Exception ex)
        {
            operationResult.AddResultWithError(false, ActionErrorResult("deleting"), -1, ex);
            _logger.LogError(ex, "Error deleting entity of type {EntityType} with ID {EntityId}", typeof(TEntity).Name, id);
        }

        return operationResult;
    }

    public abstract Task<OperationResult<bool>> SaveAsync(TEntity entity);

    public abstract Task<OperationResult<bool>> SaveMultipleAsync(IEnumerable<TEntity> entities);
}
