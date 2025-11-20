using IOC.EAssistant.Gateway.Library.Entities.Databases.EAssistant;
using System.Text.Json.Nodes;

namespace IOC.EAssistant.Gateway.Library.UnitTests.Helpers;

public static class AnswerTestHelper
{
    public static Answer CreateAnswer(
        Guid answerId,
        Guid questionId,
        string content = "Test answer content",
        int tokenCount = 100
    ) => new()
    {
        Id = answerId,
        IdQuestion = questionId,
        Content = content,
        TokenCount = tokenCount,
        CreatedAt = DateTime.Now
    };

    public static Answer CreateAnswerWithMetadata(
        Guid answerId,
        Guid questionId,
        JsonObject? metadata = null
    )
    {
        var answer = CreateAnswer(answerId, questionId);
        answer.Metadata = metadata ?? new()
        {
            ["model"] = "gpt-4",
            ["temperature"] = 0.7,
            ["timestamp"] = DateTime.Now.ToString("o")
        };

        return answer;
    }

    public static Answer CreateAnswerWithSources(
        Guid answerId,
        Guid questionId,
        JsonObject? sources = null
    )
    {
        var answer = CreateAnswer(answerId, questionId);
        answer.Sources = sources ?? new()
        {
            ["url"] = "https://example.com",
            ["title"] = "Example Source",
            ["relevance"] = 0.95
        };

        return answer;
    }

    public static Answer CreateCompleteAnswer(
        Guid answerId,
        Guid questionId
    )
    {
        var answer = CreateAnswerWithMetadata(answerId, questionId);
        answer.Sources = new()
        {
            ["url"] = "https://example.com",
            ["title"] = "Example Source"
        };

        return answer;
    }

    public static List<Answer> CreateMultipleAnswers(
        Guid questionId,
        int answerCount = 3
    )
    {
        var answers = new List<Answer>();

        for (int i = 1; i <= answerCount; i++)
        {
            answers.Add(CreateAnswer(
                Guid.NewGuid(),
                questionId,
                content: $"Answer {i}",
                tokenCount: 100 + (i * 10)
            ));
        }

        return answers;
    }

    public static Answer CreateAnswerWithLongContent(
        Guid answerId,
        Guid questionId,
        int contentLength = 20000
    )
    {
        var answer = CreateAnswer(answerId, questionId);
        answer.Content = new string('b', contentLength);
        answer.TokenCount = contentLength / 4;

        return answer;
    }

    public static List<Answer> CreateAnswersWithVariedTokenCounts(
        Guid questionId,
        int baseTokenCount = 100,
        int increment = 50,
        int count = 5
    )
    {
        var answers = new List<Answer>();

        for (int i = 0; i < count; i++)
        {
            answers.Add(CreateAnswer(
                Guid.NewGuid(),
                questionId,
                content: $"Answer with {baseTokenCount + (i * increment)} tokens",
                tokenCount: baseTokenCount + (i * increment)
            ));
        }

        return answers;
    }

    public static Answer CreateMinimalAnswer(
        Guid answerId,
        Guid questionId
    ) => CreateAnswer(
            answerId,
            questionId,
            content: "Yes",
            tokenCount: 1
        );

}
