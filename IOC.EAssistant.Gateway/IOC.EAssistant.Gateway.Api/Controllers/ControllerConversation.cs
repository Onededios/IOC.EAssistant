using IOC.EAssistant.Gateway.Library.Contracts.Services;
using IOC.EAssistant.Gateway.Library.Entities.Databases.EAssistant;
using IOC.EAssistant.Gateway.XCutting.Results;
using Microsoft.AspNetCore.Mvc;

namespace IOC.EAssistant.Gateway.Api.Controllers;
[ApiController]
[Route("[controller]")]
public class ControllerConversation(IServiceConversation _serviceConversation) : ControllerBase
{

    [HttpGet]
    public async Task<OperationResult<IEnumerable<Conversation>>> Conversations(Guid sessionId)
    {
        var res = await _serviceConversation.GetConversations(sessionId);
        return res;
    }

    [HttpGet]
    public async Task<OperationResult<Conversation>> Conversation(Guid conversationId)
    {
        var res = await _serviceConversation.GetConversation(conversationId);
        return res;
    }
}
