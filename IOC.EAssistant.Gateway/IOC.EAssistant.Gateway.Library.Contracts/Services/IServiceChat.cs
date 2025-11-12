using IOC.EAssistant.Gateway.Library.Entities.Proxies.EAssistant.Chat;
using IOC.EAssistant.Gateway.XCutting.Results;

namespace IOC.EAssistant.Gateway.Library.Contracts.Services;
public interface IServiceChat
{
    Task<OperationResult<ChatResponseDto>> ChatAsync(ChatRequestDto request);
}
