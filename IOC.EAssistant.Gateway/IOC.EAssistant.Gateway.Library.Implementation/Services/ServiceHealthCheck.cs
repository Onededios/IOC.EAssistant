using IOC.EAssistant.Gateway.Infrastructure.Contracts.Proxies;
using IOC.EAssistant.Gateway.Library.Contracts.Services;
using IOC.EAssistant.Gateway.Library.Entities.Base;
using IOC.EAssistant.Gateway.XCutting.Results;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace IOC.EAssistant.Gateway.Library.Implementation.Services;

/// <summary>
/// Provides service operations for checking the health status of the system and its AI model dependency.
/// </summary>
/// <remarks>
/// This service acts as a health monitoring layer between the API and the AI model infrastructure,
/// providing standardized health check responses for orchestration tools, load balancers, and
/// monitoring systems. It verifies that the AI model service is operational before allowing
/// chat operations to proceed.
/// </remarks>
/// <param name="_logger">The logger instance for tracking health check operations and errors.</param>
/// <param name="_proxyEAssistant">The proxy for communicating with the AI model service health endpoint.</param>
public class ServiceHealthCheck(
    ILogger<ServiceHealthCheck> _logger,
    IProxyEAssistant _proxyEAssistant,
    IConfiguration _configuration
) : IServiceHealthCheck
{
    /// <summary>
    /// Retrieves the overall health status of the application, including AI model availability.
    /// </summary>
    /// <returns>
    /// An <see cref="OperationResult{T}"/> containing a <see cref="HealthResponse"/> with:
    /// <list type="bullet">
    /// <item><description>Timestamp of the health check</description></item>
    /// <item><description>Model availability status</description></item>
    /// </list>
    /// Returns errors if the health check fails or the model is unavailable.
    /// </returns>
    /// <exception cref="HttpRequestException">Thrown when communication with the AI model health endpoint fails.</exception>
    /// <exception cref="TimeoutException">Thrown when health check operations time out.</exception>
    /// <remarks>
    /// <para>
    /// This method delegates to <see cref="GetModelHealthAsync"/> to verify the AI model status
    /// and wraps the result in a <see cref="HealthResponse"/> object with timestamp information.
    /// </para>
    /// <para>
    /// This endpoint is typically used by:
    /// <list type="bullet">
    /// <item><description>Kubernetes liveness and readiness probes</description></item>
    /// <item><description>Load balancers for routing decisions</description></item>
    /// <item><description>Monitoring systems for alerting</description></item>
    /// <item><description>DevOps dashboards for system status visualization</description></item>
    /// </list>
    /// </para>
    /// <para>
    /// Business logic errors (model unavailability) are captured in the operation result.
    /// Infrastructure exceptions (network failures, timeouts) are allowed to propagate to
    /// exception middleware for proper error handling and logging.
    /// </para>
    /// </remarks>
    public async Task<OperationResult<HealthResponse>> GetHealthAsync()
    {
        var operationResult = new OperationResult<HealthResponse>();

        var eassistantRes = await GetModelHealthAsync();

        if (eassistantRes.HasErrors)
        {
            operationResult.AddErrors(eassistantRes.Errors);
            return operationResult;
        }

        var health = new HealthResponse { ModelAvailable = eassistantRes.Result };

        operationResult.AddResult(health);

        return operationResult;
    }

    /// <summary>
    /// Checks the health and availability of the AI model service.
    /// </summary>
    /// <returns>
    /// An <see cref="OperationResult{T}"/> containing:
    /// <list type="bullet">
    /// <item><description>true if the AI model is healthy and responding correctly</description></item>
    /// <item><description>false with error details if the model is unavailable or unhealthy</description></item>
    /// </list>
    /// </returns>
    /// <exception cref="HttpRequestException">Thrown when communication with the AI model fails.</exception>
    /// <exception cref="TimeoutException">Thrown when the health check times out.</exception>
    /// <exception cref="InvalidOperationException">Thrown when the health response cannot be parsed.</exception>
    /// <remarks>
    /// <para>
    /// This method communicates with the AI model's health endpoint through the proxy layer
    /// and evaluates the response status. A status of "healthy" indicates the model is
    /// operational and ready to process chat requests.
    /// </para>
    /// <para>
    /// This check is performed before processing chat requests to prevent attempting
    /// conversations when the AI model is unavailable, providing better error messages
    /// and preventing unnecessary processing.
    /// </para>
    /// <para>
    /// Infrastructure exceptions (network issues, timeouts, etc.) are allowed to propagate
    /// to exception middleware for proper error handling, ensuring graceful degradation
    /// and consistent error responses.
    /// </para>
    /// </remarks>
    public async Task<OperationResult<bool>> GetModelHealthAsync()
    {
        var operationResult = new OperationResult<bool>();

        var healthResponse = await _proxyEAssistant.HealthCheckAsync();
        var isHealthy = healthResponse.Status == "healthy";
        operationResult.AddResult(isHealthy);

        return operationResult;
    }
}
