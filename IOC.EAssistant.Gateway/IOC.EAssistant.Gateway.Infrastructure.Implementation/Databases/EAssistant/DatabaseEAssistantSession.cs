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
            .Select($"*")
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
            .Values($"{item.id}, {item.created_at}");

        return await PersistAsync(builder);
    }

    public override async Task<int> SaveMultipleAsync(IEnumerable<Session> items)
    {
        var builder = SimpleBuilder.CreateFluent()
            .InsertInto($"sessions")
            .Columns($"id, created_at");

        foreach (var item in items)
        {
            builder.Values($"{item.id}, {item.created_at}");
        }

        return await PersistAsync(builder);
    }

    private static Func<Session, Conversation, Question, Answer, Session> Map => (session, conversation, question, answer) =>
    {
        question.answer = answer;
        conversation.questions ??= new List<Question>();
        conversation.questions.Add(question);
        session.conversations ??= new List<Conversation>();
        session.conversations.Add(conversation);
        return session;
    };
}
