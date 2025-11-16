using Dapper.SimpleSqlBuilder;
using IOC.EAssistant.Gateway.Infrastructure.Contracts.Databases;
using IOC.EAssistant.Gateway.Library.Entities.Databases.EAssistant;

namespace IOC.EAssistant.Gateway.Infrastructure.Implementation.Databases.EAssistant;
/// <summary>
/// Database access layer for managing EAssistant conversations.
/// </summary>
/// <param name="connectionString"></param>
public class DatabaseEAssistantConversation(string? connectionString) : DatabaseEAssistantBase<Conversation>(connectionString), IDatabaseEAssistantConversation
{
    /// <summary>
    /// Deletes a conversation record with the specified identifier.
    /// </summary>
    /// <remarks>This method constructs a delete query for the "conversations" table and executes it
    /// asynchronously. Ensure the specified <paramref name="id"/> corresponds to an existing record.</remarks>
    /// <param name="id">The unique identifier of the conversation to delete.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the number of records deleted.</returns>
    public override async Task<int> DeleteAsync(Guid id)
    {
        var builder = SimpleBuilder.CreateFluent()
            .DeleteFrom($"conversations")
            .Where($"id = {id}");
        return await PersistAsync(builder);
    }

    /// <summary>
    /// Asynchronously retrieves a conversation by its unique identifier, including its associated questions and
    /// answers.
    /// </summary>
    /// <remarks>The method retrieves a conversation along with its related questions and answers from the
    /// data source. The questions and answers are grouped and associated with the conversation based on their
    /// relationships.</remarks>
    /// <param name="id">The unique identifier of the conversation to retrieve.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the <see cref="Conversation"/>
    /// object if found; otherwise, <see langword="null"/>.</returns>
    public override async Task<Conversation?> GetAsync(Guid id)
    {
        var builder = SimpleBuilder.CreateFluent()
            .Select($"""
                    c.id as Id, c.created_at as CreatedAt, c.title as Title, c.session_id as IdSession,
                    q.id as Id, q.index as Index, q.created_at as CreatedAt, q.question as Content, q.token_count as TokenCount, q.metadata as Metadata, q.conversation_id as IdConversation,
                    a.id as Id, a.created_at as CreatedAt, a.answer as Content, a.token_count as TokenCount, a.metadata as Metadata, a.question_id as IdQuestion
                    """)
            .From($"conversations c")
            .InnerJoin($"questions q ON c.id = q.conversation_id")
            .InnerJoin($"answers a ON q.id = a.question_id")
            .Where($"c.id = {id}")
            .OrderBy($"q.index");

        var results = await GetAllAsync(builder, Map);

        var conversation = results?
            .GroupBy(c => c.Id)
            .Select(g =>
            {
                var first = g.First();
                first.Questions = g
                    .SelectMany(c => c.Questions ?? [])
                    .DistinctBy(q => q.Id)
                    .ToList();
                return first;
            }).FirstOrDefault();

        return conversation;
    }

    /// <summary>
    /// Asynchronously saves the specified conversation to the database.
    /// </summary>
    /// <remarks>This method constructs a database query to insert the conversation's details into the
    /// "conversations" table. Ensure that the <paramref name="item"/> contains valid data for all required fields
    /// before calling this method.</remarks>
    /// <param name="item">The <see cref="Conversation"/> object to be saved. Must not be <c>null</c>.</param>
    /// <returns>A task that represents the asynchronous save operation. The task result contains the number of rows affected by
    /// the operation.</returns>
    public override async Task<int> SaveAsync(Conversation item)
    {
        var builder = SimpleBuilder.CreateFluent()
            .InsertInto($"conversations")
            .Columns($"id, created_at, title, session_id")
            .Values($"{item.Id}, {item.CreatedAt}, {item.Title}, {item.IdSession}");

        return await PersistAsync(builder);
    }

    /// <summary>
    /// Saves multiple conversation records to the database asynchronously.
    /// </summary>
    /// <remarks>This method constructs and executes a batch insert operation for the provided conversation
    /// records.  Ensure that the <paramref name="items"/> collection is not null or empty to avoid unnecessary database
    /// operations.</remarks>
    /// <param name="items">A collection of <see cref="Conversation"/> objects to be saved. Each object represents a conversation record.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the number of records successfully
    /// saved.</returns>
    public override async Task<int> SaveMultipleAsync(IEnumerable<Conversation> items)
    {
        var builder = SimpleBuilder.CreateFluent()
            .InsertInto($"conversations")
            .Columns($"id, created_at, title, session_id");

        foreach (var item in items)
        {
            builder.Values($"{item.Id}, {item.CreatedAt}, {item.Title}, {item.IdSession}");
        }

        return await PersistAsync(builder);
    }

    private static Func<Conversation, Question, Answer, Conversation> Map => (conversation, question, answer) =>
    {
        question.Answer = answer;
        conversation.Questions ??= new List<Question>();
        conversation.Questions.Add(question);
        return conversation;
    };
}
