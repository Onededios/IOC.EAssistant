using IOC.EAssistant.Gateway.Library.Entities.Databases.EAssistant;
using IOC.EAssistant.Gateway.Infrastructure.Contracts.Databases;
using Dapper.SimpleSqlBuilder;
using Dapper.SimpleSqlBuilder.FluentBuilder;

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

    public override async Task<Answer?> GetAsync(Guid id)
    {
        var builder = SimpleBuilder.CreateFluent()
            .Select($"id as Id, created_at as CreatedAt, question_id as IdQuestion, answer as Content, token_count as TokenCount, metadata as Metadata, sources as Sources")
            .From($"answers")
            .Where($"id = {id}");
        return await GetFirstByIdAsync(builder);
    }

    public override async Task<int> SaveAsync(Answer item)
    {
        var builder = SimpleBuilder.CreateFluent()
            .InsertInto($"answers")
            .Columns($"id, created_at, question_id, answer, token_count, metadata, sources")
            .Values($"{item.Id}, {item.CreatedAt}, {item.IdQuestion}, {item.Content}, {item.TokenCount}, {item.Metadata}, {item.Sources}");
        return await PersistAsync(builder);
    }

    public override async Task<int> SaveMultipleAsync(IEnumerable<Answer> items)
    {
        var builder = SimpleBuilder.CreateFluent()
            .InsertInto($"answers")
            .Columns($"id, created_at, question_id, answer, token_count, metadata, sources");
        foreach (var item in items)
        {
            builder.Values($"{item.Id}, {item.CreatedAt}, {item.IdQuestion}, {item.Content}, {item.TokenCount}, {item.Metadata}, {item.Sources}");
        }

        return await PersistAsync(builder);
    }
}
