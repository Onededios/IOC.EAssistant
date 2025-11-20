using IOC.EAssistant.Gateway.XCutting.Results;

namespace IOC.EAssistant.Gateway.Library.Contracts.Services;

/// <summary>
/// Defines the base contract for service operations on entities of type <typeparamref name="T"/>.
/// </summary>
/// <typeparam name="T">The entity type that this service manages.</typeparam>
/// <remarks>
/// This interface provides standard CRUD (Create, Read, Update, Delete) operations
/// that return <see cref="OperationResult{T}"/> to handle success and error scenarios consistently.
/// All operations are asynchronous to support scalable and responsive applications.
/// </remarks>
public interface IServiceBase<T>
{
    /// <summary>
    /// Retrieves an entity by its unique identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the entity to retrieve.</param>
    /// <returns>
    /// An <see cref="OperationResult{T}"/> containing the entity if found, or null if not found.
    /// </returns>
    /// <remarks>
    /// This method returns null in the Result property if the entity doesn't exist,
    /// rather than adding an error to the operation result.
    /// </remarks>
    public Task<OperationResult<T>> GetAsync(Guid id);

    /// <summary>
    /// Saves a single entity to the data store.
    /// </summary>
    /// <param name="entity">The entity to save.</param>
    /// <returns>
    /// An <see cref="OperationResult{T}"/> containing true if the save was successful, 
    /// false otherwise with error details.
    /// </returns>
    /// <remarks>
    /// This method handles both insert and update operations. If the entity has an existing
    /// Id, it will be updated; otherwise, a new entity will be created.
    /// </remarks>
    public Task<OperationResult<bool>> SaveAsync(T entity);

    /// <summary>
    /// Deletes an entity by its unique identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the entity to delete.</param>
    /// <returns>
    /// An <see cref="OperationResult{T}"/> containing true if the deletion was successful,
    /// false otherwise with error details.
    /// </returns>
    /// <remarks>
    /// If the entity doesn't exist, this method may return false or handle it as a successful
    /// deletion depending on the implementation.
    /// </remarks>
    public Task<OperationResult<bool>> DeleteAsync(Guid id);

    /// <summary>
    /// Saves multiple entities to the data store in a single operation.
    /// </summary>
    /// <param name="entities">The collection of entities to save.</param>
    /// <returns>
    /// An <see cref="OperationResult{T}"/> containing true if all saves were successful,
    /// false otherwise with error details.
    /// </returns>
    /// <remarks>
    /// This method is optimized for batch operations and may use transactions to ensure
    /// data consistency. If any entity fails to save, the entire operation may be rolled back
    /// depending on the implementation.
    /// </remarks>
    public Task<OperationResult<bool>> SaveMultipleAsync(IEnumerable<T> entities);
}
