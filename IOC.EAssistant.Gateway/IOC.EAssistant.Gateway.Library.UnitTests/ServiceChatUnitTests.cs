using IOC.EAssistant.Gateway.Infrastructure.Contracts.Proxies;
using IOC.EAssistant.Gateway.Infrastructure.Contracts.Proxies.EAssistant;
using IOC.EAssistant.Gateway.Library.Contracts.Services;
using IOC.EAssistant.Gateway.Library.Entities.Databases.EAssistant;
using IOC.EAssistant.Gateway.Library.Entities.Proxies.EAssistant;
using IOC.EAssistant.Gateway.Library.Entities.Proxies.EAssistant.Chat;
using IOC.EAssistant.Gateway.Library.Implementation.Services;
using IOC.EAssistant.Gateway.Library.Implementation.Validators;
using IOC.EAssistant.Gateway.Library.UnitTests.Helpers;
using IOC.EAssistant.Gateway.XCutting.Results;
using Microsoft.Extensions.Logging;
using Moq;
using System.Text.Json.Nodes;

namespace IOC.EAssistant.Gateway.Library.UnitTests;

[TestClass]
public class ServiceChatUnitTests
{
    private Mock<ILogger<ServiceChat>> _mockLogger = null!;
    private Mock<IProxyEAssistant> _mockProxyEAssistant = null!;
    private Mock<IServiceSession> _mockServiceSession = null!;
    private Mock<IServiceConversation> _mockServiceConversation = null!;
    private Mock<IServiceHealthCheck> _mockServiceHealthCheck = null!;
    private ValidatorChat _validatorChat = null!;
    private ServiceChat _service = null!;

    [TestInitialize]
    public void Setup()
    {
        _mockLogger = new Mock<ILogger<ServiceChat>>();
        _mockProxyEAssistant = new Mock<IProxyEAssistant>();
        _mockServiceSession = new Mock<IServiceSession>();
        _mockServiceConversation = new Mock<IServiceConversation>();
        _mockServiceHealthCheck = new Mock<IServiceHealthCheck>();
        _validatorChat = new ValidatorChat(Mock.Of<ILogger<ValidatorChat>>());

        _service = new ServiceChat(
            _mockLogger.Object,
            _mockProxyEAssistant.Object,
            _mockServiceSession.Object,
            _mockServiceConversation.Object,
            _mockServiceHealthCheck.Object,
            _validatorChat
        );
    }

    #region Validation Tests

    [TestMethod]
    public async Task ChatAsync_WhenRequestIsInvalid_ShouldReturnValidationErrors()
    {
        // Arrange
        var request = new ChatRequestDto { Messages = new List<ChatMessage>() };

        // Act
        var result = await _service.ChatAsync(request);

        // Assert
        Assert.IsNotNull(result);
        Assert.IsTrue(result.HasErrors);
        Assert.IsTrue(result.Errors.Any(e => e.Message.Contains("Messages")));
        Assert.IsNull(result.Result);

        _mockServiceHealthCheck.Verify(v => v.GetModelHealthAsync(), Times.Never);
    }

    [TestMethod]
    public async Task ChatAsync_WhenRequestHasEmptyQuestions_ShouldReturnValidationErrors()
    {
        // Arrange
        var request = new ChatRequestDto { Messages = new List<ChatMessage> { new ChatMessage { Index = 1, Question = "" } } };

        // Act
        var result = await _service.ChatAsync(request);

        // Assert
        Assert.IsTrue(result.HasErrors);
        Assert.IsTrue(result.Errors.Any());
    }

    #endregion

    #region Health Check Tests

    [TestMethod]
    public async Task ChatAsync_WhenModelIsUnhealthy_ShouldReturnError()
    {
        // Arrange
        var request = ChatTestHelper.CreateValidChatRequest();

        var healthCheckResult = new OperationResult<bool>();
        healthCheckResult.AddResult(false);

        _mockServiceHealthCheck.Setup(h => h.GetModelHealthAsync()).ReturnsAsync(healthCheckResult);

        // Act
        var result = await _service.ChatAsync(request);

        // Assert
        Assert.IsNotNull(result);
        Assert.IsTrue(result.HasErrors);
        Assert.IsTrue(result.Errors.Any(e => e.Message.Contains("not available")));
        Assert.IsNull(result.Result);

        _mockServiceHealthCheck.Verify(h => h.GetModelHealthAsync(), Times.Once);
        _mockProxyEAssistant.Verify(p => p.ChatAsync(It.IsAny<ChatRequest>()), Times.Never);
    }

    [TestMethod]
    public async Task ChatAsync_WhenModelIsHealthy_ShouldProceedWithRequest()
    {
        // Arrange
        var request = ChatTestHelper.CreateValidChatRequest();
        ChatTestHelper.SetupSuccessfulChatFlow(
            _mockServiceSession,
            _mockServiceConversation,
            _mockProxyEAssistant,
            _mockServiceHealthCheck
        );

        // Act
        var result = await _service.ChatAsync(request);

        // Assert
        Assert.IsNotNull(result);
        Assert.IsFalse(result.HasErrors);
        Assert.IsNotNull(result.Result);

        _mockServiceHealthCheck.Verify(h => h.GetModelHealthAsync(), Times.Once);
        _mockProxyEAssistant.Verify(p => p.ChatAsync(It.IsAny<ChatRequest>()), Times.Once);
    }

    #endregion

    #region Model Response Tests

    [TestMethod]
    public async Task ChatAsync_WhenModelResponseIsValid_ShouldReturnSuccessfully()
    {
        // Arrange
        var request = ChatTestHelper.CreateValidChatRequest();
        ChatTestHelper.SetupSuccessfulChatFlow(
            _mockServiceSession,
            _mockServiceConversation,
            _mockProxyEAssistant,
            _mockServiceHealthCheck
        );

        // Act
        var result = await _service.ChatAsync(request);

        // Assert
        Assert.IsNotNull(result);
        Assert.IsFalse(result.HasErrors);
        Assert.IsNotNull(result.Result);
        Assert.IsNotNull(result.Result.Choices);
        Assert.IsTrue(result.Result.Choices.Any());
        Assert.IsNotNull(result.Result.Usage);
        Assert.AreNotEqual(Guid.Empty, result.Result.IdSession);
        Assert.AreNotEqual(Guid.Empty, result.Result.IdConversation);
    }

    [TestMethod]
    public async Task ChatAsync_WhenModelResponseIsInvalid_ShouldReturnValidationErrors()
    {
        // Arrange
        var request = ChatTestHelper.CreateValidChatRequest();

        ChatTestHelper.SetupHealthyModel(_mockServiceHealthCheck);

        var chatResponse = new ChatResponse { Choices = new List<Choice>(), Usage = new Usage() };

        _mockProxyEAssistant.Setup(p => p.ChatAsync(It.IsAny<ChatRequest>())).ReturnsAsync(chatResponse);

        // Act
        var result = await _service.ChatAsync(request);

        // Assert
        Assert.IsTrue(result.HasErrors);
        Assert.IsTrue(result.Errors.Any(e => e.Message.Contains("choices")));
    }

    #endregion

    #region New Conversation Tests

    [TestMethod]
    public async Task ChatAsync_WhenCreatingNewConversation_ShouldCreateSessionAndConversation()
    {
        // Arrange
        var request = ChatTestHelper.CreateValidChatRequest();
        ChatTestHelper.SetupSuccessfulChatFlow(
            _mockServiceSession,
            _mockServiceConversation,
            _mockProxyEAssistant,
            _mockServiceHealthCheck
        );

        // Act
        var result = await _service.ChatAsync(request);

        // Assert
        Assert.IsNotNull(result);
        Assert.IsFalse(result.HasErrors);
        Assert.IsNotNull(result.Result);

        _mockServiceSession.Verify(s => s.SaveAsync(It.Is<Session>(session =>
            session.Conversations.Count == 1 &&
            session.Conversations[0].Questions.Count == 1
        )), Times.Once);
    }

    [TestMethod]
    public async Task ChatAsync_WhenCreatingNewConversationWithSessionId_ShouldUseProvidedSessionId()
    {
        // Arrange
        var sessionId = Guid.NewGuid();
        var request = ChatTestHelper.CreateValidChatRequest(sessionId: sessionId);

        ChatTestHelper.SetupHealthyModel(_mockServiceHealthCheck);
        ChatTestHelper.SetupValidModelResponse(_mockProxyEAssistant);

        var sessionResult = new OperationResult<Session>();
        sessionResult.AddResult(null!);

        _mockServiceSession.Setup(s => s.GetAsync(sessionId)).ReturnsAsync(sessionResult);

        var sessionSaveResult = new OperationResult<bool>();
        sessionSaveResult.AddResult(true);

        _mockServiceSession.Setup(s => s.SaveAsync(It.IsAny<Session>())).ReturnsAsync(sessionSaveResult);

        // Act
        var result = await _service.ChatAsync(request);

        // Assert
        Assert.IsNotNull(result);
        Assert.IsFalse(result.HasErrors);
        Assert.AreEqual(sessionId, result.Result!.IdSession);

        _mockServiceSession.Verify(s => s.SaveAsync(It.Is<Session>(session =>
            session.Id == sessionId &&
            session.Conversations.Count == 1 &&
            session.Conversations[0].Questions.Count == 1
        )), Times.Once);

        _mockServiceConversation.Verify(s => s.SaveAsync(It.IsAny<Conversation>()), Times.Never);
    }

    #endregion

    #region Existing Conversation Tests

    [TestMethod]
    public async Task ChatAsync_WhenContinuingExistingConversation_ShouldAppendToHistory()
    {
        // Arrange
        var conversationId = Guid.NewGuid();
        var sessionId = Guid.NewGuid();

        var existingConversation = ConversationTestHelper.CreateExistingConversation(conversationId, sessionId, 3);

        var request = ChatTestHelper.CreateValidChatRequest(sessionId: sessionId, conversationId: conversationId);

        ChatTestHelper.SetupHealthyModel(_mockServiceHealthCheck);

        var conversationResult = new OperationResult<Conversation>();
        conversationResult.AddResult(existingConversation);

        _mockServiceConversation.Setup(s => s.GetAsync(conversationId)).ReturnsAsync(conversationResult);

        ChatTestHelper.SetupValidModelResponse(_mockProxyEAssistant);

        var conversationSaveResult = new OperationResult<bool>();
        conversationSaveResult.AddResult(true);

        _mockServiceConversation.Setup(s => s.SaveAsync(It.IsAny<Conversation>())).ReturnsAsync(conversationSaveResult);

        // Act
        var result = await _service.ChatAsync(request);

        // Assert
        Assert.IsNotNull(result);
        Assert.IsFalse(result.HasErrors);
        Assert.AreEqual(conversationId, result.Result!.IdConversation);
        Assert.AreEqual(sessionId, result.Result.IdSession);

        _mockServiceConversation.Verify(s => s.GetAsync(conversationId), Times.Once);
        _mockServiceConversation.Verify(s => s.SaveAsync(It.Is<Conversation>(c =>
            c.Id == conversationId &&
            c.Questions.Count == 4
        )), Times.Once);
    }

    [TestMethod]
    public async Task ChatAsync_WhenConversationIdProvidedButNotFound_ShouldCreateNewConversation()
    {
        // Arrange
        var conversationId = Guid.NewGuid();
        var request = ChatTestHelper.CreateValidChatRequest(conversationId: conversationId);

        ChatTestHelper.SetupHealthyModel(_mockServiceHealthCheck);

        var conversationResult = new OperationResult<Conversation>();
        conversationResult.AddResult(null!);

        _mockServiceConversation.Setup(s => s.GetAsync(conversationId)).ReturnsAsync(conversationResult);

        ChatTestHelper.SetupValidModelResponse(_mockProxyEAssistant);

        var sessionResult = new OperationResult<Session>();
        sessionResult.AddResult(null!);

        _mockServiceSession.Setup(s => s.GetAsync(It.IsAny<Guid>())).ReturnsAsync(sessionResult);

        var sessionSaveResult = new OperationResult<bool>();
        sessionSaveResult.AddResult(true);

        _mockServiceSession.Setup(s => s.SaveAsync(It.IsAny<Session>())).ReturnsAsync(sessionSaveResult);

        // Act
        var result = await _service.ChatAsync(request);

        // Assert
        Assert.IsNotNull(result);
        Assert.IsFalse(result.HasErrors);
        Assert.IsNotNull(result.Result);

        _mockServiceSession.Verify(s => s.SaveAsync(It.IsAny<Session>()), Times.Once);
    }

    #endregion

    #region Session Management Tests

    [TestMethod]
    public async Task ChatAsync_WhenSessionExists_ShouldAddConversationToExistingSession()
    {
        // Arrange
        var sessionId = Guid.NewGuid();
        var request = ChatTestHelper.CreateValidChatRequest(sessionId: sessionId);

        ChatTestHelper.SetupHealthyModel(_mockServiceHealthCheck);
        ChatTestHelper.SetupValidModelResponse(_mockProxyEAssistant);

        var existingSession = SessionTestHelper.CreateEmptySession(sessionId);

        var sessionResult = new OperationResult<Session>();
        sessionResult.AddResult(existingSession);

        _mockServiceSession.Setup(s => s.GetAsync(sessionId)).ReturnsAsync(sessionResult);

        var conversationSaveResult = new OperationResult<bool>();
        conversationSaveResult.AddResult(true);

        _mockServiceConversation.Setup(s => s.SaveAsync(It.IsAny<Conversation>())).ReturnsAsync(conversationSaveResult);

        // Act
        var result = await _service.ChatAsync(request);

        // Assert
        Assert.IsNotNull(result);
        Assert.IsFalse(result.HasErrors);
        Assert.AreEqual(sessionId, result.Result!.IdSession);

        _mockServiceConversation.Verify(s => s.SaveAsync(It.IsAny<Conversation>()), Times.Once);
        _mockServiceSession.Verify(s => s.SaveAsync(It.IsAny<Session>()), Times.Never);
    }

    #endregion

    #region Persistence Error Tests

    [TestMethod]
    public async Task ChatAsync_WhenConversationSaveFails_ShouldReturnError()
    {
        // Arrange
        var request = ChatTestHelper.CreateValidChatRequest();

        ChatTestHelper.SetupHealthyModel(_mockServiceHealthCheck);
        ChatTestHelper.SetupValidModelResponse(_mockProxyEAssistant);

        var sessionResult = new OperationResult<Session>();
        sessionResult.AddResult(null!);

        _mockServiceSession.Setup(s => s.GetAsync(It.IsAny<Guid>())).ReturnsAsync(sessionResult);

        var sessionSaveResult = new OperationResult<bool>();
        sessionSaveResult.AddError(new ErrorResult("Database error", "Persistence"));

        _mockServiceSession.Setup(s => s.SaveAsync(It.IsAny<Session>())).ReturnsAsync(sessionSaveResult);

        // Act
        var result = await _service.ChatAsync(request);

        // Assert
        Assert.IsNotNull(result);
        Assert.IsTrue(result.HasErrors);
        Assert.IsTrue(result.Errors.Any(e => e.Message.Contains("Database error")));
    }

    [TestMethod]
    public async Task ChatAsync_WhenSessionSaveFails_ShouldReturnError()
    {
        // Arrange
        var sessionId = Guid.NewGuid();
        var request = ChatTestHelper.CreateValidChatRequest(sessionId: sessionId);

        ChatTestHelper.SetupHealthyModel(_mockServiceHealthCheck);
        ChatTestHelper.SetupValidModelResponse(_mockProxyEAssistant);

        var existingSession = SessionTestHelper.CreateEmptySession(sessionId);

        var sessionResult = new OperationResult<Session>();
        sessionResult.AddResult(existingSession);

        _mockServiceSession.Setup(s => s.GetAsync(sessionId)).ReturnsAsync(sessionResult);

        var conversationSaveResult = new OperationResult<bool>();
        conversationSaveResult.AddError(new ErrorResult("Failed to save conversation", "Persistence"));

        _mockServiceConversation.Setup(s => s.SaveAsync(It.IsAny<Conversation>())).ReturnsAsync(conversationSaveResult);

        // Act
        var result = await _service.ChatAsync(request);

        // Assert
        Assert.IsNotNull(result);
        Assert.IsTrue(result.HasErrors);
        Assert.IsTrue(result.Errors.Any(e => e.Message.Contains("Failed to save conversation")));
    }

    #endregion

    #region Message Index Tests

    [TestMethod]
    public async Task ChatAsync_WhenAddingToExistingConversation_ShouldIncrementMessageIndex()
    {
        // Arrange
        var conversationId = Guid.NewGuid();
        var sessionId = Guid.NewGuid();

        var existingConversation = ConversationTestHelper.CreateExistingConversation(conversationId, sessionId, 3);

        var request = ChatTestHelper.CreateValidChatRequest(
            sessionId: sessionId,
            conversationId: conversationId,
            questionText: "Fourth question?"
        );

        ChatTestHelper.SetupHealthyModel(_mockServiceHealthCheck);

        var conversationResult = new OperationResult<Conversation>();
        conversationResult.AddResult(existingConversation);

        _mockServiceConversation.Setup(s => s.GetAsync(conversationId)).ReturnsAsync(conversationResult);

        ChatTestHelper.SetupValidModelResponse(_mockProxyEAssistant);

        var conversationSaveResult = new OperationResult<bool>();
        conversationSaveResult.AddResult(true);

        _mockServiceConversation.Setup(s => s.SaveAsync(It.IsAny<Conversation>())).ReturnsAsync(conversationSaveResult);

        // Act
        var result = await _service.ChatAsync(request);

        // Assert
        Assert.IsNotNull(result);
        Assert.IsFalse(result.HasErrors);

        _mockServiceConversation.Verify(s => s.SaveAsync(It.Is<Conversation>(c =>
            c.Questions.Count == 4 &&
            c.Questions.Last().Index == 4
        )), Times.Once);
    }

    #endregion
}
