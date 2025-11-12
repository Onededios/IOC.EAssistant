using IOC.EAssistant.Gateway.Infrastructure.Contracts.Proxies;
using IOC.EAssistant.Gateway.Library.Contracts.Services;
using IOC.EAssistant.Gateway.Library.Entities.Proxies.EAssistant.Chat;
using IOC.EAssistant.Gateway.XCutting.Results;
using Microsoft.Extensions.Logging;

namespace IOC.EAssistant.Gateway.Library.Implementation.Services;
public class ServiceChat(
    ILogger<ServiceChat> _logger,
    IProxyEAssistant _proxyEAssistant
) : IServiceChat
{
    public async Task<OperationResult<ChatResponseDto>> ChatAsync(ChatRequestDto request)
    {
        var operationResult = new OperationResult<ChatResponseDto>();
        return operationResult;
    }
}
