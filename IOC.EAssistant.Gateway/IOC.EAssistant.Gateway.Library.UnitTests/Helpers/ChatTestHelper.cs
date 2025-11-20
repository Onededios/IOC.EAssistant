using IOC.EAssistant.Gateway.Infrastructure.Contracts.Proxies;
using IOC.EAssistant.Gateway.Infrastructure.Contracts.Proxies.EAssistant;
using IOC.EAssistant.Gateway.Library.Contracts.Services;
using IOC.EAssistant.Gateway.Library.Entities.Databases.EAssistant;
using IOC.EAssistant.Gateway.Library.Entities.Proxies.EAssistant;
using IOC.EAssistant.Gateway.Library.Entities.Proxies.EAssistant.Chat;
using IOC.EAssistant.Gateway.XCutting.Results;
using Moq;

namespace IOC.EAssistant.Gateway.Library.UnitTests.Helpers;

public static class ChatTestHelper
{
    public static ChatRequestDto CreateValidChatRequest(
        Guid? sessionId = null,
        Guid? conversationId = null,
        string questionText = "What is the capital of France?",
        int messageIndex = 1
    ) => new ChatRequestDto
    {
        SessionId = sessionId,
        ConversationId = conversationId,
        Messages = new List<ChatMessage> { new() { Index = messageIndex, Question = questionText } }
    };

    public static ChatRequestDto CreateChatRequestWithMultipleMessages(
        Guid? sessionId = null,
        Guid? conversationId = null,
        int messageCount = 3
    )
    {
        var messages = new List<ChatMessage>();
        for (int i = 1; i <= messageCount; i++)
        {
            messages.Add(new() { Index = i, Question = $"Question {i}?" });
        }

        return new()
        {
            SessionId = sessionId,
            ConversationId = conversationId,
            Messages = messages
        };
    }

    public static void SetupHealthyModel(Mock<IServiceHealthCheck> mockServiceHealthCheck)
    {
        var healthResult = new OperationResult<bool>();
        healthResult.AddResult(true);

        mockServiceHealthCheck.Setup(h => h.GetModelHealthAsync()).ReturnsAsync(healthResult);
    }

    public static void SetupUnhealthyModel(Mock<IServiceHealthCheck> mockServiceHealthCheck)
    {
        var healthResult = new OperationResult<bool>();
        healthResult.AddResult(false);

        mockServiceHealthCheck.Setup(h => h.GetModelHealthAsync()).ReturnsAsync(healthResult);
    }

    public static void SetupValidModelResponse(
        Mock<IProxyEAssistant> mockProxyEAssistant,
        string answerContent = "Paris is the capital of France.",
        int promptTokens = 15,
        int completionTokens = 10
    )
    {
        var chatResponse = new ChatResponse
        {
            Choices = new List<Choice>
            {
                new ()
                {
                    Index = 0,
                    Message = new ChoiceMessage
                    {
                        Role = "assistant",
                        Content = answerContent
                    },
                    FinishReason = "stop"
                }
            },
            Usage = new()
            {
                PromptTokens = promptTokens,
                CompletionTokens = completionTokens,
                TotalTokens = promptTokens + completionTokens
            }
        };

        mockProxyEAssistant.Setup(p => p.ChatAsync(It.IsAny<ChatRequest>())).ReturnsAsync(chatResponse);
    }

    public static void SetupInvalidModelResponse(Mock<IProxyEAssistant> mockProxyEAssistant)
    {
        var chatResponse = new ChatResponse
        {
            Choices = new List<Choice>(),
            Usage = new() { PromptTokens = 0, CompletionTokens = 0, TotalTokens = 0 }
        };

        mockProxyEAssistant.Setup(p => p.ChatAsync(It.IsAny<ChatRequest>())).ReturnsAsync(chatResponse);
    }

    public static void SetupNoExistingSession(Mock<IServiceSession> mockServiceSession)
    {
        var sessionResult = new OperationResult<Session>();
        sessionResult.AddResult(null!);

        mockServiceSession.Setup(s => s.GetAsync(It.IsAny<Guid>())).ReturnsAsync(sessionResult);
    }

    public static void SetupSuccessfulSessionSave(Mock<IServiceSession> mockServiceSession)
    {
        var sessionSaveResult = new OperationResult<bool>();
        sessionSaveResult.AddResult(true);

        mockServiceSession.Setup(s => s.SaveAsync(It.IsAny<Session>())).ReturnsAsync(sessionSaveResult);
    }

    public static void SetupSessionWorkflow(Mock<IServiceSession> mockServiceSession)
    {
        SetupNoExistingSession(mockServiceSession);
        SetupSuccessfulSessionSave(mockServiceSession);
    }

    public static void SetupNoExistingConversation(Mock<IServiceConversation> mockServiceConversation)
    {
        var conversationResult = new OperationResult<Conversation>();
        conversationResult.AddResult(null!);

        mockServiceConversation.Setup(s => s.GetAsync(It.IsAny<Guid>())).ReturnsAsync(conversationResult);
    }

    public static void SetupSuccessfulConversationSave(Mock<IServiceConversation> mockServiceConversation)
    {
        var conversationSaveResult = new OperationResult<bool>();
        conversationSaveResult.AddResult(true);

        mockServiceConversation.Setup(s => s.SaveAsync(It.IsAny<Conversation>())).ReturnsAsync(conversationSaveResult);
    }

    public static void SetupConversationWorkflow(Mock<IServiceConversation> mockServiceConversation)
    {
        SetupNoExistingConversation(mockServiceConversation);
        SetupSuccessfulConversationSave(mockServiceConversation);
    }

    public static void SetupSuccessfulChatFlow(
        Mock<IServiceSession> mockServiceSession,
        Mock<IServiceConversation> mockServiceConversation,
        Mock<IProxyEAssistant> mockProxyEAssistant,
        Mock<IServiceHealthCheck> mockServiceHealthCheck
    )
    {
        SetupHealthyModel(mockServiceHealthCheck);
        SetupValidModelResponse(mockProxyEAssistant);
        SetupSessionWorkflow(mockServiceSession);
        SetupConversationWorkflow(mockServiceConversation);
    }

    public static Question CreateQuestion(int index, Guid? conversationId = null)
    {
        var questionId = Guid.NewGuid();
        var convId = conversationId ?? Guid.NewGuid();

        return new()
        {
            Id = questionId,
            IdConversation = convId,
            Content = $"Question {index}?",
            TokenCount = 50,
            Index = index,
            CreatedAt = DateTime.Now,
            Answer = new()
            {
                Id = Guid.NewGuid(),
                IdQuestion = questionId,
                Content = $"Answer {index}",
                TokenCount = 75,
                CreatedAt = DateTime.Now
            }
        };
    }
    public static Conversation CreateConversationWithHistory(
        Guid conversationId,
        Guid sessionId,
        int questionCount = 3
    )
    {
        var questions = new List<Question>();
        for (int i = 1; i <= questionCount; i++) questions.Add(CreateQuestion(i, conversationId));

        return new()
        {
            Id = conversationId,
            IdSession = sessionId,
            CreatedAt = DateTime.Now,
            Questions = questions
        };
    }
}
