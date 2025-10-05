using Dapper;
using Npgsql;

namespace IOC.E_Assistant.Infraestructure.Implementation.Databases;
public abstract class DapperContext
{
    private readonly string connectionString;
    protected DapperContext(string connstr)
    {
        if (string.IsNullOrEmpty(connstr))
            throw new ArgumentNullException(nameof(connstr), "Connection string cannot be null or empty.");
        connectionString = connstr;
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
