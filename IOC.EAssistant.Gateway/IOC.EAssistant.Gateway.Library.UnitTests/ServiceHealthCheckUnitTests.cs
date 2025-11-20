using IOC.EAssistant.Gateway.Infrastructure.Contracts.Proxies;
using IOC.EAssistant.Gateway.Infrastructure.Contracts.Proxies.EAssistant;
using IOC.EAssistant.Gateway.Library.Implementation.Services;
using Microsoft.Extensions.Logging;
using Moq;

namespace IOC.EAssistant.Gateway.Library.UnitTests;

[TestClass]
public class ServiceHealthCheckUnitTests
{
    private Mock<ILogger<ServiceHealthCheck>> _mockLogger = null!;
    private Mock<IProxyEAssistant> _mockProxyEAssistant = null!;
    private ServiceHealthCheck _service = null!;

    [TestInitialize]
    public void Setup()
    {
        _mockLogger = new Mock<ILogger<ServiceHealthCheck>>();
        _mockProxyEAssistant = new Mock<IProxyEAssistant>();
        _service = new ServiceHealthCheck(_mockLogger.Object, _mockProxyEAssistant.Object);
    }

    #region GetModelHealthAsync Tests

    [TestMethod]
    public async Task GetModelHealthAsync_WhenModelIsHealthy_ShouldReturnTrue()
    {
        // Arrange
        var healthResponse = new HealthResponse()
        {
            Status = "healthy",
            Model = "gpt-4",
            Timestamp = DateTime.Now
        };

        _mockProxyEAssistant.Setup(p => p.HealthCheckAsync()).ReturnsAsync(healthResponse);

        // Act
        var result = await _service.GetModelHealthAsync();

        // Assert
        Assert.IsNotNull(result);
        Assert.IsTrue(result.Result);
        Assert.IsFalse(result.HasErrors);
        Assert.IsFalse(result.HasExceptions);

        _mockProxyEAssistant.Verify(p => p.HealthCheckAsync(), Times.Once);
    }

    [TestMethod]
    public async Task GetModelHealthAsync_WhenModelIsUnhealthy_ShouldReturnFalse()
    {
        // Arrange
        var healthResponse = new HealthResponse
        {
            Status = "unhealthy",
            Model = "gpt-4",
            Timestamp = DateTime.Now
        };

        _mockProxyEAssistant.Setup(p => p.HealthCheckAsync()).ReturnsAsync(healthResponse);

        // Act
        var result = await _service.GetModelHealthAsync();

        // Assert
        Assert.IsNotNull(result);
        Assert.IsFalse(result.Result);
        Assert.IsFalse(result.HasErrors);

        _mockProxyEAssistant.Verify(p => p.HealthCheckAsync(), Times.Once);
    }

    [TestMethod]
    public async Task GetModelHealthAsync_WhenModelStatusIsEmpty_ShouldReturnFalse()
    {
        // Arrange
        var healthResponse = new HealthResponse
        {
            Status = "",
            Model = "gpt-4",
            Timestamp = DateTime.Now
        };

        _mockProxyEAssistant.Setup(p => p.HealthCheckAsync()).ReturnsAsync(healthResponse);

        // Act
        var result = await _service.GetModelHealthAsync();

        // Assert
        Assert.IsNotNull(result);
        Assert.IsFalse(result.Result);
    }

    [TestMethod]
    public async Task GetModelHealthAsync_WhenProxyThrowsHttpRequestException_ShouldPropagateException()
    {
        // Arrange
        _mockProxyEAssistant.Setup(p => p.HealthCheckAsync()).ThrowsAsync(new HttpRequestException("Connection failed"));

        // Act & Assert
        await Assert.ThrowsExceptionAsync<HttpRequestException>(_service.GetModelHealthAsync);

        _mockProxyEAssistant.Verify(p => p.HealthCheckAsync(), Times.Once);
    }

    [TestMethod]
    public async Task GetModelHealthAsync_WhenProxyThrowsTimeoutException_ShouldPropagateException()
    {
        // Arrange
        _mockProxyEAssistant.Setup(p => p.HealthCheckAsync()).ThrowsAsync(new TimeoutException("Health check timed out"));

        // Act & Assert
        await Assert.ThrowsExceptionAsync<TimeoutException>(_service.GetModelHealthAsync);
    }

    [TestMethod]
    public async Task GetModelHealthAsync_WhenProxyThrowsInvalidOperationException_ShouldPropagateException()
    {
        // Arrange
        _mockProxyEAssistant.Setup(p => p.HealthCheckAsync()).ThrowsAsync(new InvalidOperationException("Invalid response format"));

        // Act & Assert
        await Assert.ThrowsExceptionAsync<InvalidOperationException>(_service.GetModelHealthAsync);
    }

    #endregion

    #region GetHealthAsync Tests

    [TestMethod]
    public async Task GetHealthAsync_WhenModelIsHealthy_ShouldReturnHealthResponse()
    {
        // Arrange
        var proxyHealthResponse = new HealthResponse
        {
            Status = "healthy",
            Model = "gpt-4",
            Timestamp = DateTime.Now
        };

        _mockProxyEAssistant.Setup(p => p.HealthCheckAsync()).ReturnsAsync(proxyHealthResponse);

        // Act
        var result = await _service.GetHealthAsync();

        // Assert
        Assert.IsNotNull(result);
        Assert.IsNotNull(result.Result);
        Assert.IsTrue(result.Result.ModelAvailable);
        Assert.IsFalse(result.HasErrors);
        Assert.IsFalse(result.HasExceptions);

        _mockProxyEAssistant.Verify(p => p.HealthCheckAsync(), Times.Once);
    }

    [TestMethod]
    public async Task GetHealthAsync_WhenModelIsUnhealthy_ShouldReturnHealthResponseWithModelUnavailable()
    {
        // Arrange
        var proxyHealthResponse = new HealthResponse
        {
            Status = "unhealthy",
            Model = "gpt-4",
            Timestamp = DateTime.Now
        };

        _mockProxyEAssistant.Setup(p => p.HealthCheckAsync()).ReturnsAsync(proxyHealthResponse);

        // Act
        var result = await _service.GetHealthAsync();

        // Assert
        Assert.IsNotNull(result);
        Assert.IsNotNull(result.Result);
        Assert.IsFalse(result.Result.ModelAvailable);
        Assert.IsFalse(result.HasErrors);
    }

    [TestMethod]
    public async Task GetHealthAsync_WhenModelHealthCheckFails_ShouldReturnErrors()
    {
        // Arrange
        var proxyHealthResponse = new HealthResponse
        {
            Status = "error",
            Model = "gpt-4",
            Timestamp = DateTime.Now
        };

        _mockProxyEAssistant.Setup(p => p.HealthCheckAsync()).ReturnsAsync(proxyHealthResponse);

        // Act
        var result = await _service.GetHealthAsync();

        // Assert
        Assert.IsNotNull(result);
        Assert.IsNotNull(result.Result);
        Assert.IsFalse(result.Result.ModelAvailable);
    }

    [TestMethod]
    public async Task GetHealthAsync_WhenProxyThrowsException_ShouldPropagateException()
    {
        // Arrange
        _mockProxyEAssistant.Setup(p => p.HealthCheckAsync()).ThrowsAsync(new HttpRequestException("Network error"));

        // Act & Assert
        await Assert.ThrowsExceptionAsync<HttpRequestException>(_service.GetHealthAsync);
    }

    [TestMethod]
    public async Task GetHealthAsync_ShouldHaveTimestampInResponse()
    {
        // Arrange
        var proxyHealthResponse = new HealthResponse
        {
            Status = "healthy",
            Model = "gpt-4",
            Timestamp = DateTime.Now
        };

        _mockProxyEAssistant.Setup(p => p.HealthCheckAsync()).ReturnsAsync(proxyHealthResponse);

        // Act
        var result = await _service.GetHealthAsync();

        // Assert
        Assert.IsNotNull(result);
        Assert.IsNotNull(result.Result);
        Assert.AreNotEqual(default, result.Result.Timestamp);
    }

    #endregion
}
