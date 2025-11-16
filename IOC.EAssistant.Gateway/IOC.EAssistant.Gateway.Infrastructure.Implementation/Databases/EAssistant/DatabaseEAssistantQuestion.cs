using Dapper.SimpleSqlBuilder;
using IOC.EAssistant.Gateway.Infrastructure.Contracts.Databases;
using IOC.EAssistant.Gateway.Library.Entities.Databases.EAssistant;

namespace IOC.EAssistant.Gateway.Infrastructure.Implementation.Databases.EAssistant;
/// <summary>
/// Implements database operations for the <see cref="Question"/> entity.
/// </summary>
/// <param name="connectionString"></param>
public class DatabaseEAssistantQuestion(string? connectionString) : DatabaseEAssistantBase<Question>(connectionString), IDatabaseEAssistantQuestion
{
    /// <summary>
    /// Deletes a question record with the specified identifier.
    /// </summary>
    /// <remarks>This method constructs a delete query for the "questions" table and executes it
    /// asynchronously. Ensure the specified <paramref name="id"/> corresponds to an existing record.</remarks>
    /// <param name="id">The unique identifier of the question to delete.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the number of records deleted.</returns>
    public override async Task<int> DeleteAsync(Guid id)
    {
        var builder = SimpleBuilder.CreateFluent()
            .DeleteFrom($"questions")
            .Where($"id = {id}");
        return await PersistAsync(builder);
    }

    /// <summary>
    /// Asynchronously retrieves a question and its associated answers by the specified identifier.
    /// </summary>
    /// <remarks>This method retrieves a question along with its related answers from the data source. The
    /// question and answers are mapped to their respective properties in the returned <see cref="Question"/>
    /// object.</remarks>
    /// <param name="id">The unique identifier of the question to retrieve.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the <see cref="Question"/> object if
    /// found; otherwise, <see langword="null"/>.</returns>
    public override async Task<Question?> GetAsync(Guid id)
    {
        var builder = SimpleBuilder.CreateFluent()
            .Select($"""
                        q.id as Id, q.index as Index, q.created_at as CreatedAt, q.question as Content, q.token_count as TokenCount, q.metadata as Metadata, q.conversation_id as IdConversation,
                        a.id as Id, a.created_at as CreatedAt, a.answer as Content, a.token_count as TokenCount, a.metadata as Metadata, a.question_id as IdQuestion
                    """)
            .From($"questions q")
            .InnerJoin($"answers a ON q.id = a.question_id")
            .Where($"q.id = {id}");
        return await GetFirstByIdAsync(builder, MapQuestionAnswer);
    }

    /// <summary>
    /// Saves the specified question to the database asynchronously.
    /// </summary>
    /// <remarks>This method constructs a database query to insert the question's details into the "questions"
    /// table. Ensure that the <paramref name="item"/> contains valid data, including non-null and properly formatted
    /// fields,  to avoid database errors during execution.</remarks>
    /// <param name="item">The <see cref="Question"/> object to be saved. Must not be <c>null</c>.</param>
    /// <returns>A task that represents the asynchronous save operation. The task result contains the number of rows affected by
    /// the save operation.</returns>
    public override async Task<int> SaveAsync(Question item)
    {

        var builder = SimpleBuilder.CreateFluent()
            .InsertInto($"questions")
            .Columns($"id, created_at, question, token_count, metadata, conversation_id")
            .Values($"{item.Id}, {item.CreatedAt}, {item.Content}, {item.TokenCount}, CAST({item.Metadata} AS jsonb), {item.IdConversation}");

        return await PersistAsync(builder);
    }

    /// <summary>
    /// Saves multiple questions to the database asynchronously.
    /// </summary>
    /// <param name="items">The collection of <see cref="Question"/> objects to be saved. Must not be <c>null</c>.</param>
    /// <returns>A task that represents the asynchronous save operation. The task result contains the number of rows affected by
    /// the save operation.</returns>
    public override async Task<int> SaveMultipleAsync(IEnumerable<Question> items)
    {
        var builder = SimpleBuilder.CreateFluent()
            .InsertInto($"questions")
            .Columns($"id, created_at, index, question, token_count, metadata, conversation_id");

        foreach (var item in items)
        {
            builder.Values($"{item.Id}, {item.CreatedAt}, {item.Index}, {item.Content}, {item.TokenCount}, CAST({item.Metadata} AS jsonb), {item.IdConversation}");
        }

        return await PersistAsync(builder);
    }

    private static Func<Question, Answer, Question> MapQuestionAnswer => (question, answer) =>
    {
        question.Answer = answer;
        return question;
    };
}
