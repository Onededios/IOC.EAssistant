using IOC.EAssistant.Gateway.Library.Contracts.Services;
using IOC.EAssistant.Gateway.XCutting.Results;

namespace IOC.EAssistant.Gateway.Library.Implementation.Services;
public abstract class ServiceBase<T> : IServiceBase<T>
{
    public Task<OperationResult<T>> GetAsync(Guid id)
    {
        throw new NotImplementedException();
    }

    public Task<OperationResult<IEnumerable<T>>> GetAllAsync()
    {
        throw new NotImplementedException();
    }

    public Task<OperationResult<bool>> SaveAsync(T entity)
    {
        throw new NotImplementedException();
    }

    public Task<OperationResult<bool>> DeleteAsync(Guid id)
    {
        throw new NotImplementedException();
    }
}
