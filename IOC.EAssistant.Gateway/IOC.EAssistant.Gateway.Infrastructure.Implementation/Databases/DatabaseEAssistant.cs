using IOC.EAssistant.Gateway.Infrastructure.Contracts.Databases;
using IOC.EAssistant.Gateway.Library.Entities.Databases.EAssistant;

namespace IOC.EAssistant.Gateway.Infrastructure.Implementation.Databases;
public class DatabaseEAssistant(string connectionString) : DatabaseContext(connectionString), IDatabaseEAssistant
{
    public async Task<Answer> GetAnswerAsync(Guid id)
    {
        var command = "SELECT * FROM Answers WHERE Id = @Id";
        return await GetById<Answer>(command, id);
    }

    public async Task<Question> GetQuestionAsync(Guid id)
    {
        var command = "SELECT * FROM Questions WHERE Id = @Id";
        return await GetById<Question>(command, id);
    }

    public async Task<Session> GetSessionAsync(Guid id)
    {
        var command = "SELECT * FROM Sessions WHERE Id = @Id";
        return await GetById<Session>(command, id);
    }

    public async Task<int> SaveAnswerAsync(Answer answer)
    {
        var command = "INSERT INTO Answers (Id, QuestionId, Text, CreatedAt) VALUES (@Id, @QuestionId, @Text, @CreatedAt)";
        return await SaveAsync(command, answer);
    }

    public async Task<int> SaveQuestionAsync(Question question)
    {
        var command = "INSERT INTO Questions (Id, SessionId, Text, CreatedAt) VALUES (@Id, @SessionId, @Text, @CreatedAt)";
        return await SaveAsync(command, question);
    }

    public async Task<int> SaveSessionAsync(Session session)
    {
        var command = "INSERT INTO Sessions (Id, UserId, StartedAt) VALUES (@Id, @UserId, @StartedAt)";
        return await SaveAsync(command, session);
    }
}
