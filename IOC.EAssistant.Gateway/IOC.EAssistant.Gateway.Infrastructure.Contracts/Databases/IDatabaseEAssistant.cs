using IOC.EAssistant.Gateway.Library.Entities.Databases.EAssistant;

namespace IOC.EAssistant.Gateway.Infrastructure.Contracts.Databases;
public interface IDatabaseEAssistant
{
    Task<Answer> GetAnswerAsync(Guid id);
    Task<Question> GetQuestionAsync(Guid id);
    Task<Session> GetSessionAsync(Guid id);
    Task<int> SaveAnswerAsync(Answer answer);
    Task<int> SaveQuestionAsync(Question question);
    Task<int> SaveSessionAsync(Session session);
}
