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

    public override async Task<int> SaveAsync(Question item)
    {

        var builder = SimpleBuilder.CreateFluent()
            .InsertInto($"questions")
            .Columns($"id, created_at, question, token_count, metadata, conversation_id")
            .Values($"{item.Id}, {item.CreatedAt}, {item.Content}, {item.TokenCount}, CAST({item.Metadata} AS jsonb), {item.IdConversation}");

        return await PersistAsync(builder);
    }

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
