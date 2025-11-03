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
            .Select($"*")
            .From($"conversations c")
            .InnerJoin($"questions q ON c.id = q.conversation_id")
            .InnerJoin($"answers a ON q.id = a.question_id")
            .Where($"c.id = {id}");
        return await GetFirstByIdAsync(builder);
    }

    public override async Task<int> SaveAsync(Conversation item)
    {
        var builder = SimpleBuilder.CreateFluent()
            .InsertInto($"conversations")
            .Columns($"id, created_at, title, session_id")
            .Values($"{item.id}, {item.created_at}, {item.title}, {item.session_id}");

        return await PersistAsync(builder);
    }

    public override async Task<int> SaveMultipleAsync(IEnumerable<Conversation> items)
    {
        var builder = SimpleBuilder.CreateFluent()
            .InsertInto($"conversations")
            .Columns($"id, created_at, title, session_id");

        foreach (var item in items)
        {
            builder.Values($"{item.id}, {item.created_at}, {item.title}, {item.session_id}");
        }

        return await PersistAsync(builder);
    }
}
