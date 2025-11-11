using Dapper;
using Dapper.SimpleSqlBuilder.FluentBuilder;
using Npgsql;

namespace IOC.EAssistant.Gateway.Infrastructure.Implementation.Databases;
public abstract class DatabaseContext<T>
{
    private readonly string _connectionString;
    protected DatabaseContext(string? connectionString)
    {
        if (string.IsNullOrEmpty(connectionString))
            throw new ArgumentNullException(nameof(connectionString), "Connection string cannot be null or empty.");
        _connectionString = connectionString;
    }


    // Get by Id
    protected async Task<T?> GetFirstByIdAsync(IFluentSqlBuilder command)
    {
        using (var conn = CreateConnection())
        {
            var res = await conn.QueryFirstOrDefaultAsync<T>(command.Sql, command.Parameters);
            return res;
        }
    }

    protected async Task<T?> GetFirstByIdAsync<T2>(
        IFluentSqlBuilder command,
        Func<T, T2, T> map,
        string splitOn = "Id"
    )
    {
        using (var conn = CreateConnection())
        {
            var res = await conn.QueryAsync<T, T2, T>(command.Sql, map, command.Parameters, splitOn: splitOn);
            return res.FirstOrDefault();
        }
    }

    protected async Task<T?> GetFirstByIdAsync<T2, T3>(
        IFluentSqlBuilder command,
        Func<T, T2, T3, T> map,
        string splitOn = "Id"
    )
    {
        using (var conn = CreateConnection())
        {
            var res = await conn.QueryAsync<T, T2, T3, T>(command.Sql, map, command.Parameters, splitOn: splitOn);
            return res.FirstOrDefault();
        }
    }

    protected async Task<T?> GetFirstByIdAsync<T2, T3, T4>(
        IFluentSqlBuilder command,
        Func<T, T2, T3, T4, T> map,
        string splitOn = "Id"
    )
    {
        using (var conn = CreateConnection())
        {
            var res = await conn.QueryAsync<T, T2, T3, T4, T>(
                command.Sql, map, command.Parameters, splitOn: splitOn);
            return res.FirstOrDefault();
        }
    }

    protected async Task<T?> GetFirstByIdAsync<T2, T3, T4, T5>(
        IFluentSqlBuilder command,
        Func<T, T2, T3, T4, T5, T> map,
        string splitOn = "Id"
    )
    {
        using (var conn = CreateConnection())
        {
            var res = await conn.QueryAsync<T, T2, T3, T4, T5, T>(
                command.Sql, map, command.Parameters, splitOn: splitOn);
            return res.FirstOrDefault();
        }
    }

    protected async Task<T?> GetFirstByIdAsync<T2, T3, T4, T5, T6>(
        IFluentSqlBuilder command,
        Func<T, T2, T3, T4, T5, T6, T> map,
        string splitOn = "Id"
    )
    {
        using (var conn = CreateConnection())
        {
            var res = await conn.QueryAsync<T, T2, T3, T4, T5, T6, T>(
                command.Sql, map, command.Parameters, splitOn: splitOn);
            return res.FirstOrDefault();
        }
    }

    protected async Task<T?> GetFirstByIdAsync<T2, T3, T4, T5, T6, T7>(
        IFluentSqlBuilder command,
        Func<T, T2, T3, T4, T5, T6, T7, T> map,
        string splitOn = "Id"
    )
    {
        using (var conn = CreateConnection())
        {
            var res = await conn.QueryAsync<T, T2, T3, T4, T5, T6, T7, T>(
                command.Sql, map, command.Parameters, splitOn: splitOn);
            return res.FirstOrDefault();
        }
    }

    // Get all
    protected async Task<IEnumerable<T>?> GetAllAsync(IFluentSqlBuilder command)
    {
        using (var conn = CreateConnection())
        {
            var res = await conn.QueryAsync<T>(command.Sql, command.Parameters);
            return res;
        }
    }

    protected async Task<IEnumerable<T>> GetAllAsync<T2, T3>(
        IFluentSqlBuilder command,
        Func<T, T2, T3, T> map,
        string splitOn = "Id"
    )
    {
        using (var conn = CreateConnection())
        {
            var res = await conn.QueryAsync<T, T2, T3, T>(command.Sql, map, command.Parameters, splitOn: splitOn);
            return res;
        }
    }

    protected async Task<IEnumerable<T>> GetAllAsync<T2>(
        IFluentSqlBuilder command,
        Func<T, T2, T> map,
        string splitOn = "Id"
    )
    {
        using (var conn = CreateConnection())
        {
            return await conn.QueryAsync<T, T2, T>(
                command.Sql, map, command.Parameters, splitOn: splitOn);
        }
    }

    protected async Task<IEnumerable<T>> GetAllAsync<T2, T3, T4>(
        IFluentSqlBuilder command,
        Func<T, T2, T3, T4, T> map,
        string splitOn = "Id"
    )
    {
        using (var conn = CreateConnection())
        {
            var res = await conn.QueryAsync<T, T2, T3, T4, T>(command.Sql, map, command.Parameters, splitOn: splitOn);
            return res;
        }
    }

    protected async Task<IEnumerable<T>> GetAllAsync<T2, T3, T4, T5>(
        IFluentSqlBuilder command,
        Func<T, T2, T3, T4, T5, T> map,
        string splitOn = "Id"
    )
    {
        using (var conn = CreateConnection())
        {
            var res = await conn.QueryAsync<T, T2, T3, T4, T5, T>(command.Sql, map, command.Parameters, splitOn: splitOn);
            return res;
        }
    }

    protected async Task<IEnumerable<T>> GetAllAsync<T2, T3, T4, T5, T6>(
        IFluentSqlBuilder command,
        Func<T, T2, T3, T4, T5, T6, T> map,
        string splitOn = "Id"
    )
    {
        using (var conn = CreateConnection())
        {
            var res = await conn.QueryAsync<T, T2, T3, T4, T5, T6, T>(command.Sql, map, command.Parameters, splitOn: splitOn);
            return res;
        }
    }

    protected async Task<IEnumerable<T>> GetAllAsync<T2, T3, T4, T5, T6, T7>(
        IFluentSqlBuilder command,
        Func<T, T2, T3, T4, T5, T6, T7, T> map,
        string splitOn = "Id"
    )
    {
        using (var conn = CreateConnection())
        {
            var res = await conn.QueryAsync<T, T2, T3, T4, T5, T6, T7, T>(command.Sql, map, command.Parameters, splitOn: splitOn);
            return res;
        }
    }

    // Persist (Insert/Update/Delete)
    protected async Task<int> PersistAsync(IFluentSqlBuilder command)
    {
        using (var conn = CreateConnection())
        {
            var res = await conn.ExecuteAsync(command.Sql, command.Parameters);
            return res;
        }
    }

    protected NpgsqlConnection CreateConnection() => new NpgsqlConnection(_connectionString);
}
