using IOC.E_Assistant.Infraestructure.Contracts.NeonDB.Entities;

namespace IOC.E_Assistant.Infraestructure.Contracts.Database;
public interface IEAssistantDb
{
    Task<Question> GetQuestion(Guid id);
    Task<Answer> GetAnswer(Guid id);
    Task<Session> GetSession(Guid id);
    Task<int> SaveQuestion(Question question);
    Task<int> SaveAnswer(Answer answer);
    Task<int> SaveSession(Session session);
}
