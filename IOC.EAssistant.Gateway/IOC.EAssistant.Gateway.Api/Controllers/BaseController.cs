using IOC.EAssistant.Gateway.Library.Contracts.Services;
using Microsoft.AspNetCore.Mvc;

namespace IOC.EAssistant.Gateway.Api.Controllers;
[ApiController]
[Route("[controller]")]
[Produces("application/json")]
public class BaseController<TEntity>(ILogger _logger, IServiceBase<TEntity> _service) : ControllerBase
{
    [HttpGet("{id}")]
    public async Task<ActionResult<IEnumerable<TEntity>>> ById(Guid id)
    {
        var res = await _service.GetAsync(id);
        return res.ToActionResult<IEnumerable<TEntity>>(this);
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult<bool>> Delete(Guid id)
    {
        var res = await _service.DeleteAsync(id);
        return res.ToActionResult<bool>(this);
    }
}
