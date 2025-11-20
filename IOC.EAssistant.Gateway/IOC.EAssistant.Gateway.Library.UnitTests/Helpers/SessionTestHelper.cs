using IOC.EAssistant.Gateway.Library.Entities.Databases.EAssistant;

namespace IOC.EAssistant.Gateway.Library.UnitTests.Helpers;

public static class SessionTestHelper
{
    public static Session CreateSessionWithConversations(
        Guid sessionId,
        int conversationCount = 1,
        int questionsPerConversation = 1
    ) => new()
    {
        Id = sessionId,
        CreatedAt = DateTime.Now,
        Conversations = ConversationTestHelper.CreateMultipleConversations(
            sessionId,
            conversationCount,
            questionsPerConversation
        )
    };

    public static Session CreateEmptySession(Guid sessionId) => new()
    {
        Id = sessionId,
        CreatedAt = DateTime.Now,
        Conversations = []
    };

    public static Session CreateSessionWithNullConversations(Guid sessionId) => new()
    {
        Id = sessionId,
        CreatedAt = DateTime.Now,
        Conversations = null!
    };

    public static List<Session> CreateMultipleSessions(
        int sessionCount = 3,
        int conversationsPerSession = 2,
     int questionsPerConversation = 1
    ) => Enumerable.Range(1, sessionCount)
        .Select(_ => CreateSessionWithConversations(
            Guid.NewGuid(),
            conversationsPerSession,
            questionsPerConversation
        )).ToList();

    public static Session CreateSessionWithMixedConversations(Guid sessionId) => new()
    {
        Id = sessionId,
        CreatedAt = DateTime.Now,
        Conversations =
        [
            ConversationTestHelper.CreateConversationWithQuestions(
                Guid.NewGuid(),
                sessionId,
                questionCount: 3,
                title: "Active Conversation"
            ),
            ConversationTestHelper.CreateEmptyConversation(
                Guid.NewGuid(),
                sessionId,
                title: "Empty Conversation"),
            ConversationTestHelper.CreateConversationWithMixedQuestions(
                Guid.NewGuid(),
                sessionId,
                questionsWithAnswers: 2,
                questionsWithoutAnswers: 1
            ),
            ConversationTestHelper.CreateConversationWithQuestions(
                Guid.NewGuid(),
                sessionId,
                questionCount: 1,
                title: "Single Question Conversation"
            )
        ]
    };

    public static Session CreateSessionWithSpecificConversation(
        Guid sessionId,
        Conversation specificConversation,
        int additionalConversationCount = 0
    )
    {
        var conversations = new List<Conversation> { specificConversation };
        if (additionalConversationCount > 0)
        {
            conversations.AddRange(ConversationTestHelper.CreateMultipleConversations(
                sessionId,
                additionalConversationCount,
                questionsPerConversation: 1
            ));
        }

        return new()
        {
            Id = sessionId,
            CreatedAt = DateTime.Now,
            Conversations = conversations
        };
    }

    public static Session CreateLargeSession(
        Guid sessionId,
        int conversationCount = 50,
        int questionsPerConversation = 10
    ) => CreateSessionWithConversations(sessionId, conversationCount, questionsPerConversation);

    public static Session CreateSessionWithVariedTokenCounts(Guid sessionId, int conversationCount = 3) => new()
    {
        Id = sessionId,
        CreatedAt = DateTime.Now,
        Conversations = Enumerable.Range(1, conversationCount)
            .Select(i => ConversationTestHelper.CreateConversationWithVariedTokenCounts(
                Guid.NewGuid(),
                sessionId,
                questionCount: i + 1)
            ).ToList()
    };

    public static Session CreateSessionWithSequentialQuestions(
        Guid sessionId,
      int conversationCount = 3,
        int questionsPerConversation = 3
    ) => new()
    {
        Id = sessionId,
        CreatedAt = DateTime.Now,
        Conversations = Enumerable.Range(0, conversationCount)
            .Select(i =>
            {
                var startIndex = (i * questionsPerConversation) + 1;
                return ConversationTestHelper.CreateConversationWithSequentialQuestions(
                    Guid.NewGuid(),
                    sessionId,
                    startIndex,
                    questionsPerConversation
                );
            }).ToList()
    };

    public static Session CreateActiveUserSession(Guid sessionId) => new()
    {
        Id = sessionId,
        CreatedAt = DateTime.Now.AddHours(-1),
        Conversations =
        [
            ConversationTestHelper.CreateConversationWithQuestions(
                Guid.NewGuid(),
                sessionId,
                questionCount: 5,
                title: "Current Discussion"
            ),
            ConversationTestHelper.CreateExistingConversation(
                Guid.NewGuid(),
                sessionId,
                existingQuestionCount: 3
            )
        ]
    };
}
