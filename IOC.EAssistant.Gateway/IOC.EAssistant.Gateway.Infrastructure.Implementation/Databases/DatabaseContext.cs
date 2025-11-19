using Dapper;
using Dapper.SimpleSqlBuilder.FluentBuilder;
using Npgsql;

namespace IOC.EAssistant.Gateway.Infrastructure.Implementation.Databases;

/// <summary>
/// Represents a database context for a specific entity type.
/// </summary>
/// <typeparam name="T"></typeparam>
public abstract class DatabaseContext<T>
{
    private readonly string _connectionString;
    /// <summary>
    /// Initializes a new instance of the <see cref="DatabaseContext{T}"/> class.
    /// </summary>
    /// <param name="connectionString"></param>
    /// <exception cref="ArgumentNullException"></exception>
    protected DatabaseContext(string? connectionString)
    {
        if (string.IsNullOrEmpty(connectionString))
            throw new ArgumentNullException(nameof(connectionString), "Connection string cannot be null or empty.");
        _connectionString = connectionString;
    }

    /// <summary>
    /// Retrieves the first record of type <typeparamref name="T"/> that matches the specified query.
    /// </summary>
    /// <remarks>This method executes the query defined in the <paramref name="command"/> parameter and
    /// returns the first matching result. Ensure that the <paramref name="command"/> contains a valid SQL statement and
    /// any required parameters.</remarks>
    /// <param name="command">An <see cref="IFluentSqlBuilder"/> instance containing the SQL query and its parameters.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the first record of type
    /// <typeparamref name="T"/> that matches the query, or <see langword="null"/> if no matching record is found.</returns>
    protected async Task<T?> GetFirstByIdAsync(IFluentSqlBuilder command)
    {
        using (var conn = CreateConnection())
        {
            var res = await conn.QueryFirstOrDefaultAsync<T>(command.Sql, command.Parameters);
            return res;
        }
    }

    /// <summary>
    /// Retrieves the first record of type <typeparamref name="T"/> by executing the specified SQL command.
    /// </summary>
    /// <remarks>This method uses Dapper's multi-mapping feature to map the result set to objects of type
    /// <typeparamref name="T"/> and <typeparamref name="T2"/>.</remarks>
    /// <typeparam name="T2">The type of the secondary object used in the mapping function.</typeparam>
    /// <param name="command">The SQL command to execute, including the query and parameters.</param>
    /// <param name="map">A function that maps the primary object of type <typeparamref name="T"/> and the secondary object of type
    /// <typeparamref name="T2"/> to a single result of type <typeparamref name="T"/>.</param>
    /// <param name="splitOn">The column name used to split the result set for mapping. Defaults to "Id".</param>
    /// <returns>The first record of type <typeparamref name="T"/> resulting from the query, or <see langword="null"/> if no
    /// records are found.</returns>
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

    /// <summary>
    /// Retrieves the first record of type <typeparamref name="T"/> by executing the specified SQL command with three-way mapping.
    /// </summary>
    /// <remarks>This method uses Dapper's multi-mapping feature to map the result set to objects of type
    /// <typeparamref name="T"/>, <typeparamref name="T2"/>, and <typeparamref name="T3"/>.</remarks>
    /// <typeparam name="T2">The type of the second object used in the mapping function.</typeparam>
    /// <typeparam name="T3">The type of the third object used in the mapping function.</typeparam>
    /// <param name="command">The SQL command to execute, including the query and parameters.</param>
    /// <param name="map">A function that maps the three objects to a single result of type <typeparamref name="T"/>.</param>
    /// <param name="splitOn">The column name used to split the result set for mapping. Defaults to "Id".</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the first record of type
    /// <typeparamref name="T"/> resulting from the query, or <see langword="null"/> if no records are found.</returns>
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

    /// <summary>
    /// Retrieves the first record of type <typeparamref name="T"/> by executing the specified SQL command with four-way mapping.
    /// </summary>
    /// <remarks>This method uses Dapper's multi-mapping feature to map the result set to objects of type
    /// <typeparamref name="T"/>, <typeparamref name="T2"/>, <typeparamref name="T3"/>, and <typeparamref name="T4"/>.</remarks>
    /// <typeparam name="T2">The type of the second object used in the mapping function.</typeparam>
    /// <typeparam name="T3">The type of the third object used in the mapping function.</typeparam>
    /// <typeparam name="T4">The type of the fourth object used in the mapping function.</typeparam>
    /// <param name="command">The SQL command to execute, including the query and parameters.</param>
    /// <param name="map">A function that maps the four objects to a single result of type <typeparamref name="T"/>.</param>
    /// <param name="splitOn">The column name used to split the result set for mapping. Defaults to "Id".</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the first record of type
    /// <typeparamref name="T"/> resulting from the query, or <see langword="null"/> if no records are found.</returns>
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

    /// <summary>
    /// Retrieves the first record of type <typeparamref name="T"/> by executing the specified SQL command with five-way mapping.
    /// </summary>
    /// <remarks>This method uses Dapper's multi-mapping feature to map the result set to objects of type
    /// <typeparamref name="T"/>, <typeparamref name="T2"/>, <typeparamref name="T3"/>, <typeparamref name="T4"/>, and <typeparamref name="T5"/>.</remarks>
    /// <typeparam name="T2">The type of the second object used in the mapping function.</typeparam>
    /// <typeparam name="T3">The type of the third object used in the mapping function.</typeparam>
    /// <typeparam name="T4">The type of the fourth object used in the mapping function.</typeparam>
    /// <typeparam name="T5">The type of the fifth object used in the mapping function.</typeparam>
    /// <param name="command">The SQL command to execute, including the query and parameters.</param>
    /// <param name="map">A function that maps the five objects to a single result of type <typeparamref name="T"/>.</param>
    /// <param name="splitOn">The column name used to split the result set for mapping. Defaults to "Id".</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the first record of type
    /// <typeparamref name="T"/> resulting from the query, or <see langword="null"/> if no records are found.</returns>
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

    /// <summary>
    /// Retrieves the first record of type <typeparamref name="T"/> by executing the specified SQL command with six-way mapping.
    /// </summary>
    /// <remarks>This method uses Dapper's multi-mapping feature to map the result set to objects of type
    /// <typeparamref name="T"/>, <typeparamref name="T2"/>, <typeparamref name="T3"/>, <typeparamref name="T4"/>, 
    /// <typeparamref name="T5"/>, and <typeparamref name="T6"/>.</remarks>
    /// <typeparam name="T2">The type of the second object used in the mapping function.</typeparam>
    /// <typeparam name="T3">The type of the third object used in the mapping function.</typeparam>
    /// <typeparam name="T4">The type of the fourth object used in the mapping function.</typeparam>
    /// <typeparam name="T5">The type of the fifth object used in the mapping function.</typeparam>
    /// <typeparam name="T6">The type of the sixth object used in the mapping function.</typeparam>
    /// <param name="command">The SQL command to execute, including the query and parameters.</param>
    /// <param name="map">A function that maps the six objects to a single result of type <typeparamref name="T"/>.</param>
    /// <param name="splitOn">The column name used to split the result set for mapping. Defaults to "Id".</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the first record of type
    /// <typeparamref name="T"/> resulting from the query, or <see langword="null"/> if no records are found.</returns>
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

    /// <summary>
    /// Retrieves the first record of type <typeparamref name="T"/> by executing the specified SQL command with seven-way mapping.
    /// </summary>
    /// <remarks>This method uses Dapper's multi-mapping feature to map the result set to objects of type
    /// <typeparamref name="T"/>, <typeparamref name="T2"/>, <typeparamref name="T3"/>, <typeparamref name="T4"/>, 
    /// <typeparamref name="T5"/>, <typeparamref name="T6"/>, and <typeparamref name="T7"/>.</remarks>
    /// <typeparam name="T2">The type of the second object used in the mapping function.</typeparam>
    /// <typeparam name="T3">The type of the third object used in the mapping function.</typeparam>
    /// <typeparam name="T4">The type of the fourth object used in the mapping function.</typeparam>
    /// <typeparam name="T5">The type of the fifth object used in the mapping function.</typeparam>
    /// <typeparam name="T6">The type of the sixth object used in the mapping function.</typeparam>
    /// <typeparam name="T7">The type of the seventh object used in the mapping function.</typeparam>
    /// <param name="command">The SQL command to execute, including the query and parameters.</param>
    /// <param name="map">A function that maps the seven objects to a single result of type <typeparamref name="T"/>.</param>
    /// <param name="splitOn">The column name used to split the result set for mapping. Defaults to "Id".</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the first record of type
    /// <typeparamref name="T"/> resulting from the query, or <see langword="null"/> if no records are found.</returns>
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
    /// <summary>
    /// Retrieves all records of type <typeparamref name="T"/> that match the specified query.
    /// </summary>
    /// <param name="command">An <see cref="IFluentSqlBuilder"/> instance containing the SQL query and its parameters.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains an enumerable collection of
    /// records of type <typeparamref name="T"/> that match the query, or <see langword="null"/> if no records are found.</returns>
    protected async Task<IEnumerable<T>?> GetAllAsync(IFluentSqlBuilder command)
    {
        using (var conn = CreateConnection())
        {
            var res = await conn.QueryAsync<T>(command.Sql, command.Parameters);
            return res;
        }
    }

    /// <summary>
    /// Retrieves all records of type <typeparamref name="T"/> by executing the specified SQL command with three-way mapping.
    /// </summary>
    /// <remarks>This method uses Dapper's multi-mapping feature to map the result set to objects of type
    /// <typeparamref name="T"/>, <typeparamref name="T2"/>, and <typeparamref name="T3"/>.</remarks>
    /// <typeparam name="T2">The type of the second object used in the mapping function.</typeparam>
    /// <typeparam name="T3">The type of the third object used in the mapping function.</typeparam>
    /// <param name="command">The SQL command to execute, including the query and parameters.</param>
    /// <param name="map">A function that maps the three objects to a single result of type <typeparamref name="T"/>.</param>
    /// <param name="splitOn">The column name used to split the result set for mapping. Defaults to "Id".</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains an enumerable collection of
    /// records of type <typeparamref name="T"/> that match the query.</returns>
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

    /// <summary>
    /// Retrieves all records of type <typeparamref name="T"/> by executing the specified SQL command with two-way mapping.
    /// </summary>
    /// <remarks>This method uses Dapper's multi-mapping feature to map the result set to objects of type
    /// <typeparamref name="T"/> and <typeparamref name="T2"/>.</remarks>
    /// <typeparam name="T2">The type of the secondary object used in the mapping function.</typeparam>
    /// <param name="command">The SQL command to execute, including the query and parameters.</param>
    /// <param name="map">A function that maps the primary object of type <typeparamref name="T"/> and the secondary object of type
    /// <typeparamref name="T2"/> to a single result of type <typeparamref name="T"/>.</param>
    /// <param name="splitOn">The column name used to split the result set for mapping. Defaults to "Id".</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains an enumerable collection of
    /// records of type <typeparamref name="T"/> that match the query.</returns>
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

    /// <summary>
    /// Retrieves all records of type <typeparamref name="T"/> by executing the specified SQL command with four-way mapping.
    /// </summary>
    /// <remarks>This method uses Dapper's multi-mapping feature to map the result set to objects of type
    /// <typeparamref name="T"/>, <typeparamref name="T2"/>, <typeparamref name="T3"/>, and <typeparamref name="T4"/>.</remarks>
    /// <typeparam name="T2">The type of the second object used in the mapping function.</typeparam>
    /// <typeparam name="T3">The type of the third object used in the mapping function.</typeparam>
    /// <typeparam name="T4">The type of the fourth object used in the mapping function.</typeparam>
    /// <param name="command">The SQL command to execute, including the query and parameters.</param>
    /// <param name="map">A function that maps the four objects to a single result of type <typeparamref name="T"/>.</param>
    /// <param name="splitOn">The column name used to split the result set for mapping. Defaults to "Id".</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains an enumerable collection of
    /// records of type <typeparamref name="T"/> that match the query.</returns>
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

    /// <summary>
    /// Retrieves all records of type <typeparamref name="T"/> by executing the specified SQL command with five-way mapping.
    /// </summary>
    /// <remarks>This method uses Dapper's multi-mapping feature to map the result set to objects of type
    /// <typeparamref name="T"/>, <typeparamref name="T2"/>, <typeparamref name="T3"/>, <typeparamref name="T4"/>, and <typeparamref name="T5"/>.</remarks>
    /// <typeparam name="T2">The type of the second object used in the mapping function.</typeparam>
    /// <typeparam name="T3">The type of the third object used in the mapping function.</typeparam>
    /// <typeparam name="T4">The type of the fourth object used in the mapping function.</typeparam>
    /// <typeparam name="T5">The type of the fifth object used in the mapping function.</typeparam>
    /// <param name="command">The SQL command to execute, including the query and parameters.</param>
    /// <param name="map">A function that maps the five objects to a single result of type <typeparamref name="T"/>.</param>
    /// <param name="splitOn">The column name used to split the result set for mapping. Defaults to "Id".</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains an enumerable collection of
    /// records of type <typeparamref name="T"/> that match the query.</returns>
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

    /// <summary>
    /// Retrieves all records of type <typeparamref name="T"/> by executing the specified SQL command with six-way mapping.
    /// </summary>
    /// <remarks>This method uses Dapper's multi-mapping feature to map the result set to objects of type
    /// <typeparamref name="T"/>, <typeparamref name="T2"/>, <typeparamref name="T3"/>, <typeparamref name="T4"/>, 
    /// <typeparamref name="T5"/>, and <typeparamref name="T6"/>.</remarks>
    /// <typeparam name="T2">The type of the second object used in the mapping function.</typeparam>
    /// <typeparam name="T3">The type of the third object used in the mapping function.</typeparam>
    /// <typeparam name="T4">The type of the fourth object used in the mapping function.</typeparam>
    /// <typeparam name="T5">The type of the fifth object used in the mapping function.</typeparam>
    /// <typeparam name="T6">The type of the sixth object used in the mapping function.</typeparam>
    /// <param name="command">The SQL command to execute, including the query and parameters.</param>
    /// <param name="map">A function that maps the six objects to a single result of type <typeparamref name="T"/>.</param>
    /// <param name="splitOn">The column name used to split the result set for mapping. Defaults to "Id".</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains an enumerable collection of
    /// records of type <typeparamref name="T"/> that match the query.</returns>
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

    /// <summary>
    /// Retrieves all records of type <typeparamref name="T"/> by executing the specified SQL command with seven-way mapping.
    /// </summary>
    /// <remarks>This method uses Dapper's multi-mapping feature to map the result set to objects of type
    /// <typeparamref name="T"/>, <typeparamref name="T2"/>, <typeparamref name="T3"/>, <typeparamref name="T4"/>, 
    /// <typeparamref name="T5"/>, <typeparamref name="T6"/>, and <typeparamref name="T7"/>.</remarks>
    /// <typeparam name="T2">The type of the second object used in the mapping function.</typeparam>
    /// <typeparam name="T3">The type of the third object used in the mapping function.</typeparam>
    /// <typeparam name="T4">The type of the fourth object used in the mapping function.</typeparam>
    /// <typeparam name="T5">The type of the fifth object used in the mapping function.</typeparam>
    /// <typeparam name="T6">The type of the sixth object used in the mapping function.</typeparam>
    /// <typeparam name="T7">The type of the seventh object used in the mapping function.</typeparam>
    /// <param name="command">The SQL command to execute, including the query and parameters.</param>
    /// <param name="map">A function that maps the seven objects to a single result of type <typeparamref name="T"/>.</param>
    /// <param name="splitOn">The column name used to split the result set for mapping. Defaults to "Id".</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains an enumerable collection of
    /// records of type <typeparamref name="T"/> that match the query.</returns>
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
    /// <summary>
    /// Executes a data persistence command (Insert, Update, or Delete) against the database.
    /// </summary>
    /// <param name="command">An <see cref="IFluentSqlBuilder"/> instance containing the SQL command and its parameters.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the number of rows affected by the command.</returns>
    protected async Task<int> PersistAsync(IFluentSqlBuilder command)
    {
        using (var conn = CreateConnection())
        {
            var res = await conn.ExecuteAsync(command.Sql, command.Parameters);
            return res;
        }
    }

    /// <summary>
    /// Creates a new PostgreSQL database connection using the configured connection string.
    /// </summary>
    /// <returns>A new instance of <see cref="NpgsqlConnection"/>.</returns>
    protected NpgsqlConnection CreateConnection() => new NpgsqlConnection(_connectionString);
}
