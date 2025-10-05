using Dapper;
using IOC.E_Assistant.Infraestructure.Contracts.Database;
using IOC.E_Assistant.Infraestructure.Contracts.NeonDB.Entities;
using Microsoft.Extensions.Configuration;

namespace IOC.E_Assistant.Infraestructure.Implementation.Databases;
public class EAssistantDb : DapperContext, IEAssistantDb
{
    public EAssistantDb(IConfiguration configuration) : base(configuration["EASSISTANT_CONNSTR"])
    {
    }

    public async Task<Answer> GetAnswer(Guid id)
    {
        var command = "SELECT * FROM Answers WHERE Id = @Id";
        return await GetById<Answer>(command, id);
    }

    public async Task<Question> GetQuestion(Guid id)
    {
        var command = "SELECT * FROM Questions WHERE Id = @Id";
        return await GetById<Question>(command, id);
    }

    public async Task<Session> GetSession(Guid id)
    {
        var command = "SELECT * FROM Sessions WHERE Id = @Id";
        return await GetById<Session>(command, id);
    }

    public Task<int> SaveAnswer(Answer answer)
    {
        var command = "INSERT INTO Answers (Id, QuestionId, Text, CreatedAt) VALUES (@Id, @QuestionId, @Text, @CreatedAt)";
        return SaveAsync<Answer>(command, answer);
    }

    public Task<int> SaveQuestion(Question question)
    {
        var command = "INSERT INTO Questions (Id, SessionId, Text, CreatedAt) VALUES (@Id, @SessionId, @Text, @CreatedAt)";
        return SaveAsync<Question>(command, question);
    }

    public Task<int> SaveSession(Session session)
    {
        var command = "INSERT INTO Sessions (Id, UserId, StartedAt) VALUES (@Id, @UserId, @StartedAt)";
        return SaveAsync<Session>(command, session);
    }
}