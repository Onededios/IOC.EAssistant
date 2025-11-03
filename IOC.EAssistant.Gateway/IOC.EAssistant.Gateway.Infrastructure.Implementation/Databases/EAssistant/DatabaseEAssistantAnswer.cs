using IOC.EAssistant.Gateway.Library.Entities.Databases.EAssistant;
using IOC.EAssistant.Gateway.Infrastructure.Contracts.Databases;
using Dapper.SimpleSqlBuilder;

namespace IOC.EAssistant.Gateway.Infrastructure.Implementation.Databases.EAssistant;
public class DatabaseEAssistantAnswer(string? connectionString) : DatabaseEAssistantBase<Answer>(connectionString), IDatabaseEAssistantAnswer
{
    public override async Task<int> DeleteAsync(Guid id)
    {
        var builder = SimpleBuilder.CreateFluent()
            .DeleteFrom($"answers")
            .Where($"id = {id}");
        return await PersistAsync(builder);
    }

    public override async Task<Answer> GetAsync(Guid id)
    {
        var builder = SimpleBuilder.CreateFluent()
            .Select($"*")
            .From($"answers")
            .Where($"id = {id}");
        return await GetFirstByIdAsync(builder);
    }

    public override async Task<int> SaveAsync(Answer item)
    {
        var builder = SimpleBuilder.CreateFluent()
            .InsertInto($"answers")
            .Columns($"id, created_at, question_id, answer, token_count, metadata, sources")
            .Values($"{item.id}, {item.created_at}, {item.question_id}, {item.answer}, {item.token_count}, {item.metadata}, {item.sources}");
        return await PersistAsync(builder);
    }
}
