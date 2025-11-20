using IOC.EAssistant.Gateway.Library.Entities.Databases.EAssistant;
using System.Text.Json.Nodes;

namespace IOC.EAssistant.Gateway.Library.UnitTests.Helpers;

public static class QuestionTestHelper
{
    public static Question CreateQuestionWithAnswer(
        Guid questionId,
        Guid conversationId,
        int index = 1,
        string questionText = "Test question?",
        string answerText = "Test answer content",
        int questionTokenCount = 50,
        int answerTokenCount = 100
    )
    {
        var answer = AnswerTestHelper.CreateAnswer(
            Guid.NewGuid(),
            questionId,
            answerText,
            answerTokenCount
        );

        return new()
        {
            Id = questionId,
            IdConversation = conversationId,
            Content = questionText,
            TokenCount = questionTokenCount,
            Index = index,
            CreatedAt = DateTime.Now,
            Answer = answer
        };
    }

    public static Question CreateQuestionWithCompleteAnswer(
        Guid questionId,
        Guid conversationId,
        int index = 1
    )
    {
        var answer = AnswerTestHelper.CreateCompleteAnswer(Guid.NewGuid(), questionId);

        return new()
        {
            Id = questionId,
            IdConversation = conversationId,
            Content = $"Question {index}?",
            TokenCount = 50,
            Index = index,
            CreatedAt = DateTime.Now,
            Answer = answer
        };
    }

    public static Question CreateQuestionWithoutAnswer(
        Guid questionId,
        Guid conversationId,
        int index = 1,
        string questionText = "Test question?",
        int tokenCount = 50
    ) => new()
    {
        Id = questionId,
        IdConversation = conversationId,
        Content = questionText,
        TokenCount = tokenCount,
        Index = index,
        CreatedAt = DateTime.Now,
        Answer = null!
    };

    public static Question CreateQuestionWithMetadata(
        Guid questionId,
        Guid conversationId,
        JsonObject? metadata = null
    )
    {
        var question = CreateQuestionWithAnswer(questionId, conversationId);
        question.Metadata = metadata ?? new JsonObject
        {
            ["source"] = "test",
            ["timestamp"] = DateTime.Now.ToString("o")
        };

        return question;
    }

    public static List<Question> CreateMultipleQuestions(
        Guid conversationId,
        int questionCount = 3,
        bool includeAnswers = true
    )
    {
        var questions = new List<Question>();

        for (int i = 1; i <= questionCount; i++)
        {
            var questionId = Guid.NewGuid();
            var question = includeAnswers ?
                CreateQuestionWithAnswer(
                    questionId,
                    conversationId,
                    index: i,
                    questionText: $"Question {i}?",
                    answerText: $"Answer {i}"
                ) :
                CreateQuestionWithoutAnswer(
                    questionId,
                    conversationId,
                    index: i,
                    questionText: $"Question {i}?"
                );

            questions.Add(question);
        }

        return questions;
    }

    public static Question CreateQuestionWithLongContent(
        Guid questionId,
        Guid conversationId,
        int contentLength = 10000
    )
    {
        var question = CreateQuestionWithAnswer(questionId, conversationId);
        question.Content = new string('a', contentLength);
        question.TokenCount = contentLength / 4;

        question.Answer = AnswerTestHelper.CreateAnswerWithLongContent(
            question.Answer.Id,
            questionId,
            contentLength * 2
        );

        return question;
    }
    public static List<Question> CreateQuestionsWithSequentialIndices(
        Guid conversationId,
        int startIndex = 1,
        int count = 5
    )
    {
        var questions = new List<Question>();

        for (int i = 0; i < count; i++)
        {
            var question = CreateQuestionWithAnswer(
                Guid.NewGuid(),
                conversationId,
                index: startIndex + i
            );
            questions.Add(question);
        }

        return questions;
    }

    public static List<Question> CreateQuestionsWithVariedTokenCounts(
        Guid conversationId,
        int baseTokenCount = 50,
        int questionCount = 5
    )
    {
        var questions = new List<Question>();

        for (int i = 1; i <= questionCount; i++)
        {
            var questionId = Guid.NewGuid();
            var question = CreateQuestionWithAnswer(
                questionId,
                conversationId,
                index: i,
                questionTokenCount: baseTokenCount + (i * 10),
                answerTokenCount: (baseTokenCount * 2) + (i * 15)
            );
            questions.Add(question);
        }

        return questions;
    }
}
