using IOC.EAssistant.Gateway.Library.Contracts.Services;
using IOC.EAssistant.Gateway.Library.Entities.Base;
using IOC.EAssistant.Gateway.XCutting.Results;
using Microsoft.AspNetCore.Mvc;

namespace IOC.EAssistant.Gateway.Api.Controllers;

[ApiController]
[Route("[controller]")]
[Produces("application/json")]
public class HealthController(
    ILogger<HealthController> _logger,
    IServiceHealthCheck _serviceHealth
) : ControllerBase
{
    /// <summary>
    /// Performs a health check to determine the availability of the service model.
    /// </summary>
    /// <remarks>This endpoint returns an HTTP 200 status code if the health check is successful. It provides additional details about the service's health status.</remarks>
    /// <returns>An <see cref="ActionResult{T}"/> containing a <see cref="HealthResponse"/> object that indicates the health
    /// status of the service. The response includes a flag indicating whether the model is available.</returns>
    [HttpGet]
    [ProducesResponseType(200)]
    public async Task<OperationResult<HealthResponse>> Health()
    {
        var res = await _serviceHealth.GetHealthAsync();
        return res;
    }
}
