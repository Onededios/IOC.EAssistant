using Dapper;
using Npgsql;

namespace IOC.EAssistant.Gateway.Infrastructure.Implementation.Databases;
public abstract class DatabaseContext
{
    private readonly string connectionString;
    protected DatabaseContext(string connectionString)
    {
        if (string.IsNullOrEmpty(connectionString))
            throw new ArgumentNullException(nameof(connectionString), "Connection string cannot be null or empty.");
        this.connectionString = connectionString;
    }

    protected async Task<T> GetById<T>(string command, Guid id)
    {
        using (var conn = CreateConnection())
        {
            var res = await conn.QueryFirstOrDefaultAsync<T>(command, new { id });
            if (res == null) throw new KeyNotFoundException($"{typeof(T).Name} with id {id} not found.");
            return res;
        }
    }

    protected async Task<int> SaveAsync<T>(string command, T item)
    {
        using (var conn = CreateConnection())
        {
            return await conn.ExecuteAsync(command, item);
        }
    }

    protected NpgsqlConnection CreateConnection() => new NpgsqlConnection(connectionString);
}
