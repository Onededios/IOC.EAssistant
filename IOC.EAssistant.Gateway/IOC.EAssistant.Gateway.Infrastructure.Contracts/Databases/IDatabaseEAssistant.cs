using IOC.EAssistant.Gateway.Library.Entities.Databases.EAssistant;

namespace IOC.EAssistant.Gateway.Infrastructure.Contracts.Databases;
public interface IDatabaseEAssistant
{
    Task<int> SaveSessionAsync(Session session);
    Task<Session> GetSessionAsync(Guid id);
    Task<int> DeleteSessionAsync(Guid id);

    Task<int> SaveConversationAsync(Conversation conversation);
    Task<Conversation> GetConversationAsync(Guid id);
    Task<IEnumerable<Conversation>> GetConversationsAsync(Guid id);

    Task<int> SaveQuestionAsync(Question question);
    Task<Question> GetQuestionAsync(Guid id);

    Task<IEnumerable<Answer>> GetAnswersAsync(Guid id);
    Task<Answer> GetAnswerAsync(Guid id);
    Task<int> SaveAnswerAsync(Answer answer);
}
