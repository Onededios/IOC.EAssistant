using IOC.EAssistant.Gateway.Library.Contracts.Services;
using IOC.EAssistant.Gateway.Library.Entities.Base;
using IOC.EAssistant.Gateway.XCutting.Results;
using Microsoft.AspNetCore.Mvc;

namespace IOC.EAssistant.Gateway.Api.Controllers;

/// <summary>
/// Controller for monitoring the health status of the EAssistant Gateway API and its dependencies.
/// </summary>
/// <remarks>
/// This controller provides endpoints to check the availability and health of the service,
/// particularly the AI model availability. It's typically used by monitoring tools, load balancers,
/// and orchestrators to determine service readiness and liveness.
/// </remarks>
[ApiController]
[Route("[controller]")]
[Produces("application/json")]
public class HealthController(
    ILogger<HealthController> _logger,
    IServiceHealthCheck _serviceHealth
) : ControllerBase
{
    /// <summary>
    /// Performs a health check to determine the availability of the service and its AI model.
    /// </summary>
    /// <remarks>
    /// This endpoint returns an HTTP 200 status code if the health check is successful. 
    /// It provides detailed information about the service's health status, including:
    /// <list type="bullet">
    /// <item><description>Model availability status</description></item>
    /// <item><description>Overall health status</description></item>
    /// <item><description>Timestamp of the health check</description></item>
    /// </list>
    /// This is useful for monitoring systems to verify that the API and its underlying AI model are operational.
    /// </remarks>
    /// <returns>
    /// An <see cref="OperationResult{T}"/> containing a <see cref="HealthResponse"/> object that indicates 
    /// the health status of the service. The response includes a flag indicating whether the model is available
    /// and the overall health status of the system.
    /// </returns>
    /// <response code="200">Returns the health status of the service, including model availability.</response>
    [HttpGet]
    [ProducesResponseType(200)]
    public async Task<OperationResult<HealthResponse>> Health()
    {
        var res = await _serviceHealth.GetHealthAsync();
        return res;
    }
}
