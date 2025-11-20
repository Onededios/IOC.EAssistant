using IOC.EAssistant.Gateway.Library.Entities.Databases.EAssistant;

namespace IOC.EAssistant.Gateway.Library.UnitTests.Helpers;

public static class ConversationTestHelper
{
    public static Conversation CreateConversationWithQuestions(
        Guid conversationId,
        Guid sessionId,
        int questionCount = 1,
        string? title = null
    )
    {
        var questions = QuestionTestHelper.CreateMultipleQuestions(
            conversationId,
            questionCount,
            includeAnswers: true
        );

        return new()
        {
            Id = conversationId,
            IdSession = sessionId,
            Title = title,
            CreatedAt = DateTime.Now,
            Questions = questions
        };
    }

    public static Conversation CreateEmptyConversation(
        Guid conversationId,
        Guid sessionId,
        string? title = null
    ) => new()
    {
        Id = conversationId,
        IdSession = sessionId,
        Title = title,
        CreatedAt = DateTime.Now,
        Questions = new List<Question>()
    };
    public static List<Conversation> CreateMultipleConversations(
        Guid sessionId,
        int conversationCount = 3,
        int questionsPerConversation = 2
    )
    {
        var conversations = new List<Conversation>();

        for (int i = 1; i <= conversationCount; i++)
        {
            conversations.Add(CreateConversationWithQuestions(
                Guid.NewGuid(),
                sessionId,
                questionsPerConversation,
                $"Conversation {i}"
            ));
        }

        return conversations;
    }

    public static Conversation CreateConversationWithVariedTokenCounts(
        Guid conversationId,
        Guid sessionId,
        int questionCount = 3
    )
    {
        var questions = QuestionTestHelper.CreateQuestionsWithVariedTokenCounts(
            conversationId,
            baseTokenCount: 50,
            questionCount
        );

        return new()
        {
            Id = conversationId,
            IdSession = sessionId,
            CreatedAt = DateTime.Now,
            Questions = questions
        };
    }
    public static Conversation CreateConversationWithSequentialQuestions(
        Guid conversationId,
        Guid sessionId,
        int startIndex = 1,
        int questionCount = 5
    )
    {
        var questions = QuestionTestHelper.CreateQuestionsWithSequentialIndices(
            conversationId,
            startIndex,
            questionCount
        );

        return new()
        {
            Id = conversationId,
            IdSession = sessionId,
            CreatedAt = DateTime.Now,
            Questions = questions
        };
    }

    public static Conversation CreateConversationWithMixedQuestions(
        Guid conversationId,
        Guid sessionId,
        int questionsWithAnswers = 2,
        int questionsWithoutAnswers = 1
    )
    {
        var questions = new List<Question>();

        questions.AddRange(QuestionTestHelper.CreateMultipleQuestions(
            conversationId,
            questionsWithAnswers,
            includeAnswers: true
        ));

        var startIndex = questionsWithAnswers + 1;
        for (int i = 0; i < questionsWithoutAnswers; i++)
        {
            questions.Add(QuestionTestHelper.CreateQuestionWithoutAnswer(
                Guid.NewGuid(),
                conversationId,
                index: startIndex + i,
                questionText: $"Question {startIndex + i} without answer?"
            ));
        }

        return new()
        {
            Id = conversationId,
            IdSession = sessionId,
            CreatedAt = DateTime.Now,
            Questions = questions
        };
    }

    public static Conversation CreateExistingConversation(
        Guid conversationId,
        Guid sessionId,
        int existingQuestionCount = 3
    ) => CreateConversationWithQuestions(
            conversationId,
            sessionId,
            existingQuestionCount,
            $"Existing Conversation"
        );

    public static Conversation CreateConversationWithLongContent(
        Guid conversationId,
        Guid sessionId,
        int contentLength = 10000
    )
    {
        var question = QuestionTestHelper.CreateQuestionWithLongContent(
            Guid.NewGuid(),
            conversationId,
            contentLength
        );

        return new()
        {
            Id = conversationId,
            IdSession = sessionId,
            CreatedAt = DateTime.Now,
            Questions = new List<Question> { question }
        };
    }
}
