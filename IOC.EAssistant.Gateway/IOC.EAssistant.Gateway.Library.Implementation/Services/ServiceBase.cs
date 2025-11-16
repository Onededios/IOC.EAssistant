using IOC.EAssistant.Gateway.Infrastructure.Contracts.Databases;
using IOC.EAssistant.Gateway.Library.Contracts.Services;
using IOC.EAssistant.Gateway.XCutting.Results;
using Microsoft.Extensions.Logging;

namespace IOC.EAssistant.Gateway.Library.Implementation.Services;

/// <summary>
/// Provides the base implementation for service operations on entities of type <typeparamref name="TEntity"/>.
/// </summary>
/// <typeparam name="TEntity">The entity type that this service manages.</typeparam>
/// <remarks>
/// This abstract class implements common CRUD operations (Get, Delete) while requiring
/// derived classes to implement Save operations. It provides centralized error handling,
/// logging, and helper methods for consistent error messaging across all entity services.
/// </remarks>
/// <param name="_logger">The logger instance for tracking operations and errors.</param>
/// <param name="_repository">The database repository for entity operations.</param>
public abstract class ServiceBase<TEntity>(
        ILogger _logger,
        IDatabaseEAssistantBase<TEntity> _repository
    ) : IServiceBase<TEntity>
{
    /// <summary>
    /// Creates a standardized error result for entity not found scenarios.
    /// </summary>
    /// <param name="id">The unique identifier of the entity that was not found.</param>
    /// <returns>An <see cref="ErrorResult"/> with a descriptive message including the entity type and ID.</returns>
    protected static ErrorResult SearchingForIdErrorResult(Guid id) => new ErrorResult($"Entity of type {typeof(TEntity).Name} with ID {id} not found.", $"search for {typeof(TEntity).Name}");
    
    /// <summary>
    /// Creates a standardized error result for general action failures.
    /// </summary>
 /// <param name="action">The action that was being performed when the error occurred (e.g., "saving", "deleting").</param>
    /// <returns>An <see cref="ErrorResult"/> with a descriptive message including the entity type and action.</returns>
    protected static ErrorResult ActionErrorResult(string action) => new ErrorResult($"An error occurred while {action} the {typeof(TEntity).Name}.", action);
    
    /// <summary>
    /// Creates a standardized error result for cascading save failures involving related entities.
    /// </summary>
    /// <typeparam name="T1">The primary entity type that was being saved.</typeparam>
    /// <typeparam name="T2">The related entity type that failed to save.</typeparam>
    /// <returns>
    /// An <see cref="ErrorResult"/> indicating that the primary entity saved successfully
    /// but the related entity failed.
    /// </returns>
    protected static ErrorResult ActionSavingResult<T1, T2>() => new ErrorResult($"{typeof(T1).Name} but {typeof(T2).Name} failed to save.", "multiple entity saving");
    
    /// <summary>
/// Retrieves an entity by its unique identifier from the data store.
/// </summary>
/// <param name="id">The unique identifier of the entity to retrieve.</param>
    /// <returns>
    /// An <see cref="OperationResult{T}"/> containing:
    /// <list type="bullet">
    /// <item><description>The entity if found</description></item>
    /// <item><description>null with an error if the entity doesn't exist</description></item>
    /// <item><description>An error result if an exception occurred</description></item>
    /// </list>
    /// </returns>
    /// <remarks>
    /// This method logs all retrieval attempts and their outcomes (success, not found, or error).
    /// If the entity is not found, an error is added to the operation result.
    /// </remarks>
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

    /// <summary>
    /// Deletes an entity by its unique identifier from the data store.
    /// </summary>
    /// <param name="id">The unique identifier of the entity to delete.</param>
    /// <returns>
    /// An <see cref="OperationResult{T}"/> containing:
    /// <list type="bullet">
    /// <item><description>true if the entity was deleted successfully</description></item>
    /// <item><description>false if the entity doesn't exist or deletion failed</description></item>
    /// </list>
    /// </returns>
    /// <remarks>
    /// This method logs all deletion attempts and their outcomes. The repository returns
    /// the number of affected rows, which is converted to a boolean result (> 0 = success).
    /// </remarks>
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

    /// <summary>
    /// Saves a single entity to the data store.
    /// </summary>
    /// <param name="entity">The entity to save.</param>
    /// <returns>
    /// An <see cref="OperationResult{T}"/> containing true if the save was successful,
    /// false otherwise with error details.
    /// </returns>
    /// <remarks>
    /// This abstract method must be implemented by derived classes to provide entity-specific
 /// save logic, including handling of related entities and business rules.
    /// </remarks>
    public abstract Task<OperationResult<bool>> SaveAsync(TEntity entity);

    /// <summary>
    /// Saves multiple entities to the data store in a batch operation.
    /// </summary>
  /// <param name="entities">The collection of entities to save.</param>
    /// <returns>
    /// An <see cref="OperationResult{T}"/> containing true if all saves were successful,
    /// false otherwise with error details.
    /// </returns>
    /// <remarks>
    /// This abstract method must be implemented by derived classes to provide optimized
    /// batch save logic, potentially including transaction management and cascading saves
    /// for related entities.
    /// </remarks>
    public abstract Task<OperationResult<bool>> SaveMultipleAsync(IEnumerable<TEntity> entities);
}
