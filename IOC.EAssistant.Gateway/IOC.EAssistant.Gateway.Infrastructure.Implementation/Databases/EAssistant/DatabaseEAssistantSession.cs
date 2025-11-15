using IOC.EAssistant.Gateway.Library.Entities.Databases.EAssistant;
using IOC.EAssistant.Gateway.Infrastructure.Contracts.Databases;
using Dapper.SimpleSqlBuilder;

namespace IOC.EAssistant.Gateway.Infrastructure.Implementation.Databases.EAssistant;
public class DatabaseEAssistantSession(string? connectionString) : DatabaseEAssistantBase<Session>(connectionString), IDatabaseEAssistantSession
{
    public override async Task<int> DeleteAsync(Guid id)
    {
        var builder = SimpleBuilder.CreateFluent()
            .DeleteFrom($"sessions")
            .Where($"id = {id}");
        return await PersistAsync(builder);
    }

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
            .Where($"s.id = {id}");
        return await GetFirstByIdAsync(builder, Map);
    }

    public override async Task<int> SaveAsync(Session item)
    {

        var builder = SimpleBuilder.CreateFluent()
            .InsertInto($"sessions")
            .Columns($"id, created_at")
            .Values($"{item.Id}, {item.CreatedAt}");

        return await PersistAsync(builder);
    }

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
