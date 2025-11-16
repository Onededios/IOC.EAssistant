using IOC.EAssistant.Gateway.Library.Entities.Base;
using IOC.EAssistant.Gateway.XCutting.Results;

namespace IOC.EAssistant.Gateway.Library.Contracts.Services;

/// <summary>
/// Defines service operations for checking the health status of the system and its dependencies.
/// </summary>
/// <remarks>
/// This service provides health check endpoints to monitor the availability and status
/// of the application and its underlying AI model infrastructure. It's commonly used
/// for monitoring, load balancing, and orchestration purposes.
/// </remarks>
public interface IServiceHealthCheck
{
    /// <summary>
    /// Retrieves the overall health status of the application.
    /// </summary>
    /// <returns>
    /// An <see cref="OperationResult{T}"/> containing a <see cref="HealthResponse"/>
    /// with health status information including timestamp and model availability.
    /// </returns>
    /// <remarks>
    /// This method checks the general health of the application, including its ability
    /// to respond to requests and the availability of critical dependencies.
    /// </remarks>
    public Task<OperationResult<HealthResponse>> GetHealthAsync();

    /// <summary>
    /// Checks the health and availability of the AI model.
    /// </summary>
    /// <returns>
    /// An <see cref="OperationResult{T}"/> containing a boolean value indicating
    /// whether the AI model is available and responding correctly.
    /// </returns>
    /// <remarks>
    /// This method specifically validates that the underlying AI model service is
    /// operational and can process requests. It's useful for determining if chat
    /// functionality is available before attempting to process user requests.
    /// </remarks>
    public Task<OperationResult<bool>> GetModelHealthAsync();
}
