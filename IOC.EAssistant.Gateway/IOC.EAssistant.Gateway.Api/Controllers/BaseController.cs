using IOC.EAssistant.Gateway.Library.Contracts.Services;
using IOC.EAssistant.Gateway.XCutting.Results;
using Microsoft.AspNetCore.Mvc;

namespace IOC.EAssistant.Gateway.Api.Controllers;

[ApiController]
[Route("[controller]")]
public class BaseController<TEntity>(ILogger _logger, IServiceBase<TEntity> _service) : ControllerBase
{

    /// <summary>
    /// Retrieves an entity by its unique identifier.
    /// </summary>
    /// <remarks>This method returns a 200 OK response if the entity is found. If the entity is not found or
    /// an error occurs, the response will indicate the appropriate HTTP status code.</remarks>
    /// <param name="id">The unique identifier of the entity to retrieve.</param>
    /// <returns>An <see cref="ActionResult{T}"/> containing a collection of entities of type <typeparamref name="TEntity"/> if
    /// the operation is successful; otherwise, an appropriate HTTP response indicating the error.</returns>
    [HttpGet("{id}")]
    [ProducesResponseType(200)]
    public async Task<OperationResult<TEntity?>> ById(Guid id)
    {
        var res = await _service.GetAsync(id);
        return res;
    }

    /// <summary>
    /// Deletes the resource identified by the specified ID.
    /// </summary>
    /// <remarks>Returns a response with a status code of 200 (OK) and a value of <see langword="true"/> if
    /// the resource was successfully deleted; otherwise, <see langword="false"/>.</remarks>
    /// <param name="id">The unique identifier of the resource to delete.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains an <see cref="ActionResult{T}"/>
    /// indicating whether the deletion was successful.</returns>
    [HttpDelete("{id}")]
    [ProducesResponseType(typeof(bool), 200)]
    public async Task<OperationResult<bool>> Delete(Guid id)
    {
        var res = await _service.DeleteAsync(id);
        return res;
    }
}
