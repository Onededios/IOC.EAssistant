namespace IOC.EAssistant.Gateway.Infrastructure.Contracts.Databases;
public interface IDatabaseEAssistantBase<T>
{
    Task<T?> GetAsync(Guid id);
    Task<int> SaveAsync(T item);
    Task<int> DeleteAsync(Guid id);
    Task<int> SaveMultipleAsync(IEnumerable<T> items);
}
