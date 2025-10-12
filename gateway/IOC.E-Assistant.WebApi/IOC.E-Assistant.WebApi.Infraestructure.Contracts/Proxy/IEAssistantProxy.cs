using IOC.E_Assistant.WebApi.Library.Models.Entities;

namespace IOC.E_Assistant.Infrastructure.Contracts.Proxy;
public interface IEAssistantProxy
{
    Task<ChatResponse> ChatAsync(ChatRequest body);
}
