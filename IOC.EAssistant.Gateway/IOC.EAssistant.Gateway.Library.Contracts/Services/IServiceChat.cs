using IOC.EAssistant.Gateway.Library.Entities.Proxies.EAssistant;
using IOC.EAssistant.Gateway.XCutting.Results;

namespace IOC.EAssistant.Gateway.Library.Contracts.Services;
public interface IServiceChat
{
    Task<OperationResult<ChatResponse>> ChatAsync(ChatRequest request);
}
