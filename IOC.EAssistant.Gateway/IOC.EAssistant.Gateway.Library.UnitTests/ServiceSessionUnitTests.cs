using IOC.EAssistant.Gateway.Infrastructure.Contracts.Databases;
using IOC.EAssistant.Gateway.Library.Contracts.Services;
using IOC.EAssistant.Gateway.Library.Entities.Databases.EAssistant;
using IOC.EAssistant.Gateway.Library.Implementation.Services;
using IOC.EAssistant.Gateway.Library.UnitTests.Helpers;
using IOC.EAssistant.Gateway.XCutting.Results;
using Microsoft.Extensions.Logging;
using Moq;

namespace IOC.EAssistant.Gateway.Library.UnitTests;

[TestClass]
public class ServiceSessionUnitTests
{
    private Mock<ILogger<ServiceSession>> _mockLogger = null!;
    private Mock<IDatabaseEAssistantBase<Session>> _mockRepository = null!;
    private Mock<IServiceConversation> _mockServiceConversation = null!;
    private ServiceSession _service = null!;

    [TestInitialize]
    public void Setup()
    {
        _mockLogger = new Mock<ILogger<ServiceSession>>();
        _mockRepository = new Mock<IDatabaseEAssistantBase<Session>>();
        _mockServiceConversation = new Mock<IServiceConversation>();
        _service = new ServiceSession(_mockLogger.Object, _mockRepository.Object, _mockServiceConversation.Object);
    }

    #region SaveAsync Tests

    [TestMethod]
    public async Task SaveAsync_WhenSessionIsNew_ShouldSaveSessionAndConversations()
    {
        // Arrange
        var sessionId = Guid.NewGuid();
        var session = SessionTestHelper.CreateSessionWithConversations(sessionId, 2);

        _mockRepository.Setup(r => r.SaveAsync(session)).ReturnsAsync(1);

        var conversationsSaveResult = new OperationResult<bool>();
        conversationsSaveResult.AddResult(true);
        _mockServiceConversation.Setup(s => s.SaveMultipleAsync(It.IsAny<IEnumerable<Conversation>>())).ReturnsAsync(conversationsSaveResult);

        // Act
        var result = await _service.SaveAsync(session);

        // Assert
        Assert.IsNotNull(result);
        Assert.IsTrue(result.Result);
        Assert.IsFalse(result.HasErrors);

        _mockRepository.Verify(r => r.SaveAsync(session), Times.Once);
        _mockServiceConversation.Verify(s => s.SaveMultipleAsync(It.Is<IEnumerable<Conversation>>(c => c.Count() == 2)), Times.Once);
    }

    [TestMethod]
    public async Task SaveAsync_WhenSessionSaveFails_ShouldReturnFalse()
    {
        // Arrange
        var sessionId = Guid.NewGuid();
        var session = SessionTestHelper.CreateSessionWithConversations(sessionId, 1);

        _mockRepository.Setup(r => r.SaveAsync(session)).ReturnsAsync(0);

        // Act
        var result = await _service.SaveAsync(session);

        // Assert
        Assert.IsNotNull(result);
        Assert.IsFalse(result.Result);
        Assert.IsFalse(result.HasErrors);

        _mockRepository.Verify(r => r.SaveAsync(session), Times.Once);
        _mockServiceConversation.Verify(s => s.SaveMultipleAsync(It.IsAny<IEnumerable<Conversation>>()), Times.Never);
    }

    [TestMethod]
    public async Task SaveAsync_WhenConversationsSaveFails_ShouldReturnErrorResult()
    {
        // Arrange
        var sessionId = Guid.NewGuid();
        var session = SessionTestHelper.CreateSessionWithConversations(sessionId, 2);

        _mockRepository.Setup(r => r.SaveAsync(session)).ReturnsAsync(1);

        var conversationsSaveResult = new OperationResult<bool>();
        conversationsSaveResult.AddError(new ErrorResult("Failed to save conversations", "Conversation Save Error"));
        _mockServiceConversation.Setup(s => s.SaveMultipleAsync(It.IsAny<IEnumerable<Conversation>>())).ReturnsAsync(conversationsSaveResult);

        // Act
        var result = await _service.SaveAsync(session);

        // Assert
        Assert.IsNotNull(result);
        Assert.IsFalse(result.Result);
        Assert.IsTrue(result.HasErrors);
        Assert.IsTrue(result.Errors.Any());

        _mockRepository.Verify(r => r.SaveAsync(session), Times.Once);
        _mockServiceConversation.Verify(s => s.SaveMultipleAsync(It.IsAny<IEnumerable<Conversation>>()), Times.Once);
    }

    [TestMethod]
    public async Task SaveAsync_WhenSessionHasNoConversations_ShouldSaveOnlySession()
    {
        // Arrange
        var sessionId = Guid.NewGuid();
        var session = SessionTestHelper.CreateEmptySession(sessionId);

        _mockRepository.Setup(r => r.SaveAsync(session)).ReturnsAsync(1);

        // Act
        var result = await _service.SaveAsync(session);

        // Assert
        Assert.IsNotNull(result);
        Assert.IsTrue(result.Result);
        Assert.IsFalse(result.HasErrors);

        _mockRepository.Verify(r => r.SaveAsync(session), Times.Once);
        _mockServiceConversation.Verify(s => s.SaveMultipleAsync(It.IsAny<IEnumerable<Conversation>>()), Times.Never);
    }

    [TestMethod]
    public async Task SaveAsync_WhenSessionHasNullConversations_ShouldSaveOnlySession()
    {
        // Arrange
        var sessionId = Guid.NewGuid();
        var session = SessionTestHelper.CreateSessionWithNullConversations(sessionId);

        _mockRepository.Setup(r => r.SaveAsync(session)).ReturnsAsync(1);

        // Act
        var result = await _service.SaveAsync(session);

        // Assert
        Assert.IsNotNull(result);
        Assert.IsTrue(result.Result);
        Assert.IsFalse(result.HasErrors);

        _mockRepository.Verify(r => r.SaveAsync(session), Times.Once);
        _mockServiceConversation.Verify(s => s.SaveMultipleAsync(It.IsAny<IEnumerable<Conversation>>()), Times.Never);
    }

    [TestMethod]
    public async Task SaveAsync_WithMultipleConversations_ShouldSaveAll()
    {
        // Arrange
        var sessionId = Guid.NewGuid();
        var session = SessionTestHelper.CreateSessionWithConversations(sessionId, 5);

        _mockRepository.Setup(r => r.SaveAsync(session)).ReturnsAsync(1);

        var conversationsSaveResult = new OperationResult<bool>();
        conversationsSaveResult.AddResult(true);
        _mockServiceConversation.Setup(s => s.SaveMultipleAsync(It.IsAny<IEnumerable<Conversation>>())).ReturnsAsync(conversationsSaveResult);

        // Act
        var result = await _service.SaveAsync(session);

        // Assert
        Assert.IsNotNull(result);
        Assert.IsTrue(result.Result);

        _mockServiceConversation.Verify(s => s.SaveMultipleAsync(It.Is<IEnumerable<Conversation>>(c => c.Count() == 5)), Times.Once);
    }

    #endregion

    #region SaveMultipleAsync Tests

    [TestMethod]
    public async Task SaveMultipleAsync_WhenAllSessionsAreNew_ShouldSaveAllWithConversations()
    {
        // Arrange
        var sessions = new List<Session>
        {
            SessionTestHelper.CreateSessionWithConversations(Guid.NewGuid(), 2),
            SessionTestHelper.CreateSessionWithConversations(Guid.NewGuid(), 3),
            SessionTestHelper.CreateSessionWithConversations(Guid.NewGuid(), 1)
        };

        _mockRepository.Setup(r => r.SaveMultipleAsync(It.Is<IEnumerable<Session>>(s => s.Count() == 3))).ReturnsAsync(3);

        var conversationsSaveResult = new OperationResult<bool>();
        conversationsSaveResult.AddResult(true);
        _mockServiceConversation.Setup(s => s.SaveMultipleAsync(It.IsAny<IEnumerable<Conversation>>())).ReturnsAsync(conversationsSaveResult);

        // Act
        var result = await _service.SaveMultipleAsync(sessions);

        // Assert
        Assert.IsNotNull(result);
        Assert.IsTrue(result.Result);
        Assert.IsFalse(result.HasErrors);

        _mockRepository.Verify(r => r.SaveMultipleAsync(It.Is<IEnumerable<Session>>(s => s.Count() == 3)), Times.Once);
        _mockServiceConversation.Verify(s => s.SaveMultipleAsync(It.Is<IEnumerable<Conversation>>(c => c.Count() == 6)), Times.Once);
    }

    [TestMethod]
    public async Task SaveMultipleAsync_WhenSessionsSaveFails_ShouldReturnFalse()
    {
        // Arrange
        var sessions = new List<Session>
        {
            SessionTestHelper.CreateSessionWithConversations(Guid.NewGuid(), 1)
        };

        _mockRepository.Setup(r => r.SaveMultipleAsync(It.IsAny<IEnumerable<Session>>())).ReturnsAsync(0);

        // Act
        var result = await _service.SaveMultipleAsync(sessions);

        // Assert
        Assert.IsNotNull(result);
        Assert.IsFalse(result.Result);
        Assert.IsFalse(result.HasErrors);

        _mockRepository.Verify(r => r.SaveMultipleAsync(It.IsAny<IEnumerable<Session>>()), Times.Once);
        _mockServiceConversation.Verify(s => s.SaveMultipleAsync(It.IsAny<IEnumerable<Conversation>>()), Times.Never);
    }

    [TestMethod]
    public async Task SaveMultipleAsync_WhenConversationsSaveFails_ShouldReturnErrorResult()
    {
        // Arrange
        var sessions = new List<Session>
        {
            SessionTestHelper.CreateSessionWithConversations(Guid.NewGuid(), 2)
        };

        _mockRepository.Setup(r => r.SaveMultipleAsync(It.IsAny<IEnumerable<Session>>())).ReturnsAsync(1);

        var conversationsSaveResult = new OperationResult<bool>();
        conversationsSaveResult.AddError(new ErrorResult("Failed to save conversations", "Conversation Save Error"));
        _mockServiceConversation.Setup(s => s.SaveMultipleAsync(It.IsAny<IEnumerable<Conversation>>())).ReturnsAsync(conversationsSaveResult);

        // Act
        var result = await _service.SaveMultipleAsync(sessions);

        // Assert
        Assert.IsNotNull(result);
        Assert.IsFalse(result.Result);
        Assert.IsTrue(result.HasErrors);

        _mockServiceConversation.Verify(s => s.SaveMultipleAsync(It.IsAny<IEnumerable<Conversation>>()), Times.Once);
    }

    [TestMethod]
    public async Task SaveMultipleAsync_WhenSessionsHaveNoConversations_ShouldSaveOnlySessions()
    {
        // Arrange
        var sessions = new List<Session>
        {
            new Session { Id = Guid.NewGuid(), CreatedAt = DateTime.Now, Conversations = new List<Conversation>() },
            new Session { Id = Guid.NewGuid(), CreatedAt = DateTime.Now, Conversations = new List<Conversation>() }
        };

        _mockRepository.Setup(r => r.SaveMultipleAsync(It.Is<IEnumerable<Session>>(s => s.Count() == 2))).ReturnsAsync(2);

        // Act
        var result = await _service.SaveMultipleAsync(sessions);

        // Assert
        Assert.IsNotNull(result);
        Assert.IsTrue(result.Result);
        Assert.IsFalse(result.HasErrors);

        _mockRepository.Verify(r => r.SaveMultipleAsync(It.IsAny<IEnumerable<Session>>()), Times.Once);
        _mockServiceConversation.Verify(s => s.SaveMultipleAsync(It.IsAny<IEnumerable<Conversation>>()), Times.Never);
    }

    [TestMethod]
    public async Task SaveMultipleAsync_WithEmptyList_ShouldReturnFalse()
    {
        // Arrange
        var sessions = new List<Session>();

        _mockRepository.Setup(r => r.SaveMultipleAsync(It.IsAny<IEnumerable<Session>>())).ReturnsAsync(0);

        // Act
        var result = await _service.SaveMultipleAsync(sessions);

        // Assert
        Assert.IsNotNull(result);
        Assert.IsFalse(result.Result);

        _mockRepository.Verify(r => r.SaveMultipleAsync(It.IsAny<IEnumerable<Session>>()), Times.Once);
        _mockServiceConversation.Verify(s => s.SaveMultipleAsync(It.IsAny<IEnumerable<Conversation>>()), Times.Never);
    }

    [TestMethod]
    public async Task SaveMultipleAsync_WithMixedConversationsState_ShouldHandleCorrectly()
    {
        // Arrange
        var sessionWithConversations = SessionTestHelper.CreateSessionWithConversations(Guid.NewGuid(), 2);
        var sessionWithoutConversations = SessionTestHelper.CreateEmptySession(Guid.NewGuid());
        var sessionWithNullConversations = SessionTestHelper.CreateSessionWithNullConversations(Guid.NewGuid());

        var sessions = new List<Session> { sessionWithConversations, sessionWithoutConversations, sessionWithNullConversations };

        _mockRepository.Setup(r => r.SaveMultipleAsync(It.Is<IEnumerable<Session>>(s => s.Count() == 3))).ReturnsAsync(3);

        var conversationsSaveResult = new OperationResult<bool>();
        conversationsSaveResult.AddResult(true);
        _mockServiceConversation.Setup(s => s.SaveMultipleAsync(It.IsAny<IEnumerable<Conversation>>())).ReturnsAsync(conversationsSaveResult);

        // Act
        var result = await _service.SaveMultipleAsync(sessions);

        // Assert
        Assert.IsNotNull(result);
        Assert.IsTrue(result.Result);
        Assert.IsFalse(result.HasErrors);

        _mockServiceConversation.Verify(s => s.SaveMultipleAsync(It.Is<IEnumerable<Conversation>>(c => c.Count() == 2)), Times.Once);
    }

    [TestMethod]
    public async Task SaveMultipleAsync_WithLargeNumberOfSessions_ShouldHandleCorrectly()
    {
        // Arrange
        var sessions = Enumerable.Range(0, 50).Select(i => SessionTestHelper.CreateSessionWithConversations(Guid.NewGuid(), 2)).ToList();

        _mockRepository.Setup(r => r.SaveMultipleAsync(It.Is<IEnumerable<Session>>(s => s.Count() == 50))).ReturnsAsync(50);

        var conversationsSaveResult = new OperationResult<bool>();
        conversationsSaveResult.AddResult(true);
        _mockServiceConversation.Setup(s => s.SaveMultipleAsync(It.IsAny<IEnumerable<Conversation>>())).ReturnsAsync(conversationsSaveResult);

        // Act
        var result = await _service.SaveMultipleAsync(sessions);

        // Assert
        Assert.IsNotNull(result);
        Assert.IsTrue(result.Result);

        // 50 sessions * 2 conversations each = 100 conversations
        _mockServiceConversation.Verify(s => s.SaveMultipleAsync(It.Is<IEnumerable<Conversation>>(c => c.Count() == 100)), Times.Once);
    }

    #endregion
}
