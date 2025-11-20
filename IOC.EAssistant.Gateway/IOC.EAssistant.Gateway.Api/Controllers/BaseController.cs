using IOC.EAssistant.Gateway.Library.Contracts.Services;
using IOC.EAssistant.Gateway.XCutting.Results;
using Microsoft.AspNetCore.Mvc;

namespace IOC.EAssistant.Gateway.Api.Controllers;

/// <summary>
/// Base controller providing common CRUD operations for entity management.
/// </summary>
/// <typeparam name="TEntity">The type of entity managed by this controller.</typeparam>
/// <remarks>
/// This abstract controller provides standard RESTful endpoints for retrieving and deleting entities.
/// It leverages the <see cref="IServiceBase{T}"/> interface to perform business logic operations
/// and returns results using the <see cref="OperationResult{TResult}"/> pattern for consistent error handling.
/// Derived controllers inherit these basic operations and can extend with additional endpoints as needed.
/// </remarks>
[ApiController]
[Route("[controller]")]
public class BaseController<TEntity>(ILogger _logger, IServiceBase<TEntity> _service) : ControllerBase
{
    /// <summary>
    /// Retrieves an entity by its unique identifier.
    /// </summary>
    /// <param name="id">The unique identifier (GUID) of the entity to retrieve.</param>
    /// <returns>
    /// A task that represents the asynchronous operation. The task result contains an
    /// <see cref="OperationResult{T}"/> with the entity of type <typeparamref name="TEntity"/> if found,
    /// or <see langword="null"/> if no entity with the specified ID exists.
    /// </returns>
    /// <response code="200">Returns the requested entity or null if not found.</response>
    /// <remarks>
    /// This endpoint performs a GET request to retrieve a single entity by its ID.
    /// The operation result may contain errors if validation fails or exceptions if an error occurs during retrieval.
    /// </remarks>
    [HttpGet("{id}")]
    [ProducesResponseType(200)]
    public async Task<OperationResult<TEntity?>> ById(Guid id)
    {
        var res = await _service.GetAsync(id);
        return res;
    }

    /// <summary>
    /// Deletes an entity by its unique identifier.
    /// </summary>
    /// <param name="id">The unique identifier (GUID) of the entity to delete.</param>
    /// <returns>
    /// A task that represents the asynchronous operation. The task result contains an
    /// <see cref="OperationResult{T}"/> with a boolean value indicating whether the deletion was successful.
    /// Returns <see langword="true"/> if the entity was deleted successfully, otherwise <see langword="false"/>.
    /// </returns>
    /// <response code="200">Returns true if the entity was deleted successfully, false otherwise.</response>
    /// <remarks>
    /// This endpoint performs a DELETE request to remove an entity by its ID.
    /// The operation result may contain errors if validation fails or exceptions if an error occurs during deletion.
    /// </remarks>
    [HttpDelete("{id}")]
    [ProducesResponseType(typeof(bool), 200)]
    public async Task<OperationResult<bool>> Delete(Guid id)
    {
        var res = await _service.DeleteAsync(id);
        return res;
    }
}
