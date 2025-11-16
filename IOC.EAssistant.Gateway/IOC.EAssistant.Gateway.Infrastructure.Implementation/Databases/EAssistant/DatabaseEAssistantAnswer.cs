using Dapper.SimpleSqlBuilder;
using IOC.EAssistant.Gateway.Infrastructure.Contracts.Databases;
using IOC.EAssistant.Gateway.Library.Entities.Databases.EAssistant;

namespace IOC.EAssistant.Gateway.Infrastructure.Implementation.Databases.EAssistant;
/// <summary>
/// Implements database operations for the <see cref="Answer"/> entity.
/// </summary>
/// <param name="connectionString"></param>
public class DatabaseEAssistantAnswer(string? connectionString) : DatabaseEAssistantBase<Answer>(connectionString), IDatabaseEAssistantAnswer
{
    /// <summary>
    /// Deletes an answer record with the specified identifier.
    /// </summary>
    /// <remarks>This method constructs a delete query for the "answers" table and executes it
    /// asynchronously. Ensure the specified <paramref name="id"/> corresponds to an existing record.</remarks>
    /// <param name="id">The unique identifier of the answer to delete.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the number of records deleted.</returns>
    public override async Task<int> DeleteAsync(Guid id)
    {
        var builder = SimpleBuilder.CreateFluent()
            .DeleteFrom($"answers")
            .Where($"id = {id}");
        return await PersistAsync(builder);
    }

    /// <summary>
    /// Retrieves an answer asynchronously based on the specified unique identifier.
    /// </summary>
    /// <remarks>This method queries the data source for an answer matching the provided identifier. If no
    /// matching answer is found, the method returns <see langword="null"/>.</remarks>
    /// <param name="id">The unique identifier of the answer to retrieve.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the <see cref="Answer"/> object if
    /// found; otherwise, <see langword="null"/>.</returns>
    public override async Task<Answer?> GetAsync(Guid id)
    {
        var builder = SimpleBuilder.CreateFluent()
            .Select($"id as Id, created_at as CreatedAt, question_id as IdQuestion, answer as Content, token_count as TokenCount, metadata as Metadata, sources as Sources")
            .From($"answers")
            .Where($"id = {id}");
        return await GetFirstByIdAsync(builder);
    }

    /// <summary>
    /// Saves the specified answer to the database asynchronously.
    /// </summary>
    /// <remarks>This method constructs and executes an SQL insert statement to save the provided <see
    /// cref="Answer"/> object. Ensure that the <paramref name="item"/> parameter contains valid values for all required
    /// fields, including <c>Id</c>, <c>CreatedAt</c>, <c>IdQuestion</c>, <c>Content</c>, <c>TokenCount</c>,
    /// <c>Metadata</c>, and <c>Sources</c>.</remarks>
    /// <param name="item">The <see cref="Answer"/> object to be saved. The object must contain valid data for all required fields.</param>
    /// <returns>A task that represents the asynchronous save operation. The task result contains the number of rows affected by
    /// the operation.</returns>
    public override async Task<int> SaveAsync(Answer item)
    {
        var builder = SimpleBuilder.CreateFluent()
            .InsertInto($"answers")
            .Columns($"id, created_at, question_id, answer, token_count, metadata, sources")
            .Values($"{item.Id}, {item.CreatedAt}, {item.IdQuestion}, {item.Content}, {item.TokenCount}, CAST({item.Metadata} AS jsonb), CAST({item.Sources} AS jsonb)");
        return await PersistAsync(builder);
    }

    /// <summary>
    /// Saves multiple <see cref="Answer"/> objects to the database asynchronously.
    /// </summary>
    /// <remarks>This method constructs a bulk insert operation for the provided <see cref="Answer"/> objects
    /// and executes it asynchronously. Ensure that the <paramref name="items"/> collection is not null or empty to
    /// avoid unnecessary database operations.</remarks>
    /// <param name="items">A collection of <see cref="Answer"/> objects to be saved. Each object represents an answer with associated
    /// metadata.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the number of records successfully
    /// saved to the database.</returns>
    public override async Task<int> SaveMultipleAsync(IEnumerable<Answer> items)
    {
        var builder = SimpleBuilder.CreateFluent()
            .InsertInto($"answers")
            .Columns($"id, created_at, question_id, answer, token_count, metadata, sources");
        foreach (var item in items)
        {
            builder.Values($"{item.Id}, {item.CreatedAt}, {item.IdQuestion}, {item.Content}, {item.TokenCount}, CAST({item.Metadata} AS jsonb), CAST({item.Sources} AS jsonb)");
        }

        return await PersistAsync(builder);
    }
}
