using IOC.EAssistant.Gateway.Infrastructure.Contracts.Proxies;
using IOC.EAssistant.Gateway.Library.Contracts.Services;
using IOC.EAssistant.Gateway.Library.Entities.Base;
using IOC.EAssistant.Gateway.XCutting.Results;
using Microsoft.Extensions.Logging;

namespace IOC.EAssistant.Gateway.Library.Implementation.Services;
public class ServiceHealthCheck(
    ILogger<ServiceHealthCheck> _logger,
    IProxyEAssistant _proxyEAssistant
) : IServiceHealthCheck
{
    public async Task<OperationResult<HealthResponse>> GetHealthAsync()
    {
        var operationResult = new OperationResult<HealthResponse>();

        try
        {
            var eassistantRes = await GetModelHealthAsync();

            if (eassistantRes.HasErrors)
            {
                operationResult.AddErrors(eassistantRes.Errors);
                return operationResult;
            }

            var health = new HealthResponse { ModelAvailable = eassistantRes.Result };

            operationResult.AddResult(health);
        }
        catch (Exception ex)
        {
            operationResult.AddError(new ErrorResult("Error performing health check", "Health"), ex);
            _logger.LogError(ex, "Error performing health check");
        }

        return operationResult;
    }


    public async Task<OperationResult<bool>> GetModelHealthAsync()
    {
        var operationResult = new OperationResult<bool>();
        try
        {
            var healthResponse = await _proxyEAssistant.HealthCheckAsync();
            var isHealthy = healthResponse.Status == "healthy";
            operationResult.AddResult(isHealthy);
        }
        catch (Exception ex)
        {
            operationResult.AddError(new ErrorResult("Error performing health check", "Model API Health"), ex);
            _logger.LogError(ex, "Error performing health check on EAssistant proxy");
        }
        return operationResult;
    }
}
