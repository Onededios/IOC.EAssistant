using IOC.EAssistant.Gateway.Infrastructure.Contracts.Databases;

namespace IOC.EAssistant.Gateway.Infrastructure.Implementation.Databases.EAssistant;
/// <summary>
/// Base class for database operations.
/// </summary>
/// <typeparam name="T"></typeparam>
/// <param name="connectionString"></param>
public abstract class DatabaseEAssistantBase<T>(string? connectionString) : DatabaseContext<T>(connectionString), IDatabaseEAssistantBase<T>
{
    /// <summary>
    /// Retrieves an entity from the database.
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public abstract Task<T?> GetAsync(Guid id);

    /// <summary>
    /// Saves an entity to the database.
    /// </summary>
    /// <param name="item"></param>
    /// <returns></returns>
    public abstract Task<int> SaveAsync(T item);

    /// <summary>
    /// Saves multiple entities to the database.
    /// </summary>
    /// <param name="items"></param>
    /// <returns></returns>
    public abstract Task<int> SaveMultipleAsync(IEnumerable<T> items);

    /// <summary>
    /// Deletes an entity from the database.
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public abstract Task<int> DeleteAsync(Guid id);
}
