using IOC.EAssistant.Gateway.XCutting.Results;

namespace IOC.EAssistant.Gateway.Library.Contracts.Services;
public interface IServiceBase<T>
{
    public Task<OperationResult<T?>> GetAsync(Guid id);
    public Task<OperationResult<bool>> SaveAsync(T entity);
    public Task<OperationResult<bool>> DeleteAsync(Guid id);
    public Task<OperationResult<bool>> SaveMultipleAsync(IEnumerable<T> entities);
}
