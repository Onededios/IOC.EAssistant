using IOC.EAssistant.Gateway.Infrastructure.Contracts.Proxies.EAssistant;
using IOC.EAssistant.Gateway.Library.Entities.Databases.EAssistant;
using IOC.EAssistant.Gateway.Library.Entities.Proxies.EAssistant;
using IOC.EAssistant.Gateway.Library.Entities.Proxies.EAssistant.Chat;

namespace IOC.EAssistant.Gateway.Library.Implementation.Mappers;

/// <summary>
/// Provides static mapping methods for transforming chat-related entities between different layers of the application.
/// </summary>
/// <remarks>
/// <para>
/// This mapper class serves as a central transformation hub for chat operations, converting between:
/// <list type="bullet">
/// <item><description>DTO objects used in the API layer</description></item>
/// <item><description>Proxy request/response objects for external AI service communication</description></item>
/// <item><description>Database entity objects for persistence</description></item>
/// </list>
/// </para>
/// <para>
/// All methods are static and designed to be stateless, providing pure transformation functions
/// without side effects. This design supports testability and maintains clear separation of concerns.
/// </para>
/// </remarks>
public static class ChatMapper
{
    /// <summary>
    /// Maps a collection of chat messages to a proxy chat request.
    /// </summary>
    /// <param name="messages">The collection of <see cref="ChatMessage"/> objects to include in the request.</param>
    /// <returns>A <see cref="ChatRequest"/> configured with the provided messages.</returns>
    /// <remarks>
    /// This method prepares the request format expected by the AI model proxy, encapsulating
    /// the message collection in a request wrapper object.
    /// </remarks>
    public static ChatRequest MapToProxyRequest(IEnumerable<ChatMessage> messages) => new ChatRequest { Messages = messages };

    /// <summary>
    /// Maps a model response to a DTO suitable for API responses, including session and conversation identifiers.
    /// </summary>
    /// <param name="modelResponse">The <see cref="ChatResponse"/> received from the AI model.</param>
    /// <param name="sessionId">The unique identifier of the session containing this interaction.</param>
    /// <param name="conversationId">The unique identifier of the conversation within the session.</param>
    /// <returns>
    /// A <see cref="ChatResponseDto"/> containing the model's choices, usage statistics,
    /// and the session/conversation identifiers for client tracking.
    /// </returns>
    /// <remarks>
    /// This method bridges the gap between the external AI service response and the application's
    /// API contract, adding contextual identifiers that clients need to maintain conversation state.
    /// </remarks>
    public static ChatResponseDto MapToResponseDto(ChatResponse modelResponse, Guid sessionId, Guid conversationId) =>
        new ChatResponseDto
        {
            Choices = modelResponse.Choices,
            Usage = modelResponse.Usage,
            IdSession = sessionId,
            IdConversation = conversationId
        };

    /// <summary>
    /// Maps a collection of question entities to chat messages for conversation history reconstruction.
    /// </summary>
    /// <param name="questions">The collection of <see cref="Question"/> entities from the database.</param>
    /// <returns>
    /// A list of <see cref="ChatMessage"/> objects representing the conversation history,
    /// including both questions and their corresponding answers.
    /// </returns>
    /// <remarks>
    /// <para>
    /// This method is used to reconstruct conversation context when continuing an existing conversation.
    /// It transforms database entities into the message format required by the AI model, preserving:
    /// <list type="bullet">
    /// <item><description>Message ordering through the Index property</description></item>
    /// <item><description>Question content (user input)</description></item>
    /// <item><description>Answer content (AI response)</description></item>
    /// <item><description>Metadata for additional context</description></item>
    /// </list>
    /// </para>
    /// <para>
    /// The resulting message collection can be sent to the AI model to provide conversation
    /// context, enabling coherent multi-turn interactions.
    /// </para>
    /// </remarks>
    public static List<ChatMessage> MapQuestionsToMessages(IEnumerable<Question> questions) =>
        questions.Select(q => new ChatMessage
        {
            Index = q.Index,
            Question = q.Content,
            Answer = q.Answer.Content,
            Metadata = q.Metadata
        }).ToList();

    /// <summary>
    /// Updates a chat message's index based on the current conversation history count.
    /// </summary>
    /// <param name="message">The <see cref="ChatMessage"/> to update.</param>
    /// <param name="currentCount">The number of existing messages in the conversation.</param>
    /// <remarks>
    /// This method ensures proper message ordering by calculating the next sequential index
    /// (currentCount + 1). Message ordering is critical for maintaining conversation context
    /// and enabling proper reconstruction of conversation flow.
    /// </remarks>
    public static void UpdateMessageIndex(ChatMessage message, int currentCount) => message.Index = currentCount + 1;

    /// <summary>
    /// Creates a new conversation entity linked to a specific session.
    /// </summary>
    /// <param name="sessionId">The unique identifier of the session that will contain this conversation.</param>
    /// <returns>
    /// A new <see cref="Conversation"/> entity with a generated ID and linked to the specified session.
    /// </returns>
    /// <remarks>
    /// This method creates an empty conversation shell that will be populated with questions
    /// as the chat interaction progresses. The conversation automatically receives a new GUID
    /// identifier and is associated with the parent session.
    /// </remarks>
    public static Conversation CreateConversationEntity(Guid sessionId) => new Conversation { IdSession = sessionId };

    /// <summary>
    /// Creates a new session entity containing the specified conversation.
    /// </summary>
    /// <param name="conversation">The <see cref="Conversation"/> to include in the new session.</param>
    /// <returns>
    /// A new <see cref="Session"/> entity with a generated ID containing the provided conversation
    /// in its Conversations collection.
    /// </returns>
    /// <remarks>
    /// This method is used when creating a new chat session. It initializes the session with
    /// a new GUID identifier and adds the first conversation to the session's collection,
    /// establishing the top-level container for the conversation hierarchy.
    /// </remarks>
    public static Session CreateSessionEntity(Conversation conversation) =>
        new Session
        {
            Id = Guid.NewGuid(),
            Conversations = new List<Conversation> { conversation }
        };

    /// <summary>
    /// Creates a complete question entity with its associated answer from a chat message and model response.
    /// </summary>
    /// <param name="message">The <see cref="ChatMessage"/> containing the user's question.</param>
    /// <param name="modelResult">The <see cref="ChatResponse"/> containing the AI-generated answer.</param>
    /// <param name="conversationId">The unique identifier of the conversation containing this question.</param>
    /// <returns>
    /// A <see cref="Question"/> entity with a nested <see cref="Answer"/> entity, both populated
    /// with content, metadata, token counts, and proper relationship identifiers.
    /// </returns>
    /// <remarks>
    /// <para>
    /// This method performs a complex transformation that:
    /// <list type="number">
    /// <item><description>Generates unique identifiers for both question and answer</description></item>
    /// <item><description>Extracts the answer content from the model's first choice</description></item>
    /// <item><description>Maps token usage (prompt tokens to question, completion tokens to answer)</description></item>
    /// <item><description>Preserves metadata from both the message and model response</description></item>
    /// <item><description>Establishes the parent-child relationship between question and answer</description></item>
    /// <item><description>Sets creation timestamps for both entities</description></item>
    /// <item><description>Links the question to its parent conversation</description></item>
    /// </list>
    /// </para>
    /// <para>
    /// The resulting entity graph is ready for persistence and maintains referential integrity
    /// through the IdQuestion and IdConversation foreign key relationships.
    /// </para>
    /// </remarks>
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
