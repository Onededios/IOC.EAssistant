using IOC.EAssistant.Gateway.Infrastructure.Contracts.Proxies.EAssistant;
using IOC.EAssistant.Gateway.Library.Entities.Databases.EAssistant;
using IOC.EAssistant.Gateway.Library.Entities.Proxies.EAssistant;
using IOC.EAssistant.Gateway.Library.Entities.Proxies.EAssistant.Chat;

namespace IOC.EAssistant.Gateway.Library.Implementation.Mappers;
public static class ChatMapper
{
    public static ChatRequest MapToProxyRequest(IEnumerable<ChatMessage> messages) => new ChatRequest { Messages = messages };

    public static ChatResponseDto MapToResponseDto(ChatResponse modelResponse, Guid sessionId, Guid conversationId) =>
        new ChatResponseDto
        {
            Choices = modelResponse.Choices,
            Usage = modelResponse.Usage,
            IdSession = sessionId,
            IdConversation = conversationId
        };

    public static List<ChatMessage> MapQuestionsToMessages(IEnumerable<Question> questions) =>
        questions.Select(q => new ChatMessage
        {
            Index = q.Index,
            Question = q.Content,
            Answer = q.Answer.Content,
            Metadata = q.Metadata
        }).ToList();

    public static void UpdateMessageIndex(ChatMessage message, int currentCount) => message.Index = currentCount + 1;

    public static Conversation CreateConversationEntity(Guid sessionId) => new Conversation { IdSession = sessionId };

    public static Session CreateSessionEntity(Conversation conversation) =>
        new Session
        {
            Id = Guid.NewGuid(),
            Conversations = new List<Conversation> { conversation }
        };

    public static Question CreateQuestionEntity(
        ChatMessage message,
        ChatResponse modelResult,
        Guid conversationId)
    {
        var questionId = Guid.NewGuid();

        var answer = new Answer
        {
            CreatedAt = DateTime.Now,
            IdQuestion = questionId,
            Id = Guid.NewGuid(),
            Content = modelResult.Choices.First().Message.Content,
            TokenCount = modelResult.Usage.CompletionTokens,
            Metadata = modelResult.Metadata
        };

        var question = new Question
        {
            CreatedAt = DateTime.Now,
            Id = questionId,
            Index = message.Index,
            IdConversation = conversationId,
            Content = message.Question,
            Metadata = message.Metadata,
            Answer = answer,
            TokenCount = modelResult.Usage.PromptTokens
        };

        return question;
    }
}
