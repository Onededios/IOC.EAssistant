using Dapper;
using Dapper.SimpleSqlBuilder.FluentBuilder;
using Npgsql;

namespace IOC.EAssistant.Gateway.Infrastructure.Implementation.Databases;
public abstract class DatabaseContext<T>
{
    private readonly string connectionString;
    protected DatabaseContext(string? connectionString)
    {
        if (string.IsNullOrEmpty(connectionString))
            throw new ArgumentNullException(nameof(connectionString), "Connection string cannot be null or empty.");
        this.connectionString = connectionString;
    }

    protected async Task<T> GetFirstByIdAsync(IFluentSqlBuilder command)
    {
        using (var conn = CreateConnection())
        {
            var res = await conn.QueryFirstAsync<T>(command.Sql, command.Parameters);
            return res;
        }
    }

    protected async Task<IEnumerable<T>> GetAllAsync(IFluentSqlBuilder command)
    {
        using (var conn = CreateConnection())
        {
            var res = await conn.QueryAsync<T>(command.Sql, command.Parameters);
            return res;
        }
    }

    protected async Task<int> PersistAsync(IFluentSqlBuilder command)
    {
        using (var conn = CreateConnection())
        {
            var res = await conn.ExecuteAsync(command.Sql, command.Parameters);
            return res;
        }
    }

    protected NpgsqlConnection CreateConnection() => new NpgsqlConnection(connectionString);
}
