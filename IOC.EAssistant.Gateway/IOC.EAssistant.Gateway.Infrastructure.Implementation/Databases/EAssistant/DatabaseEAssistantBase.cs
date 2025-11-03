using IOC.EAssistant.Gateway.Infrastructure.Contracts.Databases;

namespace IOC.EAssistant.Gateway.Infrastructure.Implementation.Databases.EAssistant;
public abstract class DatabaseEAssistantBase<T>(string? connectionString) : DatabaseContext<T>(connectionString), IDatabaseEAssistantBase<T>
{
    public abstract Task<T> GetAsync(Guid id);
    public abstract Task<int> SaveAsync(T item);
    public abstract Task<int> DeleteAsync(Guid id);
}
