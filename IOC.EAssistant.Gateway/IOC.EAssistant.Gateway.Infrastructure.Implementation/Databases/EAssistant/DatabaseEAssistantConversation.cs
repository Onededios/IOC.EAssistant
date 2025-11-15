using IOC.EAssistant.Gateway.Library.Entities.Databases.EAssistant;
using IOC.EAssistant.Gateway.Infrastructure.Contracts.Databases;
using Dapper.SimpleSqlBuilder;

namespace IOC.EAssistant.Gateway.Infrastructure.Implementation.Databases.EAssistant;
public class DatabaseEAssistantConversation(string? connectionString) : DatabaseEAssistantBase<Conversation>(connectionString), IDatabaseEAssistantConversation
{
    public override async Task<int> DeleteAsync(Guid id)
    {
        var builder = SimpleBuilder.CreateFluent()
            .DeleteFrom($"conversations")
            .Where($"id = {id}");
        return await PersistAsync(builder);
    }

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
            .Where($"c.id = {id}");
        return await GetFirstByIdAsync(builder, Map);
    }

    public override async Task<int> SaveAsync(Conversation item)
    {
        var builder = SimpleBuilder.CreateFluent()
            .InsertInto($"conversations")
            .Columns($"id, created_at, title, session_id")
            .Values($"{item.Id}, {item.CreatedAt}, {item.Title}, {item.IdSession}");

        return await PersistAsync(builder);
    }

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
