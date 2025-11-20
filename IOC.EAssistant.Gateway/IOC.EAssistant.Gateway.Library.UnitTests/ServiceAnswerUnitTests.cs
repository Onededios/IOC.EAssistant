using IOC.EAssistant.Gateway.Infrastructure.Contracts.Databases;
using IOC.EAssistant.Gateway.Library.Entities.Databases.EAssistant;
using IOC.EAssistant.Gateway.Library.Implementation.Services;
using IOC.EAssistant.Gateway.Library.UnitTests.Helpers;
using Microsoft.Extensions.Logging;
using Moq;

namespace IOC.EAssistant.Gateway.Library.UnitTests;

[TestClass]
public class ServiceAnswerUnitTests
{
    private Mock<ILogger<ServiceAnswer>> _mockLogger = null!;
    private Mock<IDatabaseEAssistantBase<Answer>> _mockRepository = null!;
    private ServiceAnswer _service = null!;

    [TestInitialize]
    public void Setup()
    {
        _mockLogger = new Mock<ILogger<ServiceAnswer>>();
        _mockRepository = new Mock<IDatabaseEAssistantBase<Answer>>();
        _service = new ServiceAnswer(_mockLogger.Object, _mockRepository.Object);
    }

    #region SaveAsync Tests

    [TestMethod]
    public async Task SaveAsync_WhenAnswerIsNew_ShouldSaveSuccessfully()
    {
        // Arrange
        var answerId = Guid.NewGuid();
        var questionId = Guid.NewGuid();
        var answer = AnswerTestHelper.CreateAnswer(answerId, questionId);

        _mockRepository.Setup(r => r.GetAsync(answerId)).ReturnsAsync((Answer?)null);
        _mockRepository.Setup(r => r.SaveAsync(answer)).ReturnsAsync(1);

        // Act
        var result = await _service.SaveAsync(answer);

        // Assert
        Assert.IsNotNull(result);
        Assert.IsTrue(result.Result);
        Assert.IsFalse(result.HasErrors);
        Assert.IsFalse(result.HasExceptions);

        _mockRepository.Verify(r => r.GetAsync(answerId), Times.Once);
        _mockRepository.Verify(r => r.SaveAsync(answer), Times.Once);
    }

    [TestMethod]
    public async Task SaveAsync_WhenAnswerAlreadyExists_ShouldSkipSaveAndReturnTrue()
    {
        // Arrange
        var answerId = Guid.NewGuid();
        var questionId = Guid.NewGuid();
        var existingAnswer = AnswerTestHelper.CreateAnswer(answerId, questionId, "Existing answer", 50);
        var newAnswer = AnswerTestHelper.CreateAnswer(answerId, questionId, "New answer content");

        _mockRepository.Setup(r => r.GetAsync(answerId)).ReturnsAsync(existingAnswer);

        // Act
        var result = await _service.SaveAsync(newAnswer);

        // Assert
        Assert.IsNotNull(result);
        Assert.IsTrue(result.Result);
        Assert.IsFalse(result.HasErrors);

        _mockRepository.Verify(r => r.GetAsync(answerId), Times.Once);
        _mockRepository.Verify(r => r.SaveAsync(It.IsAny<Answer>()), Times.Never);
    }

    [TestMethod]
    public async Task SaveAsync_WhenSaveFails_ShouldReturnFalse()
    {
        // Arrange
        var answerId = Guid.NewGuid();
        var questionId = Guid.NewGuid();
        var answer = AnswerTestHelper.CreateAnswer(answerId, questionId);

        _mockRepository.Setup(r => r.GetAsync(answerId)).ReturnsAsync((Answer?)null);
        _mockRepository.Setup(r => r.SaveAsync(answer)).ReturnsAsync(0);

        // Act
        var result = await _service.SaveAsync(answer);

        // Assert
        Assert.IsNotNull(result);
        Assert.IsFalse(result.Result);
        Assert.IsFalse(result.HasErrors);

        _mockRepository.Verify(r => r.GetAsync(answerId), Times.Once);
        _mockRepository.Verify(r => r.SaveAsync(answer), Times.Once);
    }

    [TestMethod]
    public async Task SaveAsync_WithMetadataAndSources_ShouldSaveSuccessfully()
    {
        // Arrange
        var answerId = Guid.NewGuid();
        var questionId = Guid.NewGuid();
        var answer = AnswerTestHelper.CreateCompleteAnswer(answerId, questionId);

        _mockRepository.Setup(r => r.GetAsync(answerId)).ReturnsAsync((Answer?)null);
        _mockRepository.Setup(r => r.SaveAsync(answer)).ReturnsAsync(1);

        // Act
        var result = await _service.SaveAsync(answer);

        // Assert
        Assert.IsNotNull(result);
        Assert.IsTrue(result.Result);
        Assert.IsFalse(result.HasErrors);

        _mockRepository.Verify(r => r.SaveAsync(answer), Times.Once);
    }

    #endregion

    #region SaveMultipleAsync Tests

    [TestMethod]
    public async Task SaveMultipleAsync_WhenAllAnswersAreNew_ShouldSaveAllSuccessfully()
    {
        // Arrange
        var questionId = Guid.NewGuid();
        var answers = AnswerTestHelper.CreateMultipleAnswers(questionId, 3);

        foreach (var answer in answers)
        {
            _mockRepository.Setup(r => r.GetAsync(answer.Id)).ReturnsAsync((Answer?)null);
        }

        _mockRepository.Setup(r => r.SaveMultipleAsync(It.Is<IEnumerable<Answer>>(a => a.Count() == 3))).ReturnsAsync(3);

        // Act
        var result = await _service.SaveMultipleAsync(answers);

        // Assert
        Assert.IsNotNull(result);
        Assert.IsTrue(result.Result);
        Assert.IsFalse(result.HasErrors);

        _mockRepository.Verify(r => r.SaveMultipleAsync(It.Is<IEnumerable<Answer>>(a => a.Count() == 3)), Times.Once);
    }

    [TestMethod]
    public async Task SaveMultipleAsync_WhenAllAnswersExist_ShouldSkipSaveAndReturnTrue()
    {
        // Arrange
        var questionId = Guid.NewGuid();
        var answers = AnswerTestHelper.CreateMultipleAnswers(questionId, 2);

        foreach (var answer in answers)
        {
            _mockRepository.Setup(r => r.GetAsync(answer.Id)).ReturnsAsync(answer);
        }

        // Act
        var result = await _service.SaveMultipleAsync(answers);

        // Assert
        Assert.IsNotNull(result);
        Assert.IsTrue(result.Result);
        Assert.IsFalse(result.HasErrors);

        _mockRepository.Verify(r => r.SaveMultipleAsync(It.IsAny<IEnumerable<Answer>>()), Times.Never);
    }

    [TestMethod]
    public async Task SaveMultipleAsync_WhenSomeAnswersExist_ShouldSaveOnlyNewOnes()
    {
        // Arrange
        var questionId = Guid.NewGuid();
        var existingAnswer = AnswerTestHelper.CreateAnswer(Guid.NewGuid(), questionId, "Existing Answer");
        var newAnswer1 = AnswerTestHelper.CreateAnswer(Guid.NewGuid(), questionId, "New Answer 1", 150);
        var newAnswer2 = AnswerTestHelper.CreateAnswer(Guid.NewGuid(), questionId, "New Answer 2", 200);

        var answers = new List<Answer> { existingAnswer, newAnswer1, newAnswer2 };

        _mockRepository.Setup(r => r.GetAsync(existingAnswer.Id)).ReturnsAsync(existingAnswer);
        _mockRepository.Setup(r => r.GetAsync(newAnswer1.Id)).ReturnsAsync((Answer?)null);
        _mockRepository.Setup(r => r.GetAsync(newAnswer2.Id)).ReturnsAsync((Answer?)null);
        _mockRepository.Setup(r => r.SaveMultipleAsync(It.Is<IEnumerable<Answer>>(a => a.Count() == 2))).ReturnsAsync(2);

        // Act
        var result = await _service.SaveMultipleAsync(answers);

        // Assert
        Assert.IsNotNull(result);
        Assert.IsTrue(result.Result);
        Assert.IsFalse(result.HasErrors);

        _mockRepository.Verify(r => r.SaveMultipleAsync(It.Is<IEnumerable<Answer>>(a =>
            a.Count() == 2 &&
            a.Contains(newAnswer1) &&
            a.Contains(newAnswer2)
        )), Times.Once);
    }

    [TestMethod]
    public async Task SaveMultipleAsync_WhenSaveFails_ShouldReturnFalse()
    {
        // Arrange
        var questionId = Guid.NewGuid();
        var answers = new List<Answer> { AnswerTestHelper.CreateAnswer(Guid.NewGuid(), questionId) };

        _mockRepository.Setup(r => r.GetAsync(answers[0].Id)).ReturnsAsync((Answer?)null);
        _mockRepository.Setup(r => r.SaveMultipleAsync(It.IsAny<IEnumerable<Answer>>())).ReturnsAsync(0);

        // Act
        var result = await _service.SaveMultipleAsync(answers);

        // Assert
        Assert.IsNotNull(result);
        Assert.IsFalse(result.Result);
        Assert.IsFalse(result.HasErrors);

        _mockRepository.Verify(r => r.SaveMultipleAsync(It.IsAny<IEnumerable<Answer>>()), Times.Once);
    }

    [TestMethod]
    public async Task SaveMultipleAsync_WithEmptyList_ShouldReturnTrueWithoutSaving()
    {
        // Arrange
        var answers = new List<Answer>();

        // Act
        var result = await _service.SaveMultipleAsync(answers);

        // Assert
        Assert.IsNotNull(result);
        Assert.IsTrue(result.Result);
        Assert.IsFalse(result.HasErrors);

        _mockRepository.Verify(r => r.SaveMultipleAsync(It.IsAny<IEnumerable<Answer>>()), Times.Never);
    }

    [TestMethod]
    public async Task SaveMultipleAsync_WithLargeDataset_ShouldHandleEfficiently()
    {
        // Arrange
        var questionId = Guid.NewGuid();
        var answers = AnswerTestHelper.CreateAnswersWithVariedTokenCounts(questionId, 100, 50, 100);

        foreach (var answer in answers)
        {
            _mockRepository.Setup(r => r.GetAsync(answer.Id)).ReturnsAsync((Answer?)null);
        }

        _mockRepository.Setup(r => r.SaveMultipleAsync(It.Is<IEnumerable<Answer>>(a => a.Count() == 100))).ReturnsAsync(100);

        // Act
        var result = await _service.SaveMultipleAsync(answers);

        // Assert
        Assert.IsNotNull(result);
        Assert.IsTrue(result.Result);
        Assert.IsFalse(result.HasErrors);

        _mockRepository.Verify(r => r.SaveMultipleAsync(It.Is<IEnumerable<Answer>>(a => a.Count() == 100)), Times.Once);
    }

    #endregion
}
