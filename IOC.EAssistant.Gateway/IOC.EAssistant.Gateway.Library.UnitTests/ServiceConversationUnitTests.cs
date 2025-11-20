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
public class ServiceConversationUnitTests
{
    private Mock<ILogger<ServiceConversation>> _mockLogger = null!;
    private Mock<IDatabaseEAssistantBase<Conversation>> _mockRepository = null!;
    private Mock<IServiceQuestion> _mockServiceQuestion = null!;
    private ServiceConversation _service = null!;

    [TestInitialize]
    public void Setup()
    {
        _mockLogger = new Mock<ILogger<ServiceConversation>>();
        _mockRepository = new Mock<IDatabaseEAssistantBase<Conversation>>();
        _mockServiceQuestion = new Mock<IServiceQuestion>();

        _service = new ServiceConversation(
            _mockLogger.Object,
            _mockRepository.Object,
            _mockServiceQuestion.Object
        );
    }

    #region SaveAsync Tests

    [TestMethod]
    public async Task SaveAsync_WhenConversationIsNew_ShouldSaveConversationAndQuestions()
    {
        // Arrange
        var conversationId = Guid.NewGuid();
        var sessionId = Guid.NewGuid();
        var conversation = ConversationTestHelper.CreateConversationWithQuestions(conversationId, sessionId, 2);

        _mockRepository.Setup(r => r.GetAsync(conversationId)).ReturnsAsync((Conversation?)null);
        _mockRepository.Setup(r => r.SaveAsync(conversation)).ReturnsAsync(1);

        var questionsSaveResult = new OperationResult<bool>();
        questionsSaveResult.AddResult(true);
        _mockServiceQuestion.Setup(s => s.SaveMultipleAsync(It.IsAny<IEnumerable<Question>>())).ReturnsAsync(questionsSaveResult);

        // Act
        var result = await _service.SaveAsync(conversation);

        // Assert
        Assert.IsNotNull(result);
        Assert.IsTrue(result.Result);
        Assert.IsFalse(result.HasErrors);

        _mockRepository.Verify(r => r.GetAsync(conversationId), Times.Once);
        _mockRepository.Verify(r => r.SaveAsync(conversation), Times.Once);
        _mockServiceQuestion.Verify(s => s.SaveMultipleAsync(It.Is<IEnumerable<Question>>(q => q.Count() == 2)), Times.Once);
    }

    [TestMethod]
    public async Task SaveAsync_WhenConversationExists_ShouldOnlySaveQuestions()
    {
        // Arrange
        var conversationId = Guid.NewGuid();
        var sessionId = Guid.NewGuid();
        var existingConversation = ConversationTestHelper.CreateConversationWithQuestions(conversationId, sessionId, 1);
        var conversationWithNewQuestions = ConversationTestHelper.CreateConversationWithQuestions(conversationId, sessionId, 2);

        _mockRepository.Setup(r => r.GetAsync(conversationId)).ReturnsAsync(existingConversation);

        var questionsSaveResult = new OperationResult<bool>();
        questionsSaveResult.AddResult(true);
        _mockServiceQuestion.Setup(s => s.SaveMultipleAsync(It.IsAny<IEnumerable<Question>>())).ReturnsAsync(questionsSaveResult);

        // Act
        var result = await _service.SaveAsync(conversationWithNewQuestions);

        // Assert
        Assert.IsNotNull(result);
        Assert.IsTrue(result.Result);
        Assert.IsFalse(result.HasErrors);

        _mockRepository.Verify(r => r.GetAsync(conversationId), Times.Once);
        _mockRepository.Verify(r => r.SaveAsync(It.IsAny<Conversation>()), Times.Never);
        _mockServiceQuestion.Verify(s => s.SaveMultipleAsync(It.Is<IEnumerable<Question>>(q => q.Count() == 2)), Times.Once);
    }

    [TestMethod]
    public async Task SaveAsync_WhenConversationSaveFails_ShouldReturnFalse()
    {
        // Arrange
        var conversationId = Guid.NewGuid();
        var sessionId = Guid.NewGuid();
        var conversation = ConversationTestHelper.CreateConversationWithQuestions(conversationId, sessionId, 1);

        _mockRepository.Setup(r => r.GetAsync(conversationId)).ReturnsAsync((Conversation?)null);
        _mockRepository.Setup(r => r.SaveAsync(conversation)).ReturnsAsync(0);

        // Act
        var result = await _service.SaveAsync(conversation);

        // Assert
        Assert.IsNotNull(result);
        Assert.IsFalse(result.Result);
        Assert.IsFalse(result.HasErrors);

        _mockRepository.Verify(r => r.SaveAsync(conversation), Times.Once);
        _mockServiceQuestion.Verify(s => s.SaveMultipleAsync(It.IsAny<IEnumerable<Question>>()), Times.Never);
    }

    [TestMethod]
    public async Task SaveAsync_WhenQuestionsSaveFails_ShouldReturnErrorResult()
    {
        // Arrange
        var conversationId = Guid.NewGuid();
        var sessionId = Guid.NewGuid();
        var conversation = ConversationTestHelper.CreateConversationWithQuestions(conversationId, sessionId, 2);

        _mockRepository.Setup(r => r.GetAsync(conversationId)).ReturnsAsync((Conversation?)null);
        _mockRepository.Setup(r => r.SaveAsync(conversation)).ReturnsAsync(1);

        var questionsSaveResult = new OperationResult<bool>();
        questionsSaveResult.AddError(new ErrorResult("Failed to save questions", "Question Save Error"));
        _mockServiceQuestion.Setup(s => s.SaveMultipleAsync(It.IsAny<IEnumerable<Question>>())).ReturnsAsync(questionsSaveResult);

        // Act
        var result = await _service.SaveAsync(conversation);

        // Assert
        Assert.IsNotNull(result);
        Assert.IsFalse(result.Result);
        Assert.IsTrue(result.HasErrors);
        Assert.IsTrue(result.Errors.Any());

        _mockRepository.Verify(r => r.SaveAsync(conversation), Times.Once);
        _mockServiceQuestion.Verify(s => s.SaveMultipleAsync(It.IsAny<IEnumerable<Question>>()), Times.Once);
    }

    [TestMethod]
    public async Task SaveAsync_WhenConversationHasNoQuestions_ShouldSaveOnlyConversation()
    {
        // Arrange
        var conversationId = Guid.NewGuid();
        var sessionId = Guid.NewGuid();
        var conversation = new Conversation
        {
            Id = conversationId,
            IdSession = sessionId,
            CreatedAt = DateTime.Now,
            Questions = new List<Question>()
        };

        _mockRepository.Setup(r => r.GetAsync(conversationId)).ReturnsAsync((Conversation?)null);
        _mockRepository.Setup(r => r.SaveAsync(conversation)).ReturnsAsync(1);

        // Act
        var result = await _service.SaveAsync(conversation);

        // Assert
        Assert.IsNotNull(result);
        Assert.IsTrue(result.Result);
        Assert.IsFalse(result.HasErrors);

        _mockRepository.Verify(r => r.SaveAsync(conversation), Times.Once);
        _mockServiceQuestion.Verify(s => s.SaveMultipleAsync(It.IsAny<IEnumerable<Question>>()), Times.Never);
    }

    [TestMethod]
    public async Task SaveAsync_WhenConversationHasNullQuestions_ShouldSaveOnlyConversation()
    {
        // Arrange
        var conversationId = Guid.NewGuid();
        var sessionId = Guid.NewGuid();
        var conversation = ConversationTestHelper.CreateEmptyConversation(conversationId, sessionId);

        _mockRepository.Setup(r => r.GetAsync(conversationId)).ReturnsAsync((Conversation?)null);
        _mockRepository.Setup(r => r.SaveAsync(conversation)).ReturnsAsync(1);

        // Act
        var result = await _service.SaveAsync(conversation);

        // Assert
        Assert.IsNotNull(result);
        Assert.IsTrue(result.Result);
        Assert.IsFalse(result.HasErrors);

        _mockRepository.Verify(r => r.SaveAsync(conversation), Times.Once);
        _mockServiceQuestion.Verify(s => s.SaveMultipleAsync(It.IsAny<IEnumerable<Question>>()), Times.Never);
    }

    #endregion

    #region SaveMultipleAsync Tests

    [TestMethod]
    public async Task SaveMultipleAsync_WhenAllConversationsAreNew_ShouldSaveAllWithQuestions()
    {
        // Arrange
        var conversations = ConversationTestHelper.CreateMultipleConversations(
            Guid.NewGuid(),
            conversationCount: 3,
            questionsPerConversation: 2
        );

        foreach (var conv in conversations)
        {
            _mockRepository.Setup(r => r.GetAsync(conv.Id)).ReturnsAsync((Conversation?)null);
        }

        _mockRepository.Setup(r => r.SaveMultipleAsync(It.Is<IEnumerable<Conversation>>(c => c.Count() == 3))).ReturnsAsync(3);

        var questionsSaveResult = new OperationResult<bool>();
        questionsSaveResult.AddResult(true);
        _mockServiceQuestion.Setup(s => s.SaveMultipleAsync(It.IsAny<IEnumerable<Question>>())).ReturnsAsync(questionsSaveResult);

        // Act
        var result = await _service.SaveMultipleAsync(conversations);

        // Assert
        Assert.IsNotNull(result);
        Assert.IsTrue(result.Result);
        Assert.IsFalse(result.HasErrors);

        _mockRepository.Verify(r => r.SaveMultipleAsync(It.Is<IEnumerable<Conversation>>(c => c.Count() == 3)), Times.Once);
        _mockServiceQuestion.Verify(s => s.SaveMultipleAsync(It.Is<IEnumerable<Question>>(q => q.Count() == 6)), Times.Once);
    }

    [TestMethod]
    public async Task SaveMultipleAsync_WhenAllConversationsExist_ShouldOnlySaveNewQuestions()
    {
        // Arrange
        var conversations = ConversationTestHelper.CreateMultipleConversations(
            Guid.NewGuid(),
            conversationCount: 3,
            questionsPerConversation: 1
        );

        foreach (var conv in conversations)
        {
            _mockRepository.Setup(r => r.GetAsync(conv.Id)).ReturnsAsync(conv);
        }

        var questionsSaveResult = new OperationResult<bool>();
        questionsSaveResult.AddResult(true);
        _mockServiceQuestion.Setup(s => s.SaveMultipleAsync(It.IsAny<IEnumerable<Question>>())).ReturnsAsync(questionsSaveResult);

        // Act
        var result = await _service.SaveMultipleAsync(conversations);

        // Assert
        Assert.IsNotNull(result);
        Assert.IsTrue(result.Result);
        Assert.IsFalse(result.HasErrors);

        _mockRepository.Verify(r => r.SaveMultipleAsync(It.IsAny<IEnumerable<Conversation>>()), Times.Never);
        _mockServiceQuestion.Verify(s => s.SaveMultipleAsync(It.Is<IEnumerable<Question>>(q => q.Count() == 3)), Times.Once);
    }

    [TestMethod]
    public async Task SaveMultipleAsync_WhenSomeConversationsExist_ShouldSaveOnlyNewOnes()
    {
        // Arrange
        var existingConv = ConversationTestHelper.CreateConversationWithQuestions(Guid.NewGuid(), Guid.NewGuid(), 1);
        var newConv1 = ConversationTestHelper.CreateConversationWithQuestions(Guid.NewGuid(), Guid.NewGuid(), 2);
        var newConv2 = ConversationTestHelper.CreateConversationWithQuestions(Guid.NewGuid(), Guid.NewGuid(), 1);

        var conversations = new List<Conversation> { existingConv, newConv1, newConv2 };

        _mockRepository.Setup(r => r.GetAsync(existingConv.Id)).ReturnsAsync(existingConv);
        _mockRepository.Setup(r => r.GetAsync(newConv1.Id)).ReturnsAsync((Conversation?)null);
        _mockRepository.Setup(r => r.GetAsync(newConv2.Id)).ReturnsAsync((Conversation?)null);

        _mockRepository.Setup(r => r.SaveMultipleAsync(It.Is<IEnumerable<Conversation>>(c => c.Count() == 2)))
            .ReturnsAsync(2);

        var questionsSaveResult = new OperationResult<bool>();
        questionsSaveResult.AddResult(true);
        _mockServiceQuestion.Setup(s => s.SaveMultipleAsync(It.IsAny<IEnumerable<Question>>()))
            .ReturnsAsync(questionsSaveResult);

        // Act
        var result = await _service.SaveMultipleAsync(conversations);

        // Assert
        Assert.IsNotNull(result);
        Assert.IsTrue(result.Result);
        Assert.IsFalse(result.HasErrors);

        _mockRepository.Verify(r => r.SaveMultipleAsync(It.Is<IEnumerable<Conversation>>(c =>
            c.Count() == 2 &&
            c.Contains(newConv1) &&
            c.Contains(newConv2)
        )), Times.Once);
        _mockServiceQuestion.Verify(s => s.SaveMultipleAsync(It.Is<IEnumerable<Question>>(q => q.Count() == 4)), Times.Once);
    }

    [TestMethod]
    public async Task SaveMultipleAsync_WhenConversationsSaveFails_ShouldReturnFalse()
    {
        // Arrange
        var conversations = new List<Conversation>
        {
            ConversationTestHelper.CreateConversationWithQuestions(Guid.NewGuid(), Guid.NewGuid(), 1)
        };

        _mockRepository.Setup(r => r.GetAsync(conversations[0].Id)).ReturnsAsync((Conversation?)null);
        _mockRepository.Setup(r => r.SaveMultipleAsync(It.IsAny<IEnumerable<Conversation>>())).ReturnsAsync(0);

        // Act
        var result = await _service.SaveMultipleAsync(conversations);

        // Assert
        Assert.IsNotNull(result);
        Assert.IsFalse(result.Result);
        Assert.IsFalse(result.HasErrors);

        _mockRepository.Verify(r => r.SaveMultipleAsync(It.IsAny<IEnumerable<Conversation>>()), Times.Once);
        _mockServiceQuestion.Verify(s => s.SaveMultipleAsync(It.IsAny<IEnumerable<Question>>()), Times.Never);
    }

    [TestMethod]
    public async Task SaveMultipleAsync_WhenQuestionsSaveFails_ShouldReturnErrorResult()
    {
        // Arrange
        var conversations = new List<Conversation>
        {
            ConversationTestHelper.CreateConversationWithQuestions(Guid.NewGuid(), Guid.NewGuid(), 2)
        };

        _mockRepository.Setup(r => r.GetAsync(conversations[0].Id)).ReturnsAsync((Conversation?)null);
        _mockRepository.Setup(r => r.SaveMultipleAsync(It.IsAny<IEnumerable<Conversation>>())).ReturnsAsync(1);

        var questionsSaveResult = new OperationResult<bool>();
        questionsSaveResult.AddError(new ErrorResult("Failed to save questions", "Question Save Error"));
        _mockServiceQuestion.Setup(s => s.SaveMultipleAsync(It.IsAny<IEnumerable<Question>>())).ReturnsAsync(questionsSaveResult);

        // Act
        var result = await _service.SaveMultipleAsync(conversations);

        // Assert
        Assert.IsNotNull(result);
        Assert.IsFalse(result.Result);
        Assert.IsTrue(result.HasErrors);

        _mockServiceQuestion.Verify(s => s.SaveMultipleAsync(It.IsAny<IEnumerable<Question>>()), Times.Once);
    }

    [TestMethod]
    public async Task SaveMultipleAsync_WhenAllExistAndNoQuestions_ShouldReturnTrueWithoutSaving()
    {
        // Arrange
        var conversations = new List<Conversation>
        {
            ConversationTestHelper.CreateEmptyConversation(Guid.NewGuid(), Guid.NewGuid())
        };

        _mockRepository.Setup(r => r.GetAsync(conversations[0].Id)).ReturnsAsync(conversations[0]);

        // Act
        var result = await _service.SaveMultipleAsync(conversations);

        // Assert
        Assert.IsNotNull(result);
        Assert.IsTrue(result.Result);
        Assert.IsFalse(result.HasErrors);

        _mockRepository.Verify(r => r.SaveMultipleAsync(It.IsAny<IEnumerable<Conversation>>()), Times.Never);
        _mockServiceQuestion.Verify(s => s.SaveMultipleAsync(It.IsAny<IEnumerable<Question>>()), Times.Never);
    }

    [TestMethod]
    public async Task SaveMultipleAsync_WithEmptyList_ShouldReturnTrue()
    {
        // Arrange
        var conversations = new List<Conversation>();

        // Act
        var result = await _service.SaveMultipleAsync(conversations);

        // Assert
        Assert.IsNotNull(result);
        Assert.IsTrue(result.Result);
        Assert.IsFalse(result.HasErrors);

        _mockRepository.Verify(r => r.SaveMultipleAsync(It.IsAny<IEnumerable<Conversation>>()), Times.Never);
        _mockServiceQuestion.Verify(s => s.SaveMultipleAsync(It.IsAny<IEnumerable<Question>>()), Times.Never);
    }

    [TestMethod]
    public async Task SaveMultipleAsync_WithMixedQuestionsState_ShouldHandleCorrectly()
    {
        // Arrange
        var convWithQuestions = ConversationTestHelper.CreateConversationWithQuestions(Guid.NewGuid(), Guid.NewGuid(), 2);
        var convWithoutQuestions = ConversationTestHelper.CreateEmptyConversation(Guid.NewGuid(), Guid.NewGuid());
        var convWithNullQuestions = ConversationTestHelper.CreateEmptyConversation(Guid.NewGuid(), Guid.NewGuid());

        convWithNullQuestions.Questions = null!;

        var conversations = new List<Conversation> { convWithQuestions, convWithoutQuestions, convWithNullQuestions };

        foreach (var conv in conversations)
        {
            _mockRepository.Setup(r => r.GetAsync(conv.Id)).ReturnsAsync((Conversation?)null);
        }

        _mockRepository.Setup(r => r.SaveMultipleAsync(It.Is<IEnumerable<Conversation>>(c => c.Count() == 3))).ReturnsAsync(3);

        var questionsSaveResult = new OperationResult<bool>();
        questionsSaveResult.AddResult(true);
        _mockServiceQuestion.Setup(s => s.SaveMultipleAsync(It.IsAny<IEnumerable<Question>>())).ReturnsAsync(questionsSaveResult);

        // Act
        var result = await _service.SaveMultipleAsync(conversations);

        // Assert
        Assert.IsNotNull(result);
        Assert.IsTrue(result.Result);
        Assert.IsFalse(result.HasErrors);

        // Only questions from convWithQuestions should be saved
        _mockServiceQuestion.Verify(s => s.SaveMultipleAsync(It.Is<IEnumerable<Question>>(q => q.Count() == 2)), Times.Once);
    }

    #endregion
}
