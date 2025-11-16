using Dapper.SimpleSqlBuilder;
using IOC.EAssistant.Gateway.Infrastructure.Contracts.Databases;
using IOC.EAssistant.Gateway.Library.Entities.Databases.EAssistant;

namespace IOC.EAssistant.Gateway.Infrastructure.Implementation.Databases.EAssistant;
/// <summary>
/// Represents a session in the E-Assistant database.
/// </summary>
/// <param name="connectionString">The connection string to the database.</param>
public class DatabaseEAssistantSession(string? connectionString) : DatabaseEAssistantBase<Session>(connectionString), IDatabaseEAssistantSession
{
    /// <summary>
    /// Deletes a session record with the specified identifier.
    /// </summary>
    /// <remarks>This method constructs a delete query for the "sessions" table and executes it
    /// asynchronously. Ensure the specified <paramref name="id"/> corresponds to an existing record.</remarks>
    /// <param name="id">The unique identifier of the session to delete.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the number of records deleted.</returns>
    public override async Task<int> DeleteAsync(Guid id)
    {
        var builder = SimpleBuilder.CreateFluent()
            .DeleteFrom($"sessions")
            .Where($"id = {id}");
        return await PersistAsync(builder);
    }

    /// <summary>
    /// Asynchronously retrieves a session by its unique identifier, including its associated conversations, questions,
    /// and answers.
    /// </summary>
    /// <remarks>The retrieved session includes its conversations, each with their respective questions and
    /// answers. The data is grouped and ordered to ensure the integrity of the hierarchical relationships.</remarks>
    /// <param name="id">The unique identifier of the session to retrieve.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the session with its associated
    /// data, or <see langword="null"/> if no session with the specified identifier is found.</returns>
    public override async Task<Session?> GetAsync(Guid id)
    {
        var builder = SimpleBuilder.CreateFluent()
            .Select($"""
                    s.id as Id, s.created_at as CreatedAt,
                    c.id as Id, c.created_at as CreatedAt, c.title as Title, c.session_id as IdSession,
                    q.id as Id, q.index as Index, q.created_at as CreatedAt, q.question as Content, q.token_count as TokenCount, q.metadata as Metadata, q.conversation_id as IdConversation,
                    a.id as Id, a.created_at as CreatedAt, a.answer as Content, a.token_count as TokenCount, a.metadata as Metadata, a.question_id as IdQuestion
                """)
            .From($"sessions s")
            .InnerJoin($"conversations c ON s.id = c.session_id")
            .InnerJoin($"questions q ON c.id = q.conversation_id")
            .InnerJoin($"answers a ON q.id = a.question_id")
            .Where($"s.id = {id}")
            .OrderBy($"c.created_at, q.index");

        var results = await GetAllAsync(builder, Map);

        var session = results?
            .GroupBy(s => s.Id)
            .Select(g =>
            {
                var first = g.First();

                var conversations = g
                    .SelectMany(s => s.Conversations ?? [])
                    .GroupBy(c => c.Id)
                    .Select(cg =>
                    {
                        var firstConv = cg.First();
                        firstConv.Questions = cg.SelectMany(c => c.Questions ?? [])
                            .DistinctBy(q => q.Id)
                            .OrderBy(q => q.Index)
                            .ToList();
                        return firstConv;
                    }).ToList();
                first.Conversations = conversations;
                return first;
            }).FirstOrDefault();

        return session;
    }

    /// <summary>
    /// Saves the specified session to the database asynchronously.
    /// </summary>
    /// <remarks>This method constructs an SQL insert statement using the provided session data and executes
    /// it asynchronously. Ensure that the session object contains valid data before calling this method.</remarks>
    /// <param name="item">The session object to be saved. Must contain valid <see cref="Session.Id"/> and <see cref="Session.CreatedAt"/>
    /// values.</param>
    /// <returns>A task that represents the asynchronous save operation. The task result contains the number of rows affected by
    /// the operation.</returns>
    public override async Task<int> SaveAsync(Session item)
    {

        var builder = SimpleBuilder.CreateFluent()
            .InsertInto($"sessions")
            .Columns($"id, created_at")
            .Values($"{item.Id}, {item.CreatedAt}");

        return await PersistAsync(builder);
    }

    /// <summary>
    /// Saves multiple session records to the database asynchronously.
    /// </summary>
    /// <remarks>This method constructs a batch insert operation for the provided session records and executes
    /// it asynchronously. Ensure that the <paramref name="items"/> collection is not null and contains valid session
    /// objects.</remarks>
    /// <param name="items">A collection of <see cref="Session"/> objects to be saved. Each object represents a session to be inserted into
    /// the database.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the number of records successfully
    /// saved to the database.</returns>
    public override async Task<int> SaveMultipleAsync(IEnumerable<Session> items)
    {
        var builder = SimpleBuilder.CreateFluent()
            .InsertInto($"sessions")
            .Columns($"id, created_at");

        foreach (var item in items)
        {
            builder.Values($"{item.Id}, {item.CreatedAt}");
        }

        return await PersistAsync(builder);
    }

    private static Func<Session, Conversation, Question, Answer, Session> Map => (session, conversation, question, answer) =>
    {
        question.Answer = answer;
        conversation.Questions ??= new List<Question>();
        conversation.Questions.Add(question);
        session.Conversations ??= new List<Conversation>();
        session.Conversations.Add(conversation);
        return session;
    };
}
