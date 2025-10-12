using IOC.E_Assistant.WebApi.Library.Models.Entities;

namespace IOC.E_Assistant.Infrastructure.Contracts.Database;
public interface IEAssistantDb
{
    Task<Question> GetQuestionAsync(Guid id);
    Task<Answer> GetAnswerAsync(Guid id);
    Task<Session> GetSessionAsync(Guid id);
    Task<int> SaveQuestionAsync(Question question);
    Task<int> SaveAnswerAsync(Answer answer);
    Task<int> SaveSessionAsync(Session session);
}
