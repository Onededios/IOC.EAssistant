namespace IOC.EAssistant.Gateway.Infrastructure.Contracts.Databases;
/// <summary>
/// Defines methods for interacting with a database entity of type <typeparamref name="T"/>.
/// </summary>
/// <typeparam name="T"></typeparam>
public interface IDatabaseEAssistantBase<T>
{
    Task<T?> GetAsync(Guid id);
    Task<int> SaveAsync(T item);
    Task<int> DeleteAsync(Guid id);
    Task<int> SaveMultipleAsync(IEnumerable<T> items);
}
