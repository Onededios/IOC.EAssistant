using IOC.EAssistant.Gateway.Library.Contracts.Services;
using IOC.EAssistant.Gateway.XCutting.Results;
using Microsoft.AspNetCore.Mvc;

namespace IOC.EAssistant.Gateway.Api.Controllers;
[ApiController]
[Route("[controller]")]
[ProducesResponseType(typeof(List<ErrorResult>), 400)]
public class BaseController<TEntity>(ILogger _logger, IServiceBase<TEntity> _service) : ControllerBase
{
    [HttpGet("{id}")]
    [ProducesResponseType(200)]
    public async Task<ActionResult<IEnumerable<TEntity>>> ById(Guid id)
    {
        var res = await _service.GetAsync(id);
        return res.ToActionResult<IEnumerable<TEntity>>(this);
    }

    [HttpDelete("{id}")]
    [ProducesResponseType(typeof(bool), 200)]
    public async Task<ActionResult<bool>> Delete(Guid id)
    {
        var res = await _service.DeleteAsync(id);
        return res.ToActionResult<bool>(this);
    }
}
