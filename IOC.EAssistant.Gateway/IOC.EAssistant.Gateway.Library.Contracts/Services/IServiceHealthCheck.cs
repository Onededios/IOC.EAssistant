using IOC.EAssistant.Gateway.Library.Entities.Base;
using IOC.EAssistant.Gateway.XCutting.Results;

namespace IOC.EAssistant.Gateway.Library.Contracts.Services;
public interface IServiceHealthCheck
{
    public Task<OperationResult<HealthResponse>> GetHealthAsync();
    public Task<OperationResult<bool>> GetModelHealthAsync();
}
