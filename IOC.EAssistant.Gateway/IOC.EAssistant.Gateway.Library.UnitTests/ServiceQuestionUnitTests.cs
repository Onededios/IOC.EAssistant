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
public class ServiceQuestionUnitTests
{
    private Mock<ILogger<ServiceQuestion>> _mockLogger = null!;
    private Mock<IDatabaseEAssistantBase<Question>> _mockRepository = null!;
    private Mock<IServiceAnswer> _mockServiceAnswer = null!;
    private ServiceQuestion _service = null!;

    [TestInitialize]
    public void Setup()
    {
        _mockLogger = new Mock<ILogger<ServiceQuestion>>();
        _mockRepository = new Mock<IDatabaseEAssistantBase<Question>>();
        _mockServiceAnswer = new Mock<IServiceAnswer>();
        _service = new ServiceQuestion(_mockLogger.Object, _mockRepository.Object, _mockServiceAnswer.Object);
    }

    #region SaveAsync Tests

    [TestMethod]
    public async Task SaveAsync_WhenQuestionIsNew_ShouldSaveQuestionAndAnswer()
    {
        // Arrange
        var questionId = Guid.NewGuid();
        var conversationId = Guid.NewGuid();
        var question = QuestionTestHelper.CreateQuestionWithAnswer(questionId, conversationId);

        _mockRepository.Setup(r => r.GetAsync(questionId)).ReturnsAsync((Question?)null);
        _mockRepository.Setup(r => r.SaveAsync(question)).ReturnsAsync(1);

        var answerSaveResult = new OperationResult<bool>();
        answerSaveResult.AddResult(true);
        _mockServiceAnswer.Setup(s => s.SaveAsync(It.IsAny<Answer>())).ReturnsAsync(answerSaveResult);

        // Act
        var result = await _service.SaveAsync(question);

        // Assert
        Assert.IsNotNull(result);
        Assert.IsTrue(result.Result);
        Assert.IsFalse(result.HasErrors);

        _mockRepository.Verify(r => r.GetAsync(questionId), Times.Once);
        _mockRepository.Verify(r => r.SaveAsync(question), Times.Once);
        _mockServiceAnswer.Verify(s => s.SaveAsync(question.Answer), Times.Once);
    }

    [TestMethod]
    public async Task SaveAsync_WhenQuestionExists_ShouldOnlySaveAnswer()
    {
        // Arrange
        var questionId = Guid.NewGuid();
        var conversationId = Guid.NewGuid();
        var existingQuestion = QuestionTestHelper.CreateQuestionWithAnswer(questionId, conversationId);
        var questionWithNewAnswer = QuestionTestHelper.CreateQuestionWithAnswer(questionId, conversationId);

        _mockRepository.Setup(r => r.GetAsync(questionId)).ReturnsAsync(existingQuestion);

        var answerSaveResult = new OperationResult<bool>();
        answerSaveResult.AddResult(true);
        _mockServiceAnswer.Setup(s => s.SaveAsync(It.IsAny<Answer>())).ReturnsAsync(answerSaveResult);

        // Act
        var result = await _service.SaveAsync(questionWithNewAnswer);

        // Assert
        Assert.IsNotNull(result);
        Assert.IsTrue(result.Result);
        Assert.IsFalse(result.HasErrors);

        _mockRepository.Verify(r => r.GetAsync(questionId), Times.Once);
        _mockRepository.Verify(r => r.SaveAsync(It.IsAny<Question>()), Times.Never);
        _mockServiceAnswer.Verify(s => s.SaveAsync(questionWithNewAnswer.Answer), Times.Once);
    }

    [TestMethod]
    public async Task SaveAsync_WhenQuestionSaveFails_ShouldReturnFalse()
    {
        // Arrange
        var questionId = Guid.NewGuid();
        var conversationId = Guid.NewGuid();
        var question = QuestionTestHelper.CreateQuestionWithAnswer(questionId, conversationId);

        _mockRepository.Setup(r => r.GetAsync(questionId)).ReturnsAsync((Question?)null);
        _mockRepository.Setup(r => r.SaveAsync(question)).ReturnsAsync(0);

        // Act
        var result = await _service.SaveAsync(question);

        // Assert
        Assert.IsNotNull(result);
        Assert.IsFalse(result.Result);
        Assert.IsFalse(result.HasErrors);

        _mockRepository.Verify(r => r.SaveAsync(question), Times.Once);
        _mockServiceAnswer.Verify(s => s.SaveAsync(It.IsAny<Answer>()), Times.Never);
    }

    [TestMethod]
    public async Task SaveAsync_WhenAnswerSaveFails_ShouldReturnErrorResult()
    {
        // Arrange
        var questionId = Guid.NewGuid();
        var conversationId = Guid.NewGuid();
        var question = QuestionTestHelper.CreateQuestionWithAnswer(questionId, conversationId);

        _mockRepository.Setup(r => r.GetAsync(questionId)).ReturnsAsync((Question?)null);
        _mockRepository.Setup(r => r.SaveAsync(question)).ReturnsAsync(1);

        var answerSaveResult = new OperationResult<bool>();
        answerSaveResult.AddError(new ErrorResult("Failed to save answer", "Answer Save Error"));
        _mockServiceAnswer.Setup(s => s.SaveAsync(It.IsAny<Answer>())).ReturnsAsync(answerSaveResult);

        // Act
        var result = await _service.SaveAsync(question);

        // Assert
        Assert.IsNotNull(result);
        Assert.IsFalse(result.Result);
        Assert.IsTrue(result.HasErrors);
        Assert.IsTrue(result.Errors.Any());

        _mockRepository.Verify(r => r.SaveAsync(question), Times.Once);
        _mockServiceAnswer.Verify(s => s.SaveAsync(question.Answer), Times.Once);
    }

    [TestMethod]
    public async Task SaveAsync_WhenQuestionHasNullAnswer_ShouldSaveOnlyQuestion()
    {
        // Arrange
        var questionId = Guid.NewGuid();
        var conversationId = Guid.NewGuid();
        var question = new Question
        {
            Id = questionId,
            IdConversation = conversationId,
            Content = "Test question?",
            TokenCount = 50,
            Index = 1,
            CreatedAt = DateTime.Now,
            Answer = null!
        };

        _mockRepository.Setup(r => r.GetAsync(questionId)).ReturnsAsync((Question?)null);
        _mockRepository.Setup(r => r.SaveAsync(question)).ReturnsAsync(1);

        // Act
        var result = await _service.SaveAsync(question);

        // Assert
        Assert.IsNotNull(result);
        Assert.IsTrue(result.Result);
        Assert.IsFalse(result.HasErrors);

        _mockRepository.Verify(r => r.SaveAsync(question), Times.Once);
        _mockServiceAnswer.Verify(s => s.SaveAsync(It.IsAny<Answer>()), Times.Never);
    }

    [TestMethod]
    public async Task SaveAsync_WithMetadata_ShouldSaveSuccessfully()
    {
        // Arrange
        var questionId = Guid.NewGuid();
        var conversationId = Guid.NewGuid();
        var question = QuestionTestHelper.CreateQuestionWithAnswer(questionId, conversationId);
        question.Metadata = new System.Text.Json.Nodes.JsonObject { ["source"] = "web" };

        _mockRepository.Setup(r => r.GetAsync(questionId)).ReturnsAsync((Question?)null);
        _mockRepository.Setup(r => r.SaveAsync(question)).ReturnsAsync(1);

        var answerSaveResult = new OperationResult<bool>();
        answerSaveResult.AddResult(true);
        _mockServiceAnswer.Setup(s => s.SaveAsync(It.IsAny<Answer>())).ReturnsAsync(answerSaveResult);

        // Act
        var result = await _service.SaveAsync(question);

        // Assert
        Assert.IsNotNull(result);
        Assert.IsTrue(result.Result);
    }

    #endregion

    #region SaveMultipleAsync Tests

    [TestMethod]
    public async Task SaveMultipleAsync_WhenAllQuestionsAreNew_ShouldSaveAllWithAnswers()
    {
        // Arrange
        var questions = new List<Question>
        {
            QuestionTestHelper.CreateQuestionWithAnswer(Guid.NewGuid(), Guid.NewGuid()),
            QuestionTestHelper.CreateQuestionWithAnswer(Guid.NewGuid(), Guid.NewGuid()),
            QuestionTestHelper.CreateQuestionWithAnswer(Guid.NewGuid(), Guid.NewGuid())
        };

        foreach (var question in questions)
        {
            _mockRepository.Setup(r => r.GetAsync(question.Id)).ReturnsAsync((Question?)null);
        }

        _mockRepository.Setup(r => r.SaveMultipleAsync(It.Is<IEnumerable<Question>>(q => q.Count() == 3))).ReturnsAsync(3);

        var answersSaveResult = new OperationResult<bool>();
        answersSaveResult.AddResult(true);
        _mockServiceAnswer.Setup(s => s.SaveMultipleAsync(It.IsAny<IEnumerable<Answer>>())).ReturnsAsync(answersSaveResult);

        // Act
        var result = await _service.SaveMultipleAsync(questions);

        // Assert
        Assert.IsNotNull(result);
        Assert.IsTrue(result.Result);
        Assert.IsFalse(result.HasErrors);

        _mockRepository.Verify(r => r.SaveMultipleAsync(It.Is<IEnumerable<Question>>(q => q.Count() == 3)), Times.Once);
        _mockServiceAnswer.Verify(s => s.SaveMultipleAsync(It.Is<IEnumerable<Answer>>(a => a.Count() == 3)), Times.Once);
    }

    [TestMethod]
    public async Task SaveMultipleAsync_WhenAllQuestionsExist_ShouldOnlySaveNewAnswers()
    {
        // Arrange
        var questions = new List<Question>
        {
            QuestionTestHelper.CreateQuestionWithAnswer(Guid.NewGuid(), Guid.NewGuid()),
            QuestionTestHelper.CreateQuestionWithAnswer(Guid.NewGuid(), Guid.NewGuid())
        };

        foreach (var question in questions)
        {
            _mockRepository.Setup(r => r.GetAsync(question.Id)).ReturnsAsync(question);
        }

        var answersSaveResult = new OperationResult<bool>();
        answersSaveResult.AddResult(true);
        _mockServiceAnswer.Setup(s => s.SaveMultipleAsync(It.IsAny<IEnumerable<Answer>>())).ReturnsAsync(answersSaveResult);

        // Act
        var result = await _service.SaveMultipleAsync(questions);

        // Assert
        Assert.IsNotNull(result);
        Assert.IsTrue(result.Result);
        Assert.IsFalse(result.HasErrors);

        _mockRepository.Verify(r => r.SaveMultipleAsync(It.IsAny<IEnumerable<Question>>()), Times.Never);
        _mockServiceAnswer.Verify(s => s.SaveMultipleAsync(It.Is<IEnumerable<Answer>>(a => a.Count() == 2)), Times.Once);
    }

    [TestMethod]
    public async Task SaveMultipleAsync_WhenSomeQuestionsExist_ShouldSaveOnlyNewOnes()
    {
        // Arrange
        var existingQuestion = QuestionTestHelper.CreateQuestionWithAnswer(Guid.NewGuid(), Guid.NewGuid());
        var newQuestion1 = QuestionTestHelper.CreateQuestionWithAnswer(Guid.NewGuid(), Guid.NewGuid());
        var newQuestion2 = QuestionTestHelper.CreateQuestionWithAnswer(Guid.NewGuid(), Guid.NewGuid());

        var questions = new List<Question> { existingQuestion, newQuestion1, newQuestion2 };

        _mockRepository.Setup(r => r.GetAsync(existingQuestion.Id)).ReturnsAsync(existingQuestion);
        _mockRepository.Setup(r => r.GetAsync(newQuestion1.Id)).ReturnsAsync((Question?)null);
        _mockRepository.Setup(r => r.GetAsync(newQuestion2.Id)).ReturnsAsync((Question?)null);

        _mockRepository.Setup(r => r.SaveMultipleAsync(It.Is<IEnumerable<Question>>(q => q.Count() == 2))).ReturnsAsync(2);

        var answersSaveResult = new OperationResult<bool>();
        answersSaveResult.AddResult(true);
        _mockServiceAnswer.Setup(s => s.SaveMultipleAsync(It.IsAny<IEnumerable<Answer>>())).ReturnsAsync(answersSaveResult);

        // Act
        var result = await _service.SaveMultipleAsync(questions);

        // Assert
        Assert.IsNotNull(result);
        Assert.IsTrue(result.Result);
        Assert.IsFalse(result.HasErrors);

        _mockRepository.Verify(r => r.SaveMultipleAsync(It.Is<IEnumerable<Question>>(q =>
            q.Count() == 2 &&
            q.Contains(newQuestion1) &&
            q.Contains(newQuestion2)
        )), Times.Once);
        _mockServiceAnswer.Verify(s => s.SaveMultipleAsync(It.Is<IEnumerable<Answer>>(a => a.Count() == 3)), Times.Once);
    }

    [TestMethod]
    public async Task SaveMultipleAsync_WhenQuestionsSaveFails_ShouldReturnFalse()
    {
        // Arrange
        var questions = new List<Question>
        {
            QuestionTestHelper.CreateQuestionWithAnswer(Guid.NewGuid(), Guid.NewGuid())
        };

        _mockRepository.Setup(r => r.GetAsync(questions[0].Id)).ReturnsAsync((Question?)null);
        _mockRepository.Setup(r => r.SaveMultipleAsync(It.IsAny<IEnumerable<Question>>())).ReturnsAsync(0);

        // Act
        var result = await _service.SaveMultipleAsync(questions);

        // Assert
        Assert.IsNotNull(result);
        Assert.IsFalse(result.Result);
        Assert.IsFalse(result.HasErrors);

        _mockRepository.Verify(r => r.SaveMultipleAsync(It.IsAny<IEnumerable<Question>>()), Times.Once);
        _mockServiceAnswer.Verify(s => s.SaveMultipleAsync(It.IsAny<IEnumerable<Answer>>()), Times.Never);
    }

    [TestMethod]
    public async Task SaveMultipleAsync_WhenAnswersSaveFails_ShouldReturnErrorResult()
    {
        // Arrange
        var questions = new List<Question>
        {
            QuestionTestHelper.CreateQuestionWithAnswer(Guid.NewGuid(), Guid.NewGuid())
        };

        _mockRepository.Setup(r => r.GetAsync(questions[0].Id)).ReturnsAsync((Question?)null);
        _mockRepository.Setup(r => r.SaveMultipleAsync(It.IsAny<IEnumerable<Question>>())).ReturnsAsync(1);

        var answersSaveResult = new OperationResult<bool>();
        answersSaveResult.AddError(new ErrorResult("Failed to save answers", "Answer Save Error"));
        _mockServiceAnswer.Setup(s => s.SaveMultipleAsync(It.IsAny<IEnumerable<Answer>>())).ReturnsAsync(answersSaveResult);

        // Act
        var result = await _service.SaveMultipleAsync(questions);

        // Assert
        Assert.IsNotNull(result);
        Assert.IsFalse(result.Result);
        Assert.IsTrue(result.HasErrors);

        _mockServiceAnswer.Verify(s => s.SaveMultipleAsync(It.IsAny<IEnumerable<Answer>>()), Times.Once);
    }

    [TestMethod]
    public async Task SaveMultipleAsync_WhenAllExistAndNoAnswers_ShouldReturnTrueWithoutSaving()
    {
        // Arrange
        var question = new Question
        {
            Id = Guid.NewGuid(),
            IdConversation = Guid.NewGuid(),
            Content = "Test question?",
            TokenCount = 50,
            Index = 1,
            CreatedAt = DateTime.Now,
            Answer = null!
        };

        var questions = new List<Question> { question };

        _mockRepository.Setup(r => r.GetAsync(question.Id)).ReturnsAsync(question);

        // Act
        var result = await _service.SaveMultipleAsync(questions);

        // Assert
        Assert.IsNotNull(result);
        Assert.IsTrue(result.Result);
        Assert.IsFalse(result.HasErrors);

        _mockRepository.Verify(r => r.SaveMultipleAsync(It.IsAny<IEnumerable<Question>>()), Times.Never);
        _mockServiceAnswer.Verify(s => s.SaveMultipleAsync(It.IsAny<IEnumerable<Answer>>()), Times.Never);
    }

    [TestMethod]
    public async Task SaveMultipleAsync_WithEmptyList_ShouldReturnTrue()
    {
        // Arrange
        var questions = new List<Question>();

        // Act
        var result = await _service.SaveMultipleAsync(questions);

        // Assert
        Assert.IsNotNull(result);
        Assert.IsTrue(result.Result);
        Assert.IsFalse(result.HasErrors);

        _mockRepository.Verify(r => r.SaveMultipleAsync(It.IsAny<IEnumerable<Question>>()), Times.Never);
        _mockServiceAnswer.Verify(s => s.SaveMultipleAsync(It.IsAny<IEnumerable<Answer>>()), Times.Never);
    }

    [TestMethod]
    public async Task SaveMultipleAsync_WithMixedAnswersState_ShouldHandleCorrectly()
    {
        // Arrange
        var questionWithAnswer = QuestionTestHelper.CreateQuestionWithAnswer(Guid.NewGuid(), Guid.NewGuid());
        var questionWithoutAnswer = new Question
        {
            Id = Guid.NewGuid(),
            IdConversation = Guid.NewGuid(),
            Content = "Question without answer?",
            TokenCount = 50,
            Index = 2,
            CreatedAt = DateTime.Now,
            Answer = null!
        };

        var questions = new List<Question> { questionWithAnswer, questionWithoutAnswer };

        foreach (var question in questions)
        {
            _mockRepository.Setup(r => r.GetAsync(question.Id)).ReturnsAsync((Question?)null);
        }

        _mockRepository.Setup(r => r.SaveMultipleAsync(It.Is<IEnumerable<Question>>(q => q.Count() == 2))).ReturnsAsync(2);

        var answersSaveResult = new OperationResult<bool>();
        answersSaveResult.AddResult(true);
        _mockServiceAnswer.Setup(s => s.SaveMultipleAsync(It.IsAny<IEnumerable<Answer>>())).ReturnsAsync(answersSaveResult);

        // Act
        var result = await _service.SaveMultipleAsync(questions);

        // Assert
        Assert.IsNotNull(result);
        Assert.IsTrue(result.Result);
        Assert.IsFalse(result.HasErrors);

        // Only answers from questionWithAnswer should be saved
        _mockServiceAnswer.Verify(s => s.SaveMultipleAsync(It.Is<IEnumerable<Answer>>(a => a.Count() == 1)), Times.Once);
    }

    #endregion
}
