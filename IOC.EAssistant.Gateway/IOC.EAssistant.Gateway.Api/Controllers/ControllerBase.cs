using IOC.EAssistant.Gateway.Library.Contracts.Services;
using IOC.EAssistant.Gateway.XCutting.Results;
using Microsoft.AspNetCore.Mvc;

namespace IOC.EAssistant.Gateway.Api.Controllers;
public class ControllerBase<TEntity>(ILogger _logger, IServiceBase<TEntity> _service) : ControllerBase
{
    [HttpGet(nameof(TEntity))]
    public async Task<OperationResult<TEntity>> ById(Guid id)
    {
        var res = await _service.GetAsync(id);
        return res;
    }
}
