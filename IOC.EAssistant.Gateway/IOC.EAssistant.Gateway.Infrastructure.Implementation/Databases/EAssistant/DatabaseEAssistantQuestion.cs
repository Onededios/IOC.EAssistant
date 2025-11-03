using IOC.EAssistant.Gateway.Library.Entities.Databases.EAssistant;
using IOC.EAssistant.Gateway.Infrastructure.Contracts.Databases;
using Dapper.SimpleSqlBuilder;

namespace IOC.EAssistant.Gateway.Infrastructure.Implementation.Databases.EAssistant;
public class DatabaseEAssistantQuestion(string? connectionString) : DatabaseEAssistantBase<Question>(connectionString), IDatabaseEAssistantQuestion
{
    public override async Task<int> DeleteAsync(Guid id)
    {
        var builder = SimpleBuilder.CreateFluent()
            .DeleteFrom($"questions")
            .Where($"id = {id}");
        return await PersistAsync(builder);
    }

    public override async Task<Question> GetAsync(Guid id)
    {
        var builder = SimpleBuilder.CreateFluent()
            .Select($"*")
            .From($"questions q")
            .InnerJoin($"answers a ON q.id = a.question_id")
            .Where($"q.id = {id}");
        return await GetFirstByIdAsync(builder);
    }

    public override async Task<int> SaveAsync(Question item)
    {

        var builder = SimpleBuilder.CreateFluent()
            .InsertInto($"questions")
            .Columns($"id, created_at, question, token_count, metadata, conversation_id")
            .Values($"{item.id}, {item.created_at}, {item.question}, {item.token_count}, {item.metadata}, {item.conversation_id}");

        return await PersistAsync(builder);
    }
}
